using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Data;
using Backend.Models;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/admin/[controller]")]
    [AllowAnonymous]
    public class AdminDoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminDoctorsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetDoctors([FromQuery] string? search)
        {
            var query = _context.Doctors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d => 
                    d.FullName.Contains(search) ||
                    d.Specialization.Contains(search) ||
                    d.Email.Contains(search));
            }

            var doctors = await query
                .OrderBy(d => d.FullName)
                .Select(d => new {
                    d.DoctorId,
                    d.FullName,
                    d.Specialization,
                    d.NIC,
                    d.Qualification,
                    d.Email,
                    d.ContactNo,
                    d.Details,
                    d.CreatedAt
                })
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpPost]
        public async Task<ActionResult> CreateDoctor([FromBody] CreateDoctorRequest request)
        {
            var doctor = new Doctor
            {
                FullName = request.FullName,
                Specialization = request.Specialization,
                NIC = request.NIC,
                Qualification = request.Qualification,
                Email = request.Email,
                ContactNo = request.ContactNo,
                Details = request.Details,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { 
                doctor.DoctorId,
                doctor.FullName,
                doctor.Specialization,
                doctor.Email,
                message = "Doctor created successfully" 
            });
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateDoctor(int id, [FromBody] CreateDoctorRequest request)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            doctor.FullName = request.FullName;
            doctor.Specialization = request.Specialization;
            doctor.NIC = request.NIC;
            doctor.Qualification = request.Qualification;
            doctor.Email = request.Email;
            doctor.ContactNo = request.ContactNo;
            doctor.Details = request.Details;
            doctor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            _context.Doctors.Remove(doctor);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Doctor deleted successfully" });
        }
    }

    public class CreateDoctorRequest
    {
        public string FullName { get; set; } = "";
        public string Specialization { get; set; } = "";
        public string NIC { get; set; } = "";
        public string Qualification { get; set; } = "";
        public string Email { get; set; } = "";
        public string ContactNo { get; set; } = "";
        public string Details { get; set; } = "";
    }
}