using Backend.Models;

namespace Backend.Repositories
{
    public interface IAppointmentRepository
    {
        Task<Appointment> CreateAsync(Appointment appointment);
        Task<Appointment?> GetByIdAsync(int appointmentId);
        Task<List<Appointment>> GetByPatientIdAsync(int patientId);
        Task<Appointment> UpdateAsync(Appointment appointment);
    }
}