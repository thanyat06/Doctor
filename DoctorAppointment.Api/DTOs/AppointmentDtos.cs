using System.ComponentModel.DataAnnotations;
using DoctorAppointment.Backend.Enums;

namespace DoctorAppointment.Backend.DTOs
{
    public class AppointmentResponseDto
    {
        public int Id { get; set; }
        public string AppointmentNumber { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? PatientPhoneNumber { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string SpecializationName { get; set; } = string.Empty;
        public decimal ConsultationFee { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string FormattedTime => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public AppointmentStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public ConsultationMode ConsultationMode { get; set; }
        public string ConsultationModeName => ConsultationMode.ToString();
        public string ReasonForVisit { get; set; } = string.Empty;
        public string? DoctorNotes { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class BookAppointmentDto
    {
        [Required]
        public int DoctorId { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        public ConsultationMode ConsultationMode { get; set; } = ConsultationMode.InPerson;

        [Required]
        [MaxLength(500)]
        public string ReasonForVisit { get; set; } = string.Empty;
    }

    public class UpdateAppointmentStatusDto
    {
        [Required]
        public AppointmentStatus Status { get; set; }

        [MaxLength(500)]
        public string? Remarks { get; set; }
    }

    public class DoctorAddNotesDto
    {
        [Required]
        [MaxLength(2000)]
        public string DoctorNotes { get; set; } = string.Empty;
    }

    public class CancelAppointmentDto
    {
        [Required]
        [MaxLength(500)]
        public string CancellationReason { get; set; } = string.Empty;
    }

    public class AppointmentFilterDto
    {
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public AppointmentStatus? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
