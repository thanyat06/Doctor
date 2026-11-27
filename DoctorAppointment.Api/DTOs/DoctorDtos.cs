using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Backend.DTOs
{
    public class DoctorResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? Bio { get; set; }
        public string? ClinicAddress { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class CreateDoctorDto
    {
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        public int SpecializationId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Qualification { get; set; } = string.Empty;

        [Range(0, 70)]
        public int ExperienceYears { get; set; }

        [Range(0, 100000)]
        public decimal ConsultationFee { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [MaxLength(250)]
        public string? ClinicAddress { get; set; }

        public bool IsAvailable { get; set; } = true;
    }

    public class UpdateDoctorDto
    {
        [MaxLength(100)]
        public string? FullName { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public int? SpecializationId { get; set; }

        [MaxLength(100)]
        public string? Qualification { get; set; }

        [Range(0, 70)]
        public int? ExperienceYears { get; set; }

        [Range(0, 100000)]
        public decimal? ConsultationFee { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [MaxLength(250)]
        public string? ClinicAddress { get; set; }

        public bool? IsAvailable { get; set; }
    }

    public class DoctorFilterDto
    {
        public int? SpecializationId { get; set; }
        public string? SearchQuery { get; set; }
        public int? MinExperience { get; set; }
        public decimal? MaxFee { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
