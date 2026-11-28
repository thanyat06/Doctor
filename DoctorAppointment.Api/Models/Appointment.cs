using DoctorAppointment.Api.Enums;

namespace DoctorAppointment.Api.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int PatientId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeOnly TimeSlot { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
        public ConsultationMode ConsultationMode { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public Doctor Doctor { get; set; } = null!;
        public Patient Patient { get; set; } = null!;
    }
}
