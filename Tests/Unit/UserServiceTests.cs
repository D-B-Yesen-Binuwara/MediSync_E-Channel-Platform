using Backend.Models;
using Backend.Models.DTOs;
using Backend.Repositories;
using Backend.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace Tests.Unit
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly Mock<ITransactionRepository> _mockTransactionRepo;
        private readonly Mock<AuthService> _mockAuthService;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _mockTransactionRepo = new Mock<ITransactionRepository>();
            _mockAuthService = new Mock<AuthService>();
            _userService = new UserService(_mockUserRepo.Object, _mockTransactionRepo.Object, _mockAuthService.Object);
        }

        private User CreateTestUser(int id = 1)
        {
            return new User
            {
                UserId = id,
                FullName = "YB-alutheka",
                Email = "alutheka@gmail.com",
                PasswordHash = "test.hash",
                NIC = "200213203875",
                Role = UserRole.Patient,
                ContactNumber = "0768614247"
            };
        }

        [Fact]
        public async Task GetProfileAsync_ValidUserId_ReturnsUserProfile()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

            // Act
            var result = await _userService.GetProfileAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("YB-alutheka");
            result.Email.Should().Be("alutheka@gmail.com");
            result.Phone.Should().Be("0768614247");
        }

        [Fact]
        public async Task GetProfileAsync_InvalidUserId_ThrowsArgumentException()
        {
            // Arrange
            _mockUserRepo.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.GetProfileAsync(999));
            exception.Message.Should().Be("User not found");
        }

        [Fact]
        public async Task UpdateProfileAsync_ValidData_UpdatesProfile()
        {
            // Arrange
            var user = CreateTestUser();
            var updateDto = new UpdateProfileDto
            {
                Name = "Updated Name",
                Email = "updated@gmail.com",
                Phone = "0771234567"
            };

            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _mockUserRepo.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            var result = await _userService.UpdateProfileAsync(1, updateDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Updated Name");
            result.Email.Should().Be("updated@gmail.com");
            result.Phone.Should().Be("0771234567");
            _mockUserRepo.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_EmptyEmail_ThrowsArgumentException()
        {
            // Arrange
            var user = CreateTestUser();
            var updateDto = new UpdateProfileDto { Name = "Test", Email = "", Phone = "123" };
            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => _userService.UpdateProfileAsync(1, updateDto));
            exception.Message.Should().Be("Email cannot be empty");
        }

        [Fact]
        public async Task ChangePasswordAsync_ValidPasswords_ChangesPassword()
        {
            // Arrange
            var user = CreateTestUser();
            var changePasswordDto = new ChangePasswordDto
            {
                CurrentPassword = "password123",
                NewPassword = "newPassword456",
                ConfirmNewPassword = "newPassword456"
            };

            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _mockAuthService.Setup(x => x.VerifyPassword("password123", user.PasswordHash)).Returns(true);
            _mockAuthService.Setup(x => x.HashPassword("newPassword456")).Returns("new.hash");
            _mockUserRepo.Setup(x => x.UpdateAsync(It.IsAny<User>())).Returns(Task.CompletedTask);

            // Act
            await _userService.ChangePasswordAsync(1, changePasswordDto);

            // Assert
            _mockUserRepo.Verify(x => x.UpdateAsync(It.IsAny<User>()), Times.Once);
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
        public async Task DeleteAccountAsync_ValidUserId_DeletesAccount()
        {
            // Arrange
            var user = CreateTestUser();
            _mockUserRepo.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
            _mockUserRepo.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            await _userService.DeleteAccountAsync(1);

            // Assert
            _mockUserRepo.Verify(x => x.DeleteAsync(1), Times.Once);
        }
    }
}