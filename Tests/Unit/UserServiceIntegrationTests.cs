using Backend.Data;
using Backend.Models;
using Backend.Models.DTOs;
using Backend.Repositories;
using Backend.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Net.Http;

namespace Tests.Unit
{
    public class UserServiceIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;
        private readonly UserRepository _userRepository;
        private readonly TransactionRepository _transactionRepository;
        private readonly UserService _userService;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserServiceIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            
            _context = new AppDbContext(options);
            
            // Create a mock HttpClientFactory
            var httpClient = new HttpClient();
            _httpClientFactory = new MockHttpClientFactory(httpClient);
            
            _authService = new AuthService(_context, _httpClientFactory);
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
                PasswordHash = _authService.HashPassword("password123"), // Use real AuthService
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
        public async Task GetProfileAsync_ValidUserId_ReturnsUserProfile()
        {
            // Arrange
            var testUser = await SeedTestUserAsync();

            // Act
            var result = await _userService.GetProfileAsync(testUser.UserId);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(testUser.UserId);
            result.Name.Should().Be("YB-alutheka");
            result.Email.Should().Be("alutheka@gmail.com");
            result.Phone.Should().Be("0768614247");
        }

        [Fact]
        public async Task GetProfileAsync_InvalidUserId_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.GetProfileAsync(999));
            exception.Message.Should().Be("User not found");
        }

        [Fact]
        public async Task UpdateProfileAsync_ValidData_UpdatesProfile()
        {
            // Arrange
            var testUser = await SeedTestUserAsync();
            var updateDto = new UpdateProfileDto
            {
                Name = "Updated Name",
                Email = "updated@gmail.com",
                Phone = "0771234567"
            };

            // Act
            var result = await _userService.UpdateProfileAsync(testUser.UserId, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Name");
            result.Email.Should().Be("updated@gmail.com");
            result.Phone.Should().Be("0771234567");
        }

        [Fact]
        public async Task UpdateProfileAsync_EmptyEmail_ThrowsArgumentException()
        {
            // Arrange
            var testUser = await SeedTestUserAsync();
            var updateDto = new UpdateProfileDto { Name = "Test", Email = "", Phone = "123" };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.UpdateProfileAsync(testUser.UserId, updateDto));
            exception.Message.Should().Be("Email cannot be empty");
        }

        [Fact]
        public async Task ChangePasswordAsync_ValidPasswords_ChangesPassword()
        {
            // Arrange
            var testUser = await SeedTestUserAsync();
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "password123",
                NewPassword = "newPassword456",
                ConfirmNewPassword = "newPassword456"
            };

            // Act
            await _userService.ChangePasswordAsync(testUser.UserId, changePasswordDto);

            // Assert - Verify password was changed
            var updatedUser = await _userRepository.GetByIdAsync(testUser.UserId);
            var passwordVerified = _authService.VerifyPassword("newPassword456", updatedUser!.PasswordHash);
            passwordVerified.Should().BeTrue();
        }

        [Fact]
        public async Task ChangePasswordAsync_PasswordMismatch_ThrowsArgumentException()
        {
            // Arrange
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "password123",
                NewPassword = "newPassword456",
                ConfirmNewPassword = "differentPassword"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.ChangePasswordAsync(1, changePasswordDto));
            exception.Message.Should().Be("New passwords do not match.");
        }

        [Fact]
        public async Task ChangePasswordAsync_ShortPassword_ThrowsArgumentException()
        {
            // Arrange
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "password123",
                NewPassword = "123",
                ConfirmNewPassword = "123"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.ChangePasswordAsync(1, changePasswordDto));
            exception.Message.Should().Be("New password must be at least 6 characters.");
        }

        [Fact]
        public async Task ChangePasswordAsync_WrongCurrentPassword_ThrowsArgumentException()
        {
            // Arrange
            var testUser = await SeedTestUserAsync();
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "wrongPassword",
                NewPassword = "newPassword456",
                ConfirmNewPassword = "newPassword456"
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.ChangePasswordAsync(testUser.UserId, changePasswordDto));
            exception.Message.Should().Be("Current password is incorrect.");
        }

        [Fact]
        public async Task DeleteAccountAsync_ValidUserId_DeletesAccount()
        {
            // Arrange
            var testUser = await SeedTestUserAsync();

            // Act
            await _userService.DeleteAccountAsync(testUser.UserId);

            // Assert
            var deletedUser = await _userRepository.GetByIdAsync(testUser.UserId);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task DeleteAccountAsync_InvalidUserId_ThrowsArgumentException()
        {
            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.DeleteAccountAsync(999));
            exception.Message.Should().Be("User not found");
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }

    // Mock HttpClientFactory for testing
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