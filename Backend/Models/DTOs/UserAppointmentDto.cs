namespace Backend.Models.DTOs
{
    public class UserAppointmentDto
    {
        public int AppointmentId { get; set; }
        public string Doctor { get; set; }
        public string Specialization { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public int Slot { get; set; }
        public string Ward { get; set; }
        public AppointmentStatus Status { get; set; }
        public string PaymentId { get; set; }
        public DateTime PaymentDate { get; set; }
    }
}