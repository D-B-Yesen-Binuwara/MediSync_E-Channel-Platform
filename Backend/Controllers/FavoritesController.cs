using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Backend.Repositories;
using Backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    // This controller handles all the favorite doctor stuff
    // Users can save doctors they like and view them later
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // gotta be logged in to use this
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly AppDbContext _context;

        public FavoritesController(IFavoriteRepository favoriteRepository, AppDbContext context)
        {
            _favoriteRepository = favoriteRepository;
            _context = context;
        }

        // Helper method to get the current user's ID from the JWT token
        // This was a pain to get working properly...
        private int GetUserId()
        {
            // Try different claim types because JWT tokens can be inconsistent
            var userIdClaim = User?.FindFirst("sub")?.Value ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"Raw user claim: {userIdClaim}");

            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out int userId))
            {
                Console.WriteLine($"Parsed user ID: {userId}");
                return userId;
            }

            // If we can't get a valid user ID, something's wrong with the token
            Console.WriteLine($"Failed to parse user ID from claim: {userIdClaim}");
            throw new UnauthorizedAccessException($"Invalid user token. Claim value: {userIdClaim ?? "null"}");
        }

        // GET endpoint to fetch all favorite doctors for the current user
        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            try
            {
                var userId = GetUserId();
                Console.WriteLine($"Getting favorites for user ID: {userId}");
                
                // Query the database directly instead of using repository
                // (probably should refactor this to use the repo but it works for now)
                var favorites = await _context.Favorites
                    .Include(f => f.Doctor) // join with doctor table to get doctor info
                    .Where(f => f.PatientId == userId)
                    .Select(f => new {
                        favoriteId = f.FavoriteId,
                        doctor = new {
                            doctorId = f.Doctor.DoctorId,
                            fullName = f.Doctor.FullName,
                            specialization = f.Doctor.Specialization,
                            qualification = f.Doctor.Qualification,
                            email = f.Doctor.Email
                        }
                    })
                    .ToListAsync();
                    
                Console.WriteLine($"Found {favorites.Count} favorites");
                return Ok(favorites);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting favorites: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // Add a doctor to user's favorites list
        // POST /api/favorites/123 (where 123 is the doctor ID)
        [HttpPost("{doctorId}")]
        public async Task<IActionResult> AddFavorite(int doctorId)
        {
            try
            {
                var userId = GetUserId();
                Console.WriteLine($"Adding favorite: UserId={userId}, DoctorId={doctorId}");
                await _favoriteRepository.AddFavoriteAsync(userId, doctorId);
                Console.WriteLine($"Successfully added favorite for user {userId}");
                return Ok(new { message = "Added to favorites", userId = userId, doctorId = doctorId });
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Unauthorized: {ex.Message}");
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Invalid operation: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding favorite: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

        // Remove doctor from favorites - simple DELETE request
        [HttpDelete("{doctorId}")]
        public async Task<IActionResult> RemoveFavorite(int doctorId)
        {
            try
            {
                var userId = GetUserId();
                await _favoriteRepository.RemoveFavoriteAsync(userId, doctorId); // let the repo handle the logic
                return Ok(new { message = "Removed from favorites" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // Check if a specific doctor is already in user's favorites
        // Useful for showing the heart icon as filled or empty on the frontend
        [HttpGet("check/{doctorId}")]
        public async Task<IActionResult> CheckFavorite(int doctorId)
        {
            try
            {
                var userId = GetUserId();
                var isFavorite = await _favoriteRepository.IsFavoriteAsync(userId, doctorId);
                return Ok(new { isFavorite }); // returns true/false
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // Debug endpoint - shows all favorites in the system
        // TODO: remove this before going to production!
        [HttpGet("debug")]
        [AllowAnonymous] // no auth needed for debugging
        public async Task<IActionResult> DebugFavorites()
        {
            try
            {
                // Get everything from the database for debugging
                var allFavs = await _context.Favorites.Include(f => f.Doctor).ToListAsync();
                var userClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
                
                return Ok(new { 
                    totalFavoritesInDb = allFavs.Count,
                    isAuthenticated = User.Identity?.IsAuthenticated,
                    userClaims = userClaims,
                    favorites = allFavs.Select(f => new {
                        favoriteId = f.FavoriteId,
                        patientId = f.PatientId,
                        doctorId = f.DoctorId,
                        doctorName = f.Doctor?.FullName
                    })
                });
            }
            catch (Exception ex)
            {
                return Ok(new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }
}