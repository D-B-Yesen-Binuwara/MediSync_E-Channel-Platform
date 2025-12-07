using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class DoctorRepository
    {
        private readonly AppDbContext _context;
        public DoctorRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Doctor>> GetDoctorsAsync(string? name, string? specialization, DateTime? date)
        {
            var query = _context.Doctors.AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(d => d.FullName.Contains(name));
            if (!string.IsNullOrWhiteSpace(specialization))
                query = query.Where(d => d.Specialization == specialization);
            // Optionally filter by available date
            // if (date.HasValue) { ... }
            return await query.ToListAsync();
        }
    }
}
