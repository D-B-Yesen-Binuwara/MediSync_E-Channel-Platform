using Backend.Models;
using Backend.Models.DTOs;
using Backend.Repositories;
using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class BookingService : IBookingService
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly ITransactionRepository _transactionRepo;
        private readonly IDoctorScheduleRepository _scheduleRepo;
        private readonly IPaymentService _paymentService;
        public BookingService(
            IAppointmentRepository appointmentRepo,
            ITransactionRepository transactionRepo,
            IDoctorScheduleRepository scheduleRepo,
            IPaymentService paymentService)
        {
            _appointmentRepo = appointmentRepo;
            _transactionRepo = transactionRepo;
            _scheduleRepo = scheduleRepo;
            _paymentService = paymentService;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request, int patientId)
        {
            try
            {
                Console.WriteLine($"Received PatientId: {patientId}");
                // Validate schedule availability
                var schedule = await _scheduleRepo.GetByIdAsync(request.ScheduleId);
                if (schedule == null)
                    throw new ArgumentException("Schedule not found");

                if (schedule.AvailableSlots <= 0)
                    throw new InvalidOperationException("No available slots");

            // Process payment
            var paymentId = await _paymentService.ProcessPaymentAsync(request.Payment);
            
            // Create appointment first
            var appointment = new Appointment
            {
                PatientId = patientId,
                ScheduleId = request.ScheduleId,
                PatientName = request.PatientName,
                PatientContact = request.ContactNo,
                SlotNumber = schedule.TotalSlots - schedule.AvailableSlots + 1,
                Status = AppointmentStatus.booked,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            var createdAppointment = await _appointmentRepo.CreateAsync(appointment);
            Console.WriteLine($"Created appointment with ID: {createdAppointment.AppointmentId}");

            // Create transaction after appointment is saved
            var transaction = new Transaction
            {
                AppointmentId = createdAppointment.AppointmentId,
                PatientId = patientId,
                PaymentId = paymentId,
                NIC = request.NIC,
                ContactNo = request.ContactNo,
                Email = request.Email,
                Amount = request.Payment.Amount,
                Status = TransactionStatus.completed,
                PaymentMethod = "bank-transfer",
                BankName = request.Payment.BankName,
                BankBranch = request.Payment.BankBranch
            };

            Console.WriteLine($"Creating transaction for appointment: {createdAppointment.AppointmentId}");
            var createdTransaction = await _transactionRepo.CreateAsync(transaction);
            Console.WriteLine($"Created transaction with ID: {createdTransaction.TransactionId}");

            // Update available slots
            schedule.AvailableSlots--;
            await _scheduleRepo.UpdateAsync(schedule);

            return new BookingResponseDto
            {
                AppointmentId = createdAppointment.AppointmentId,
                TransactionId = createdTransaction.TransactionId,
                PaymentId = paymentId,
                Status = TransactionStatus.completed,
                Amount = request.Payment.Amount,
                PaymentDate = createdTransaction.PaymentDate,
                Message = "Booking successful"
            };
            }
            catch (Exception ex)
            {
                // Log the actual error for debugging
                Console.WriteLine($"Booking error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<BookingResponseDto?> GetBookingAsync(int appointmentId)
        {
            var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
            if (appointment?.Transaction == null)
                return null;

            return new BookingResponseDto
            {
                AppointmentId = appointment.AppointmentId,
                TransactionId = appointment.Transaction.TransactionId,
                PaymentId = appointment.Transaction.PaymentId,
                Status = appointment.Transaction.Status,
                Amount = appointment.Transaction.Amount,
                PaymentDate = appointment.Transaction.PaymentDate,
                Message = "Booking found"
            };
        }

        public async Task<List<UserAppointmentDto>> GetUserAppointmentsAsync(int patientId)
        {
            var appointments = await _appointmentRepo.GetByPatientIdAsync(patientId);
            
            return appointments.Select(a => new UserAppointmentDto
            {
                AppointmentId = a.AppointmentId,
                Doctor = a.DoctorSchedule?.Doctor?.FullName ?? "Unknown",
                Specialization = a.DoctorSchedule?.Doctor?.Specialization ?? "Unknown",
                Price = a.Transaction?.Amount ?? 0,
                Date = a.DoctorSchedule?.ScheduleDate ?? DateTime.MinValue,
                Time = a.DoctorSchedule?.StartTime ?? TimeSpan.Zero,
                Slot = a.SlotNumber,
                Ward = "A-101", // TODO: Add ward to DoctorSchedule or Doctor
                Status = a.Status,
                PaymentId = a.Transaction?.PaymentId ?? "N/A",
                PaymentDate = a.Transaction?.PaymentDate ?? DateTime.MinValue
            }).ToList();
        }
    }
}