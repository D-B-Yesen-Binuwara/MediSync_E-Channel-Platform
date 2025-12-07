using Backend.Data;
using Backend.Models;
using Backend.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Tests.Unit
{
    public class FavoriteRepositoryIntegrationTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly FavoriteRepository _favoriteRepository;

        public FavoriteRepositoryIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);
            _favoriteRepository = new FavoriteRepository(_context);
        }

        private async Task<User> SeedUserAsync(int id = 1)
        {
            var user = new User
            {
                UserId = id,
                FullName = "Test User",
                Email = "test@example.com",
                PasswordHash = "hash",
                NIC = "123456789V",
                Role = UserRole.Patient,
                ContactNumber = "0771234567"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        private async Task<Doctor> SeedDoctorAsync(int id = 1)
        {
            var doctor = new Doctor
            {
                DoctorId = id,
                FullName = "Dr Test",
                Specialization = "Cardiology",
                NIC = "200213203875",
                Qualification = "MBBS",
                Email = $"doc{id}@example.com",
                ContactNo = "0711234567",
                Details = "Details"
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return doctor;
        }

        [Fact]
        public async Task AddFavoriteAsync_AddsFavorite()
        {
            var user = await SeedUserAsync();
            var doctor = await SeedDoctorAsync();

            var favorite = await _favoriteRepository.AddFavoriteAsync(user.UserId, doctor.DoctorId);

            favorite.Should().NotBeNull();
            favorite.PatientId.Should().Be(user.UserId);
            favorite.DoctorId.Should().Be(doctor.DoctorId);

            var exists = await _favoriteRepository.IsFavoriteAsync(user.UserId, doctor.DoctorId);
            exists.Should().BeTrue();
        }

        [Fact]
        public async Task AddFavoriteAsync_Duplicate_ThrowsInvalidOperationException()
        {
            var user = await SeedUserAsync();
            var doctor = await SeedDoctorAsync();

            await _favoriteRepository.AddFavoriteAsync(user.UserId, doctor.DoctorId);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _favoriteRepository.AddFavoriteAsync(user.UserId, doctor.DoctorId));
        }

        [Fact]
        public async Task GetUserFavoritesAsync_ReturnsFavorites()
        {
            var user = await SeedUserAsync();
            var doctor1 = await SeedDoctorAsync(1);
            var doctor2 = await SeedDoctorAsync(2);

            await _favoriteRepository.AddFavoriteAsync(user.UserId, doctor1.DoctorId);
            await _favoriteRepository.AddFavoriteAsync(user.UserId, doctor2.DoctorId);

            var favs = await _favoriteRepository.GetUserFavoritesAsync(user.UserId);
            favs.Should().HaveCount(2);
            favs.Select(f => f.DoctorId).Should().Contain(new[] { doctor1.DoctorId, doctor2.DoctorId });
        }

        [Fact]
        public async Task GetFavoriteAsync_ReturnsFavoriteOrNull()
        {
            var user = await SeedUserAsync();
            var doctor = await SeedDoctorAsync();

            await _favoriteRepository.AddFavoriteAsync(user.UserId, doctor.DoctorId);

            var fav = await _favoriteRepository.GetFavoriteAsync(user.UserId, doctor.DoctorId);
            fav.Should().NotBeNull();

            var missing = await _favoriteRepository.GetFavoriteAsync(user.UserId, 9999);
            missing.Should().BeNull();
        }

        [Fact]
        public async Task RemoveFavoriteAsync_RemovesFavorite()
        {
            var user = await SeedUserAsync();
            var doctor = await SeedDoctorAsync();

            await _favoriteRepository.AddFavoriteAsync(user.UserId, doctor.DoctorId);

            await _favoriteRepository.RemoveFavoriteAsync(user.UserId, doctor.DoctorId);

            var exists = await _favoriteRepository.IsFavoriteAsync(user.UserId, doctor.DoctorId);
            exists.Should().BeFalse();
        }

        [Fact]
        public async Task IsFavoriteAsync_ReturnsFalseWhenNotPresent()
        {
            var user = await SeedUserAsync();
            var doctor = await SeedDoctorAsync();

            var exists = await _favoriteRepository.IsFavoriteAsync(user.UserId, doctor.DoctorId);
            exists.Should().BeFalse();
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
