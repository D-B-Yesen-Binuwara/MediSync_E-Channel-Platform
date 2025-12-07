using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs
{
    public class ResetPasswordDto
    {
        [Required]
        public string token { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string newPassword { get; set; } = string.Empty;
    }
}