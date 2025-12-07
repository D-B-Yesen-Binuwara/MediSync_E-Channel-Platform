using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppDbContext _context;

        public AppointmentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Appointment> CreateAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<Appointment?> GetByIdAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.DoctorSchedule)
                .Include(a => a.Patient)
                .Include(a => a.Transaction)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);
        }

        public async Task<List<Appointment>> GetByPatientIdAsync(int patientId)
        {
            return await _context.Appointments
                .Include(a => a.DoctorSchedule)
                    .ThenInclude(s => s.Doctor)
                .Include(a => a.Transaction)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<Appointment> UpdateAsync(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }
    }
}