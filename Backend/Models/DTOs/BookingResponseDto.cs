namespace Backend.Models.DTOs
{
    public class BookingResponseDto
    {
        public int AppointmentId { get; set; }
        public int TransactionId { get; set; }
        public string PaymentId { get; set; }
        public TransactionStatus Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Message { get; set; }
    }
}