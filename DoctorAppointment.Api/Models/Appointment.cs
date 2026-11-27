using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DoctorAppointment.Backend.Enums;

namespace DoctorAppointment.Backend.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string AppointmentNumber { get; set; } = string.Empty;

        [Required]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient Patient { get; set; } = null!;

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public Doctor Doctor { get; set; } = null!;

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

        [Required]
        public ConsultationMode ConsultationMode { get; set; } = ConsultationMode.InPerson;

        [Required]
        [MaxLength(500)]
        public string ReasonForVisit { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? DoctorNotes { get; set; }

        [MaxLength(500)]
        public string? CancellationReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
