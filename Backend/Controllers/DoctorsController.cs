using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Backend.Models;
using Backend.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFavoriteRepository _favoriteRepository;
        
        public DoctorsController(AppDbContext context, IFavoriteRepository favoriteRepository)
        {
            _context = context;
            _favoriteRepository = favoriteRepository;
        }

        private int? GetUserIdIfAuthenticated()
        {
            var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }

        // GET: api/doctors
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? name, [FromQuery] string? specialization, [FromQuery] DateTime? date)
        {
            var query = _context.Doctors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(d => d.FullName.Contains(name));
            if (!string.IsNullOrWhiteSpace(specialization))
                query = query.Where(d => d.Specialization == specialization);
            // Optionally filter by available date (requires join with schedules)
            // if (date.HasValue) { ... }

            var doctors = await query.ToListAsync();
            
            // Add favorite status if user is authenticated
            var userId = GetUserIdIfAuthenticated();
            if (userId.HasValue)
            {
                var doctorsWithFavorites = new List<object>();
                foreach (var doctor in doctors)
                {
                    var isFavorite = await _favoriteRepository.IsFavoriteAsync(userId.Value, doctor.DoctorId);
                    doctorsWithFavorites.Add(new
                    {
                        doctor.DoctorId,
                        doctor.FullName,
                        doctor.Specialization,
                        doctor.NIC,
                        doctor.Qualification,
                        doctor.Email,
                        doctor.ContactNo,
                        doctor.Details,
                        doctor.CreatedAt,
                        doctor.UpdatedAt,
                        IsFavorite = isFavorite
                    });
                }
                return Ok(doctorsWithFavorites);
            }
            
            return Ok(doctors);
        }
    }
}
