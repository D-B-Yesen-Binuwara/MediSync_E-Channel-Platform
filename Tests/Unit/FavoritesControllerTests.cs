using Backend.Controllers;
using Backend.Models;
using Backend.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Tests.Unit
{
    public class FavoritesControllerTests
    {
        private FavoritesController CreateControllerWithUser(Mock<IFavoriteRepository> mockRepo, int? userId)
        {
            // Use a null AppDbContext for tests that don't use it; controller requires it in ctor, but some actions use repository.
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Backend.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new Backend.Data.AppDbContext(options);
            var controller = new FavoritesController(mockRepo.Object, context);

            if (userId.HasValue)
            {
                var claims = new[] { new Claim("sub", userId.Value.ToString()) };
                var identity = new ClaimsIdentity(claims, "TestAuth");
                var principal = new ClaimsPrincipal(identity);
                controller.ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = principal }
                };
            }

            return controller;
        }

        [Fact]
        public async Task AddFavorite_ReturnsOk_WhenSuccess()
        {
            var mockRepo = new Mock<IFavoriteRepository>();
            mockRepo.Setup(r => r.AddFavoriteAsync(1, 2)).ReturnsAsync(new Favorite { FavoriteId = 1, PatientId = 1, DoctorId = 2 });

            var controller = CreateControllerWithUser(mockRepo, 1);

            var result = await controller.AddFavorite(2);
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task AddFavorite_ReturnsBadRequest_WhenAlreadyExists()
        {
            var mockRepo = new Mock<IFavoriteRepository>();
            mockRepo.Setup(r => r.AddFavoriteAsync(1, 2)).ThrowsAsync(new InvalidOperationException("Doctor is already in favorites"));

            var controller = CreateControllerWithUser(mockRepo, 1);

            var result = await controller.AddFavorite(2);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        [Fact]
        public async Task AddFavorite_ReturnsUnauthorized_WhenNoUserClaim()
        {
            var mockRepo = new Mock<IFavoriteRepository>();
            var controller = CreateControllerWithUser(mockRepo, null);

            var result = await controller.AddFavorite(2);
            result.Should().BeOfType<UnauthorizedObjectResult>();
        }

        [Fact]
        public async Task RemoveFavorite_ReturnsOk_WhenSuccess()
        {
            var mockRepo = new Mock<IFavoriteRepository>();
            mockRepo.Setup(r => r.RemoveFavoriteAsync(1, 2)).Returns(Task.CompletedTask);

            var controller = CreateControllerWithUser(mockRepo, 1);

            var result = await controller.RemoveFavorite(2);
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task CheckFavorite_ReturnsIsFavorite()
        {
            var mockRepo = new Mock<IFavoriteRepository>();
            mockRepo.Setup(r => r.IsFavoriteAsync(1, 2)).ReturnsAsync(true);

            var controller = CreateControllerWithUser(mockRepo, 1);

            var result = await controller.CheckFavorite(2);
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok!.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetFavorites_ReturnsFavorites_FromContext()
        {
            // For GetFavorites controller uses DbContext directly, so we create a real context and seed it
            var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<Backend.Data.AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new Backend.Data.AppDbContext(options);

            var user = new User { UserId = 1, FullName = "U", Email = "u@e.com", PasswordHash = "h", NIC = "123456789V", Role = UserRole.Patient };
            var doctor = new Doctor { DoctorId = 2, FullName = "Dr X", Specialization = "Gen", NIC = "200213203875", Email = "d@e.com", ContactNo = "0710000000", Details = "d" };
            context.Users.Add(user);
            context.Doctors.Add(doctor);
            context.Favorites.Add(new Favorite { FavoriteId = 1, PatientId = 1, DoctorId = 2, Doctor = doctor, Patient = user });
            await context.SaveChangesAsync();

            var mockRepo = new Mock<IFavoriteRepository>();
            var controller = new FavoritesController(mockRepo.Object, context);

            var claims = new[] { new Claim("sub", "1") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = await controller.GetFavorites();
            result.Should().BeOfType<OkObjectResult>();
            var ok = result as OkObjectResult;
            ok!.Value.Should().NotBeNull();
        }
    }
}
