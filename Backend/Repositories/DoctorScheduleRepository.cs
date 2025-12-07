using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class DoctorScheduleRepository : IDoctorScheduleRepository
    {
        private readonly AppDbContext _context;

        public DoctorScheduleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DoctorSchedule?> GetByIdAsync(int scheduleId)
        {
            return await _context.DoctorSchedules
                .Include(s => s.Doctor)
                .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
        }

        public async Task<DoctorSchedule> UpdateAsync(DoctorSchedule schedule)
        {
            _context.DoctorSchedules.Update(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }
    }
}