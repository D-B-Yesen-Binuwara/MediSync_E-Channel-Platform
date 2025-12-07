using Backend.Models.DTOs;

namespace Backend.Services
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request, int patientId);
        Task<BookingResponseDto?> GetBookingAsync(int appointmentId);
        Task<List<UserAppointmentDto>> GetUserAppointmentsAsync(int patientId);
    }
}