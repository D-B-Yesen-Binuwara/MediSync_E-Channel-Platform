using System.ComponentModel.DataAnnotations;

namespace Backend.Models.DTOs
{
    public class BookingRequestDto
    {
        [Required]
        public int ScheduleId { get; set; }
        
        [Required, MaxLength(100)]
        public string PatientName { get; set; }
        
        [Required, MaxLength(12)]
        public string NIC { get; set; }
        
        [Required, MaxLength(100)]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required, MaxLength(15)]
        public string ContactNo { get; set; }
        
        [Required]
        public PaymentDetailsDto Payment { get; set; }
    }
    
    public class PaymentDetailsDto
    {
        [Required, MaxLength(100)]
        public string AccountName { get; set; }
        
        [Required, MaxLength(24)]
        public string AccountNumber { get; set; }
        
        [Required, MaxLength(100)]
        public string BankName { get; set; }
        
        [Required, MaxLength(50)]
        public string BankBranch { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
    }
}