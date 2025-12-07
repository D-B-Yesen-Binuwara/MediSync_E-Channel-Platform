using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Backend.Repositories;
using Backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Integration
{
    public class UserProfileIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly UserRepository _userRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly UserService _userService;

        public UserProfileIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);
            _authService = new AuthService(_context, null!);
            _userRepository = new UserRepository(_context);
            _transactionRepository = new TransactionRepository(_context);
            _userService = new UserService(_userRepository, _transactionRepository, _authService);
        }

        private User CreateTestUser(int id = 1)
        {
            return new User
            {
                UserId = id,
                FullName = "YB-alutheka",
                Email = "alutheka@gmail.com",
                PasswordHash = _authService.HashPassword("password123"),
                NIC = "200213203875",
                Role = UserRole.Patient,
                ContactNumber = "0768614247"
            };
        }

        private async Task<User> SeedTestUserAsync()
        {
            var user = CreateTestUser();
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        [Fact]
        public async Task UserProfile_FullCRUDWorkflow_WorksCorrectly()
        {
            // Arrange
            var testUser = await SeedTestUserAsync();

            // Test 1: Get Profile
            var profile = await _userService.GetProfileAsync(testUser.UserId);
            profile.Should().NotBeNull();
            profile.Name.Should().Be("YB-alutheka");
            profile.Email.Should().Be("alutheka@gmail.com");

            // Test 2: Update Profile
            var updateDto = new UpdateProfileDto
            {
                Name = "Updated YB-alutheka",
                Email = "updated.alutheka@gmail.com",
                Phone = "0771234567"
            };
            var updatedProfile = await _userService.UpdateProfileAsync(testUser.UserId, updateDto);
            updatedProfile.Name.Should().Be("Updated YB-alutheka");
            updatedProfile.Email.Should().Be("updated.alutheka@gmail.com");
            updatedProfile.Phone.Should().Be("0771234567");

            // Test 3: Change Password
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "password123",
                NewPassword = "newPassword456",
                ConfirmNewPassword = "newPassword456"
            };
            await _userService.ChangePasswordAsync(testUser.UserId, changePasswordDto);
            
            // Verify password was changed
            var updatedUser = await _userRepository.GetByIdAsync(testUser.UserId);
            var passwordVerified = _authService.VerifyPassword("newPassword456", updatedUser!.PasswordHash);
            passwordVerified.Should().BeTrue();

            // Test 4: Verify old password no longer works
            var oldPasswordVerified = _authService.VerifyPassword("password123", updatedUser.PasswordHash);
            oldPasswordVerified.Should().BeFalse();
        }

        [Fact]
        public async Task UserProfile_ValidationScenarios_HandledCorrectly()
        {
            var testUser = await SeedTestUserAsync();

            // Test 1: Empty email validation
            var invalidUpdateDto = new UpdateProfileDto
            {
                Name = "Test User",
                Email = "",
                Phone = "123456789"
            };
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.UpdateProfileAsync(testUser.UserId, invalidUpdateDto));
            exception.Message.Should().Be("Email cannot be empty");

            // Test 2: Password mismatch validation
            var mismatchPasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "password123",
                NewPassword = "newPassword456",
                ConfirmNewPassword = "differentPassword"
            };
            exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.ChangePasswordAsync(testUser.UserId, mismatchPasswordDto));
            exception.Message.Should().Be("New passwords do not match.");

            // Test 3: Short password validation
            var shortPasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "password123",
                NewPassword = "123",
                ConfirmNewPassword = "123"
            };
            exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.ChangePasswordAsync(testUser.UserId, shortPasswordDto));
            exception.Message.Should().Be("New password must be at least 6 characters.");

            // Test 4: Wrong current password validation
            var wrongPasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "wrongPassword",
                NewPassword = "newPassword456",
                ConfirmNewPassword = "newPassword456"
            };
            exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.ChangePasswordAsync(testUser.UserId, wrongPasswordDto));
            exception.Message.Should().Be("Current password is incorrect.");

            // Test 5: Non-existent user validation
            exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.GetProfileAsync(999));
            exception.Message.Should().Be("User not found");
        }

        [Fact]
        public async Task UserProfile_ImageUpload_WorksCorrectly()
        {
            var testUser = await SeedTestUserAsync();

            // Test 1: Valid base64 image upload
            var validImageBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5 });
            var updateWithImageDto = new UpdateProfileDto
            {
                Name = "Test User",
                Email = "test@gmail.com",
                Phone = "123456789",
                ImageBase64 = $"data:image/jpeg;base64,{validImageBase64}"
            };

            var updatedProfile = await _userService.UpdateProfileAsync(testUser.UserId, updateWithImageDto);
            updatedProfile.ImageBase64.Should().NotBeNull();
            updatedProfile.ImageBase64.Should().StartWith("data:image/jpeg;base64,");

            // Test 2: Remove image (null)
            var removeImageDto = new UpdateProfileDto
            {
                Name = "Test User",
                Email = "test@gmail.com",
                Phone = "123456789",
                ImageBase64 = null
            };

            var profileWithoutImage = await _userService.UpdateProfileAsync(testUser.UserId, removeImageDto);
            profileWithoutImage.ImageBase64.Should().BeNull();

            // Test 3: Invalid base64 format
            var invalidImageDto = new UpdateProfileDto
            {
                Name = "Test User",
                Email = "test@gmail.com",
                Phone = "123456789",
                ImageBase64 = "invalid-base64-data"
            };

            var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
                _userService.UpdateProfileAsync(testUser.UserId, invalidImageDto));
            exception.Message.Should().Be("Invalid image data format.");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}