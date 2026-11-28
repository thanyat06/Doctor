namespace DoctorAppointment.Api.DTOs
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public int SpecializationId { get; set; }
        public string SpecializationName { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDoctorDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public int SpecializationId { get; set; }
        public int ExperienceYears { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? Bio { get; set; }
    }

    public class UpdateDoctorDto
    {
        public string? Name { get; set; }
        public string? LicenseNumber { get; set; }
        public int? SpecializationId { get; set; }
        public int? ExperienceYears { get; set; }
        public decimal? ConsultationFee { get; set; }
        public string? Bio { get; set; }
        public bool? IsAvailable { get; set; }
    }
}
