using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<BookingResponseDto>> CreateBooking([FromBody] BookingRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
                if (!int.TryParse(userIdClaim, out int patientId))
                    return Unauthorized(new { message = "Invalid token" });

                var result = await _bookingService.CreateBookingAsync(request, patientId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your booking" });
            }
        }

        [HttpGet("{appointmentId}")]
        public async Task<ActionResult<BookingResponseDto>> GetBooking(int appointmentId)
        {
            try
            {
                var result = await _bookingService.GetBookingAsync(appointmentId);
                if (result == null)
                    return NotFound(new { message = "Booking not found" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the booking" });
            }
        }

        [HttpGet("user")]
        [Authorize]
        public async Task<ActionResult<List<UserAppointmentDto>>> GetUserAppointments()
        {
            try
            {
                // Get user ID from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
                if (!int.TryParse(userIdClaim, out int patientId))
                    return Unauthorized(new { message = "Invalid token" });

                var appointments = await _bookingService.GetUserAppointmentsAsync(patientId);
                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving appointments" });
            }
        }
    }
}