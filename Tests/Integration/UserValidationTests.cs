using Backend.Data;
using Backend.Services;
using Backend.Models.DTOs;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Net.Http;

namespace Tests.Integration
{
    public class UserValidationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly UserService _userService;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserValidationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);
            
            var httpClient = new HttpClient();
            _httpClientFactory = new MockHttpClientFactory(httpClient);
            _authService = new AuthService(_context, _httpClientFactory);
            
            var userRepository = new Backend.Repositories.UserRepository(_context);
            var transactionRepository = new Backend.Repositories.TransactionRepository(_context);
            _userService = new UserService(userRepository, transactionRepository, _authService);
        }

        [Fact]
        public async Task ChangePassword_ValidPasswords_ShouldWork()
        {
            TestLogger.LogTestStart("Change Password - Valid Case", "VALIDATION");
            var startTime = DateTime.Now;

            try
            {
                // Create test user
                var user = await _authService.RegisterLocalAsync("Test User", "test@example.com", "123456789", "password123", "0771234567");
                user.Should().NotBeNull();

                var changePasswordDto = new ChangePasswordDto
                {
                    CurrentPassword = "password123",
                    NewPassword = "newPassword456",
                    ConfirmNewPassword = "newPassword456"
                };

                // Test password change
                await _userService.ChangePasswordAsync(user!.UserId, changePasswordDto);

                // Verify new password works
                var loginResult = await _authService.LoginLocalAsync("test@example.com", "newPassword456");
                TestLogger.LogCredentialTest("Login with new password", "newPassword456", loginResult != null);
                loginResult.Should().NotBeNull();

                TestLogger.LogTestPass("Change Password - Valid Case", "VALIDATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Change Password - Valid Case", "VALIDATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        [Fact]
        public async Task ChangePassword_WrongCurrentPassword_ShouldFail()
        {
            TestLogger.LogTestStart("Change Password - Wrong Current Password", "VALIDATION");
            var startTime = DateTime.Now;

            try
            {
                var user = await _authService.RegisterLocalAsync("Test User", "test2@example.com", "123456780", "password123", "0771234567");
                
                var changePasswordDto = new ChangePasswordDto
                {
                    CurrentPassword = "wrongPassword",
                    NewPassword = "newPassword456",
                    ConfirmNewPassword = "newPassword456"
                };

                var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                    _userService.ChangePasswordAsync(user!.UserId, changePasswordDto));
                
                TestLogger.LogCredentialTest("Wrong current password validation", "wrongPassword", exception.Message.Contains("Current password is incorrect"));
                exception.Message.Should().Be("Current password is incorrect.");

                TestLogger.LogTestPass("Change Password - Wrong Current Password", "VALIDATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Change Password - Wrong Current Password", "VALIDATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        [Fact]
        public async Task ChangePassword_PasswordMismatch_ShouldFail()
        {
            TestLogger.LogTestStart("Change Password - Password Mismatch", "VALIDATION");
            var startTime = DateTime.Now;

            try
            {
                var changePasswordDto = new ChangePasswordDto
                {
                    CurrentPassword = "password123",
                    NewPassword = "newPassword456",
                    ConfirmNewPassword = "differentPassword"
                };

                var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                    _userService.ChangePasswordAsync(1, changePasswordDto));
                
                TestLogger.LogCredentialTest("Password mismatch validation", "mismatch", exception.Message.Contains("do not match"));
                exception.Message.Should().Be("New passwords do not match.");

                TestLogger.LogTestPass("Change Password - Password Mismatch", "VALIDATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Change Password - Password Mismatch", "VALIDATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        [Fact]
        public async Task ChangePassword_TooShort_ShouldFail()
        {
            TestLogger.LogTestStart("Change Password - Too Short", "VALIDATION");
            var startTime = DateTime.Now;

            try
            {
                var changePasswordDto = new ChangePasswordDto
                {
                    CurrentPassword = "password123",
                    NewPassword = "123",
                    ConfirmNewPassword = "123"
                };

                var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                    _userService.ChangePasswordAsync(1, changePasswordDto));
                
                TestLogger.LogCredentialTest("Short password validation", "123", exception.Message.Contains("at least 6 characters"));
                exception.Message.Should().Be("New password must be at least 6 characters.");

                TestLogger.LogTestPass("Change Password - Too Short", "VALIDATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Change Password - Too Short", "VALIDATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        [Fact]
        public async Task ChangeName_ValidName_ShouldWork()
        {
            TestLogger.LogTestStart("Change Name - Valid Case", "VALIDATION");
            var startTime = DateTime.Now;

            try
            {
                var user = await _authService.RegisterLocalAsync("Original Name", "name@example.com", "123456781", "password123", "0771234567");
                
                var updateDto = new UpdateProfileDto
                {
                    Name = "Updated Name",
                    Email = "name@example.com",
                    Phone = "0771234567"
                };

                var result = await _userService.UpdateProfileAsync(user!.UserId, updateDto);
                
                TestLogger.LogCredentialTest("Name change", "Updated Name", result.Name == "Updated Name");
                result.Name.Should().Be("Updated Name");

                TestLogger.LogTestPass("Change Name - Valid Case", "VALIDATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Change Name - Valid Case", "VALIDATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        [Fact]
        public async Task ChangeName_EmptyName_ShouldFail()
        {
            TestLogger.LogTestStart("Change Name - Empty Name", "VALIDATION");
            var startTime = DateTime.Now;

            try
            {
                var user = await _authService.RegisterLocalAsync("Test User", "empty@example.com", "123456782", "password123", "0771234567");
                
                var updateDto = new UpdateProfileDto
                {
                    Name = "",
                    Email = "empty@example.com",
                    Phone = "0771234567"
                };

                var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                    _userService.UpdateProfileAsync(user!.UserId, updateDto));
                
                TestLogger.LogCredentialTest("Empty name validation", "", exception.Message.Contains("Name cannot be empty"));
                exception.Message.Should().Be("Name cannot be empty");

                TestLogger.LogTestPass("Change Name - Empty Name", "VALIDATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Change Name - Empty Name", "VALIDATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        [Fact]
        public async Task ChangeContactNumber_ValidNumber_ShouldWork()
        {
            TestLogger.LogTestStart("Change Contact Number - Valid Case", "VALIDATION");
            var startTime = DateTime.Now;

            try
            {
                var user = await _authService.RegisterLocalAsync("Test User", "contact@example.com", "123456783", "password123", "0771234567");
                
                var updateDto = new UpdateProfileDto
                {
                    Name = "Test User",
                    Email = "contact@example.com",
                    Phone = "0779876543"
                };

                var result = await _userService.UpdateProfileAsync(user!.UserId, updateDto);
                
                TestLogger.LogCredentialTest("Contact number change", "0779876543", result.Phone == "0779876543");
                result.Phone.Should().Be("0779876543");

                TestLogger.LogTestPass("Change Contact Number - Valid Case", "VALIDATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Change Contact Number - Valid Case", "VALIDATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        [Fact]
        public async Task ChangeContactNumber_EmptyNumber_ShouldFail()
        {
            TestLogger.LogTestStart("Change Contact Number - Empty Number", "VALIDATION");
            var startTime = DateTime.Now;

            try
            {
                var user = await _authService.RegisterLocalAsync("Test User", "emptyphone@example.com", "123456784", "password123", "0771234567");
                
                var updateDto = new UpdateProfileDto
                {
                    Name = "Test User",
                    Email = "emptyphone@example.com",
                    Phone = ""
                };

                var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                    _userService.UpdateProfileAsync(user!.UserId, updateDto));
                
                TestLogger.LogCredentialTest("Empty phone validation", "", exception.Message.Contains("Phone cannot be empty"));
                exception.Message.Should().Be("Phone cannot be empty");

                TestLogger.LogTestPass("Change Contact Number - Empty Number", "VALIDATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Change Contact Number - Empty Number", "VALIDATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        [Fact]
        public async Task ChangeEmail_ValidEmail_ShouldWork()
        {
            TestLogger.LogTestStart("Change Email - Valid Case", "VALIDATION");
            var startTime = DateTime.Now;

            try
            {
                var user = await _authService.RegisterLocalAsync("Test User", "original@example.com", "123456785", "password123", "0771234567");
                
                var updateDto = new UpdateProfileDto
                {
                    Name = "Test User",
                    Email = "updated@example.com",
                    Phone = "0771234567"
                };

                var result = await _userService.UpdateProfileAsync(user!.UserId, updateDto);
                
                TestLogger.LogCredentialTest("Email change", "updated@example.com", result.Email == "updated@example.com");
                result.Email.Should().Be("updated@example.com");

                TestLogger.LogTestPass("Change Email - Valid Case", "VALIDATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Change Email - Valid Case", "VALIDATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        [Fact]
        public async Task ChangeEmail_EmptyEmail_ShouldFail()
        {
            TestLogger.LogTestStart("Change Email - Empty Email", "VALIDATION");
            var startTime = DateTime.Now;

            try
            {
                var user = await _authService.RegisterLocalAsync("Test User", "emailtest@example.com", "123456786", "password123", "0771234567");
                
                var updateDto = new UpdateProfileDto
                {
                    Name = "Test User",
                    Email = "",
                    Phone = "0771234567"
                };

                var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                    _userService.UpdateProfileAsync(user!.UserId, updateDto));
                
                TestLogger.LogCredentialTest("Empty email validation", "", exception.Message.Contains("Email cannot be empty"));
                exception.Message.Should().Be("Email cannot be empty");

                TestLogger.LogTestPass("Change Email - Empty Email", "VALIDATION", DateTime.Now - startTime);
            }
            catch (Exception ex)
            {
                TestLogger.LogTestFail("Change Email - Empty Email", "VALIDATION", ex.Message, DateTime.Now - startTime);
                throw;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}