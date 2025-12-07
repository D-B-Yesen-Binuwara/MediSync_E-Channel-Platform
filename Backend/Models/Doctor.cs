using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }

        [Required, MaxLength(100)]
        public required string FullName { get; set; }

        [Required, MaxLength(50)]
        public required string Specialization { get; set; }

        [Required, MaxLength(12)]
        [RegularExpression(@"^\d{9}[Vv]|\d{12}$", ErrorMessage = "NIC must be either 9 digits followed by 'V' or 12 digits")]
        public required string NIC { get; set; }

        [MaxLength(200)]
        public string? Qualification { get; set; }

        [Required, MaxLength(50)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required, MaxLength(15)]
        [Phone]
        public required string ContactNo { get; set; }

        [Required, MaxLength(500)]
        public required string Details { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<DoctorSchedule>? DoctorSchedules { get; set; }
        public ICollection<Favorite>? Favorites { get; set; }
    }
}