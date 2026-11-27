using System.ComponentModel.DataAnnotations;
using DoctorAppointment.Backend.Enums;

namespace DoctorAppointment.Backend.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        // Doctor specific fields (if Role == Doctor)
        public int? SpecializationId { get; set; }
        public string? Qualification { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal? ConsultationFee { get; set; }
        public string? Bio { get; set; }
        public string? ClinicAddress { get; set; }

        // Patient specific fields (if Role == Patient)
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? BloodGroup { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
    }

    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public UserDto User { get; set; } = null!;
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
