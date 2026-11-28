using DoctorAppointment.Api.Enums;

namespace DoctorAppointment.Api.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public AppointmentStatus Status { get; set; }
        public ConsultationMode ConsultationMode { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class BookAppointmentDto
    {
        public int DoctorId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = string.Empty; // "HH:mm"
        public ConsultationMode ConsultationMode { get; set; }
        public string? Reason { get; set; }
    }

    public class UpdateAppointmentDto
    {
        public DateTime? AppointmentDate { get; set; }
        public string? TimeSlot { get; set; }
        public ConsultationMode? ConsultationMode { get; set; }
        public string? Reason { get; set; }
        public string? Notes { get; set; }
        public AppointmentStatus? Status { get; set; }
    }
}
