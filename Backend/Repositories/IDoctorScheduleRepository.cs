using Backend.Models;

namespace Backend.Repositories
{
    public interface IDoctorScheduleRepository
    {
        Task<DoctorSchedule?> GetByIdAsync(int scheduleId);
        Task<DoctorSchedule> UpdateAsync(DoctorSchedule schedule);
    }
}