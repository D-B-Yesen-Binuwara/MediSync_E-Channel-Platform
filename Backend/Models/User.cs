// using System;
// using System.Collections.Generic; // Needed for ICollection
// using System.ComponentModel.DataAnnotations;

// namespace Backend.Models
// {
//     public enum UserRole { Patient, Admin } // Capitalized for convention

//     public class User
//     {
//         [Key]
//         public int UserId { get; set; }

//         [Required, MaxLength(100)]
//         public required string FullName { get; set; }

//         [Required, MaxLength(100)]
//         [EmailAddress]
//         public required string Email { get; set; }

//         [Required, MaxLength(255)]
//         public required string PasswordHash { get; set; }

//         // NIC is required but limited to 12 chars -> good
//         [Required, MaxLength(12)]
//         public required string NIC { get; set; }

//         [Required]
//         public UserRole Role { get; set; }

//         [MaxLength(13)]
//         public string? ContactNumber { get; set; }

//         // Password reset support
//         public string? PasswordResetToken { get; set; }
//         public DateTime? PasswordResetTokenExpiresUtc { get; set; }
        
//         // Clerk integration
//         public string? ClerkUserId { get; set; }

//         public ICollection<Appointment>? Appointments { get; set; }
//         public ICollection<Favorite>? Favorites { get; set; }
//         public ICollection<Transaction>? Transactions { get; set; }
//     }
// }

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public enum UserRole { Patient, Admin }

    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public required string FullName { get; set; }

        [Required, MaxLength(100)]
        [EmailAddress]
        public required string Email { get; set; }

        [Required, MaxLength(255)]
        public required string PasswordHash { get; set; }

        [Required, MaxLength(12)]
        public required string NIC { get; set; }

        [Required]
        public UserRole Role { get; set; }

        [MaxLength(13)]
        public string? ContactNumber { get; set; }

        public byte[]? ProfileImage { get; set; } // Store image as binary

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiresUtc { get; set; }

        public string? ClerkUserId { get; set; }

        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Favorite>? Favorites { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
    }
}