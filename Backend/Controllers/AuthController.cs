using System;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // POST: /api/Auth/register
        // Expects Authorization: Bearer <Clerk token>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Backend.Models.DTOs.RegisterDto? dto)
        {
            // If Clerk is configured, require Authorization header
            if (_authService.IsClerkConfigured)
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                    return BadRequest(new { message = "Missing Authorization Bearer token." });

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var principal = await _authService.VerifyTokenAsync(token);
                if (principal == null)
                    return Unauthorized(new { message = "Invalid clerk token." });

                // Extract common claims
                var sub = principal.FindFirst("sub")?.Value ?? principal.FindFirst("user_id")?.Value;
                var email = principal.FindFirst("email")?.Value ?? principal.FindFirst("preferred_username")?.Value;
                var name = principal.FindFirst("name")?.Value ?? principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { message = "Token missing required claims (sub,email)." });

                var user = await _authService.EnsureLocalUserAsync(sub, email, name ?? string.Empty);
                return Ok(new { data = new { token }, token, message = "Authenticated via Clerk" });
            }

            // Fallback: local register using JSON body
            if (dto == null)
                return BadRequest(new { message = "Missing registration data." });

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                var user = await _authService.RegisterLocalAsync(dto.FullName, dto.Email, dto.NIC, dto.Password, dto.ContactNumber);
                return Ok(new { message = "Registration successful", data = new { userId = user.UserId } });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        // POST: /api/Auth/login
        // Verifies Clerk token and returns local mapping
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Backend.Models.DTOs.LoginDto? dto)
        {
            // Clerk flow
            if (_authService.IsClerkConfigured)
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
                    return BadRequest(new { message = "Missing Authorization Bearer token." });

                var token = authHeader.Substring("Bearer ".Length).Trim();
                var principal = await _authService.VerifyTokenAsync(token);
                if (principal == null)
                    return Unauthorized(new { message = "Invalid clerk token." });

                var sub = principal.FindFirst("sub")?.Value ?? principal.FindFirst("user_id")?.Value;
                var email = principal.FindFirst("email")?.Value ?? principal.FindFirst("preferred_username")?.Value;
                var name = principal.FindFirst("name")?.Value ?? principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

                if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(email))
                    return BadRequest(new { message = "Token missing required claims (sub,email)." });

                var user = await _authService.EnsureLocalUserAsync(sub, email, name ?? string.Empty);

                return Ok(new { data = new { token }, token, message = "Logged in via Clerk" });
            }

            // Local login fallback using JSON email/password
            if (dto == null)
                return BadRequest(new { message = "Missing login data." });

            var userLocal = await _authService.LoginLocalAsync(dto.Email, dto.Password);
            if (userLocal == null)
                return Unauthorized(new { message = "Invalid email or password." });

            var jwt = _authService.GenerateJwt(userLocal);
            return Ok(new { data = new { token = jwt }, token = jwt, message = "Logged in (local)" });
        }

        // POST: /api/Auth/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] Backend.Models.DTOs.ForgotPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Invalid email format" });

            await _authService.SendPasswordResetEmailAsync(dto.Email);
            
            // Always return success to prevent email enumeration
            return Ok(new { message = "If the email exists, a password reset link has been sent" });
        }

        // POST: /api/Auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { message = "Validation failed", errors });
            }

            var success = await _authService.ResetPasswordAsync(dto.token, dto.newPassword);
            
            if (!success)
                return BadRequest(new { message = "Invalid or expired reset token" });
            
            return Ok(new { message = "Password reset successfully" });
        }
    }
}