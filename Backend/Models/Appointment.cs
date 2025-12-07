using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public enum AppointmentStatus
    {
        booked,
        cancelled,
        rescheduled
    }

    public class Appointment
    {
    [Key]
    public int AppointmentId { get; set; }

    [Required]
    public int PatientId { get; set; }

    [ForeignKey("PatientId")]
    public User? Patient { get; set; }

    [Required]
    public int ScheduleId { get; set; }

    [ForeignKey("ScheduleId")]
    public DoctorSchedule? DoctorSchedule { get; set; }

    [Required, MaxLength(100)]
    public string? PatientName { get; set; }

    [MaxLength(15)]
    public string? PatientContact { get; set; }

    [Required]
    public int SlotNumber { get; set; }

    [Required]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.booked;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public Transaction? Transaction { get; set; }
    }
}
