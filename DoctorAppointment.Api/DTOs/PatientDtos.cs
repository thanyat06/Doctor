using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Backend.DTOs
{
    public class PatientResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? BloodGroup { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
    }

    public class UpdatePatientDto
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(10)]
        public string? BloodGroup { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        [MaxLength(20)]
        public string? EmergencyContact { get; set; }
    }
}
