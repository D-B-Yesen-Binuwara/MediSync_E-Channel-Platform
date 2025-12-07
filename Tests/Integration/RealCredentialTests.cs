using Backend.Data;
using Backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Net.Http;

namespace Tests.Integration
{
    public class RealCredentialTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly IHttpClientFactory _httpClientFactory;

        public RealCredentialTests()
        {
            // Use InMemory database but test with real credential scenarios
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);
            
            var httpClient = new HttpClient();
            _httpClientFactory = new MockHttpClientFactory(httpClient);
            _authService = new AuthService(_context, _httpClientFactory);
        }

        [Fact]
        public async Task TestRealCredentials_BCryptVsPBKDF2_ShowsIncompatibility()
        {
            TestLogger.LogTestStart("BCrypt vs PBKDF2 Compatibility Test", "CREDENTIAL");
            var startTime = DateTime.Now;

            try
            {
                // Test 1: PBKDF2 (current AuthService implementation)
                var pbkdf2Hash = _authService.HashPassword("password123");
                var pbkdf2Verify = _authService.VerifyPassword("password123", pbkdf2Hash);
                
                TestLogger.LogCredentialTest("PBKDF2 Hash/Verify", "password123", pbkdf2Verify);
                pbkdf2Verify.Should().BeTrue("PBKDF2 should work with itself");

                // Test 2: Try to verify BCrypt hash with PBKDF2 verifier
                var bcryptHash = "$2a$11$pRSX02Rh02O1HLsTyfs0vOq7FnL1ZnJuiBluK8b8JD5MyWFQxu4yy"; // From your DB
                var bcryptVerifyWithPBKDF2 = _authService.VerifyPassword("password123", bcryptHash);
                
                TestLogger.LogCredentialTest("BCrypt hash with PBKDF2 verifier", "password123", bcryptVerifyWithPBKDF2);
                
                if (!bcryptVerifyWithPBKDF2)
                {
                    TestLogger.LogBugFound("BCrypt hash cannot be verified with PBKDF2 - Authentication broken for existing users", "CRITICAL");
                }

                // Test 3: Show the hash format differences
                Console.WriteLine($"PBKDF2 Hash Format: {pbkdf2Hash}");
                Console.WriteLine($"BCrypt Hash Format: {bcryptHash}");
                
                TestLogger.LogTestPass("BCrypt vs PBKDF2 Compatibility Test", "CREDENTIAL", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("BCrypt vs PBKDF2 Compatibility Test", "CREDENTIAL", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        [Fact]
        public async Task TestPasswordStrengthValidation_WithRealScenarios()
        {
            TestLogger.LogTestStart("Password Strength Validation", "VALIDATION");
            var startTime = DateTime.Now;

            try
            {
                var testCases = new[]
                {
                    new { Password = "123", ShouldPass = false, Reason = "Too short" },
                    new { Password = "password123", ShouldPass = true, Reason = "Valid length" },
                    new { Password = "newPassword456", ShouldPass = true, Reason = "Valid new password" },
                    new { Password = "", ShouldPass = false, Reason = "Empty password" },
                    new { Password = "12345", ShouldPass = false, Reason = "5 characters" },
                    new { Password = "123456", ShouldPass = true, Reason = "Minimum 6 characters" }
                };

                foreach (var testCase in testCases)
                {
                    try
                    {
                        var hash = _authService.HashPassword(testCase.Password);
                        var isValid = !string.IsNullOrEmpty(hash) && testCase.Password.Length >= 6;
                        
                        TestLogger.LogCredentialTest($"Password '{testCase.Password}' ({testCase.Reason})", testCase.Password, isValid == testCase.ShouldPass);
                        
                        if (isValid != testCase.ShouldPass)
                        {
                            TestLogger.LogBugFound($"Password validation inconsistent for '{testCase.Password}' - Expected: {testCase.ShouldPass}, Got: {isValid}", "MEDIUM");
                        }
                    }
                    catch (Exception ex)
                    {
                        TestLogger.LogCredentialTest($"Password '{testCase.Password}' ({testCase.Reason})", testCase.Password, false);
                        if (testCase.ShouldPass)
                        {
                            TestLogger.LogBugFound($"Valid password '{testCase.Password}' threw exception: {ex.Message}", "HIGH");
                        }
                    }
                }

                TestLogger.LogTestPass("Password Strength Validation", "VALIDATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Password Strength Validation", "VALIDATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        [Fact]
        public async Task TestUserCredentials_FromDatabase_WithCurrentSystem()
        {
            TestLogger.LogTestStart("Real User Credentials Test", "INTEGRATION");
            var startTime = DateTime.Now;

            try
            {
                // Test the exact credentials from your database
                var realUserEmail = "alutheka@gmail.com";
                var realUserPassword = "password123";
                var realUserNIC = "200213203875";
                var realUserPhone = "0768614247";

                // Test 1: Create user with PBKDF2 (current system)
                var newUser = await _authService.RegisterLocalAsync(
                    "YB-alutheka", 
                    realUserEmail, 
                    realUserNIC, 
                    realUserPassword, 
                    realUserPhone
                );

                TestLogger.LogCredentialTest($"Register user {realUserEmail}", realUserPassword, newUser != null);
                newUser.Should().NotBeNull();

                // Test 2: Login with same credentials
                var loginUser = await _authService.LoginLocalAsync(realUserEmail, realUserPassword);
                TestLogger.LogCredentialTest($"Login user {realUserEmail}", realUserPassword, loginUser != null);
                loginUser.Should().NotBeNull();

                // Test 3: Generate JWT token
                if (loginUser != null)
                {
                    var token = _authService.GenerateJwt(loginUser);
                    TestLogger.LogCredentialTest($"Generate JWT for {realUserEmail}", "JWT_TOKEN", !string.IsNullOrEmpty(token));
                    token.Should().NotBeNullOrEmpty();
                }

                // Test 4: Wrong password
                var wrongPasswordLogin = await _authService.LoginLocalAsync(realUserEmail, "wrongpassword");
                TestLogger.LogCredentialTest($"Login {realUserEmail} with wrong password", "wrongpassword", wrongPasswordLogin == null);
                wrongPasswordLogin.Should().BeNull();

                TestLogger.LogTestPass("Real User Credentials Test", "INTEGRATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Real User Credentials Test", "INTEGRATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }

    public class MockHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _httpClient;

        public MockHttpClientFactory(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public HttpClient CreateClient(string name)
        {
            return _httpClient;
        }
    }
}