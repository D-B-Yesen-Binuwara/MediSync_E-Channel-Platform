using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    // Repository pattern for favorites - keeps database logic separate from controllers
    // Makes testing easier and code more organized
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AppDbContext _context;

        public FavoriteRepository(AppDbContext context)
        {
            _context = context;
        }

        // Get all favorites for a specific user
        // Orders by newest first because that's probably what users want to see
        public async Task<IEnumerable<Favorite>> GetUserFavoritesAsync(int userId)
        {
            return await _context.Favorites
                .Include(f => f.Doctor) // eager loading to avoid N+1 queries
                .Where(f => f.PatientId == userId)
                .OrderByDescending(f => f.CreatedAt) // newest favorites first
                .ToListAsync();
        }

        // Find a specific favorite record by user and doctor IDs
        // Returns null if not found (hence the ? in Favorite?)
        public async Task<Favorite?> GetFavoriteAsync(int userId, int doctorId)
        {
            return await _context.Favorites
                .FirstOrDefaultAsync(f => f.PatientId == userId && f.DoctorId == doctorId);
        }

        // Add a new favorite - but first check if it already exists
        // We don't want duplicate favorites for the same doctor
        public async Task<Favorite> AddFavoriteAsync(int userId, int doctorId)
        {
            // Check if this combo already exists
            var existing = await GetFavoriteAsync(userId, doctorId);
            if (existing != null)
                throw new InvalidOperationException("Doctor is already in favorites");

            var favorite = new Favorite
            {
                PatientId = userId,
                DoctorId = doctorId
                // CreatedAt will be set automatically in the model
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync(); // commit to database
            return favorite;
        }

        // Remove a favorite - silently does nothing if it doesn't exist
        // This is probably the right behavior (idempotent operations are good)
        public async Task RemoveFavoriteAsync(int userId, int doctorId)
        {
            var favorite = await GetFavoriteAsync(userId, doctorId);
            if (favorite != null) // only remove if it exists
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            // If it doesn't exist, we just do nothing - no error thrown
        }

        // Quick check to see if a doctor is favorited by a user
        // More efficient than GetFavoriteAsync when you just need true/false
        public async Task<bool> IsFavoriteAsync(int userId, int doctorId)
        {
            return await _context.Favorites
                .AnyAsync(f => f.PatientId == userId && f.DoctorId == doctorId);
        }
    }
}