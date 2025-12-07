using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    // Simple model for storing which doctors a patient has favorited
    // Pretty straightforward - just links a patient to a doctor
    public class Favorite
    {
        [Key]
        public int FavoriteId { get; set; } // primary key, auto-generated

        [Required]
        public int PatientId { get; set; } // who favorited this doctor

        [ForeignKey("PatientId")]
        public User Patient { get; set; } // navigation property to the user

        [Required]
        public int DoctorId { get; set; } // which doctor was favorited

        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; } // navigation property to the doctor

        // Track when the favorite was added - might be useful for analytics later
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
