using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Web;

namespace Backend.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpFactory;
        private readonly IEmailService _emailService;
        private readonly string? _jwksUrl;

        public AuthService(AppDbContext db, IHttpClientFactory httpFactory, IEmailService emailService)
        {
            _db = db;
            _httpFactory = httpFactory;
            _emailService = emailService;
            _jwksUrl = Environment.GetEnvironmentVariable("CLERK_JWKS_URL");
        }

        public bool IsClerkConfigured => !string.IsNullOrWhiteSpace(_jwksUrl);

        // Local password hashing (PBKDF2) used when Clerk isn't configured
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 100_000;

        public string HashPassword(string password)
        {
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var salt = new byte[SaltSize];
            rng.GetBytes(salt);
            var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
            var parts = new[] { Convert.ToBase64String(salt), Convert.ToBase64String(key) };
            return string.Join('.', parts);
        }

        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                var parts = hash.Split('.', 2);
                if (parts.Length != 2) return false;
                var salt = Convert.FromBase64String(parts[0]);
                var expected = Convert.FromBase64String(parts[1]);
                var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
                return CryptographicOperations.FixedTimeEquals(key, expected);
            }
            catch
            {
                return false;
            }
        }

        // Local register when Clerk is not configured
        public async Task<User> RegisterLocalAsync(string fullName, string email, string nic, string password, string? contactNumber = null)
        {
            var exists = await _db.Users.AnyAsync(u => u.Email == email);
            if (exists) throw new InvalidOperationException("User already exists");

            var user = new User
            {
                FullName = fullName,
                Email = email,
                NIC = nic ?? string.Empty,
                PasswordHash = HashPassword(password),
                Role = UserRole.Patient,
                ContactNumber = contactNumber
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<User?> LoginLocalAsync(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;
            if (string.IsNullOrEmpty(user.PasswordHash)) return null;
            return VerifyPassword(password, user.PasswordHash) ? user : null;
        }

        // Generate a local JWT for the user (used for local login fallback)
        public string GenerateJwt(User user)
        {
            var secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "dev_secret_change_me";
            // Ensure key material is at least 256 bits for HS256. If the provided secret is shorter,
            // derive a 32-byte key by hashing with SHA256 to satisfy the key-size requirement.
            var secretBytes = System.Text.Encoding.UTF8.GetBytes(secret);
            if (secretBytes.Length < 32)
            {
                secretBytes = SHA256.HashData(secretBytes);
            }
            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(secretBytes);
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Verify the incoming token (Clerk session or JWT) using JWKS
        public async Task<ClaimsPrincipal?> VerifyTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(_jwksUrl))
                return null;

            var http = _httpFactory.CreateClient();
            var jwksResp = await http.GetAsync(_jwksUrl!);
            if (!jwksResp.IsSuccessStatusCode)
                return null;

            var jwksJson = await jwksResp.Content.ReadAsStringAsync();
            var jwks = new JsonWebKeySet(jwksJson);

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = jwks.Keys,
                RequireExpirationTime = true,
                ValidateLifetime = true,
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        // Map Clerk user info into local user record. If user exists by ClerkUserId or email, update; otherwise create.
        public async Task<User> EnsureLocalUserAsync(string clerkUserId, string email, string fullName, string? contactNumber = null)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.ClerkUserId == clerkUserId || u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    FullName = string.IsNullOrWhiteSpace(fullName) ? email : fullName,
                    Email = email,
                    // NIC is required in the model; use a safe default when creating from Clerk
                    NIC = string.IsNullOrWhiteSpace(contactNumber) ? "UNKNOWN" : contactNumber.Substring(0, Math.Min(12, contactNumber.Length)),
                    PasswordHash = string.Empty,
                    Role = UserRole.Patient,
                    ContactNumber = contactNumber,
                    ClerkUserId = clerkUserId
                };
                _db.Users.Add(user);
            }
            else
            {
                user.ClerkUserId ??= clerkUserId;
                if (!string.IsNullOrWhiteSpace(fullName)) user.FullName = fullName;
                if (!string.IsNullOrWhiteSpace(contactNumber)) user.ContactNumber = contactNumber;
            }

            await _db.SaveChangesAsync();
            return user;
        }

        // Generate password reset token and send email
        public async Task<bool> SendPasswordResetEmailAsync(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return false;

            var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpiresUtc = DateTime.UtcNow.AddHours(1);
            
            await _db.SaveChangesAsync();
            await _emailService.SendPasswordResetEmailAsync(email, token);
            return true;
        }

        // Reset password using token
        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            // Try URL decoding the token in case it was encoded
            var decodedToken = HttpUtility.UrlDecode(token);
            
            Console.WriteLine($"Original token: {token}");
            Console.WriteLine($"Decoded token: {decodedToken}");
            Console.WriteLine($"Current UTC time: {DateTime.UtcNow}");
            
            // Try both original and decoded token
            var user = await _db.Users.FirstOrDefaultAsync(u => 
                (u.PasswordResetToken == token || u.PasswordResetToken == decodedToken) && 
                u.PasswordResetTokenExpiresUtc > DateTime.UtcNow);
            
            if (user == null) 
            {
                Console.WriteLine("No user found with valid token");
                return false;
            }

            Console.WriteLine($"Found user: {user.Email}, Token expires: {user.PasswordResetTokenExpiresUtc}");

            user.PasswordHash = HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiresUtc = null;
            
            await _db.SaveChangesAsync();
            return true;
        }
    }
}