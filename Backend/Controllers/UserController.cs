using Backend.Models.DTOs;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    // User account management controller
    // Handles profile updates, password changes, account deletion etc.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // all endpoints require authentication
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Extract user ID from JWT token claims
        // Returns null if token is invalid or missing user ID
        private int? GetUserIdFromToken()
        {
            // Try both standard claim types because different auth systems use different names
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            return null; // couldn't parse or claim doesn't exist
        }

        // GET /api/user/profile - fetch current user's profile info
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized(new { message = "Invalid token" });

                var profile = await _userService.GetProfileAsync(userId.Value);
                return Ok(profile); // return user's profile data
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving profile" });
            }
        }

        // Update user profile - name, email, phone etc.
        // PUT because we're updating the entire profile resource
        [HttpPut("profile")]
        public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileDto request)
        {
            try
            {
                // Validate the incoming data first
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized(new { message = "Invalid token" });

                var updatedProfile = await _userService.UpdateProfileAsync(userId.Value, request);
                return Ok(updatedProfile); // return the updated profile
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating profile" });
            }
        }

        // Change user password - requires current password for security
        [HttpPost("change-password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordDto request)
        {
            Console.WriteLine("ChangePassword controller method called"); // debug logging
            try
            {
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState is invalid");
                    return BadRequest(ModelState);
                }

                var userId = GetUserIdFromToken();
                Console.WriteLine($"Extracted userId: {userId}");
                if (userId == null) return Unauthorized(new { message = "Invalid token" });

                // Let the service handle password validation and hashing
                await _userService.ChangePasswordAsync(userId.Value, request);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"ArgumentException: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An error occurred while changing password" });
            }
        }

        // Delete user account permanently - this is irreversible!
        // Maybe we should add some confirmation mechanism here...
        [HttpDelete]
        public async Task<ActionResult> DeleteAccount()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized(new { message = "Invalid token" });

                // This will cascade delete all related data (favorites, appointments, etc.)
                await _userService.DeleteAccountAsync(userId.Value);
                return Ok(new { message = "Account deleted successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting account" });
            }
        }

        // Get user's payment/transaction history
        // Useful for showing past appointments and payments
        [HttpGet("transactions")]
        public async Task<ActionResult<List<TransactionDto>>> GetTransactions()
        {
            try
            {
                var userId = GetUserIdFromToken();
                if (userId == null) return Unauthorized(new { message = "Invalid token" });

                var transactions = await _userService.GetTransactionsAsync(userId.Value);
                return Ok(transactions); // list of user's transactions
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving transactions" });
            }
        }
    }
}
