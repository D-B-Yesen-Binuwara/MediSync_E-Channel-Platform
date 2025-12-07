using Microsoft.AspNetCore.Mvc;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SchedulesController : ControllerBase
    {
        private readonly AppDbContext _context;
        
        public SchedulesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/schedules/doctor/{doctorId}
        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetDoctorSchedules(int doctorId)
        {
            var schedules = await _context.DoctorSchedules
                .Where(s => s.DoctorId == doctorId && s.ScheduleDate >= DateTime.Today)
                .Select(s => new
                {
                    id = s.ScheduleId,
                    doctorId = s.DoctorId,
                    date = s.ScheduleDate.ToString("yyyy-MM-dd"),
                    time = s.StartTime.ToString(@"hh\:mm") + " - " + s.EndTime.ToString(@"hh\:mm"),
                    totalSlots = s.TotalSlots,
                    availableSlots = s.AvailableSlots,
                    wardNo = "A-101", // Default ward
                    price = 2500 // Default consultation fee
                })
                .OrderBy(s => s.date)
                .ToListAsync();
            
            return Ok(schedules);
        }
    }
}