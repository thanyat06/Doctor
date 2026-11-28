namespace DoctorAppointment.Api.DTOs
{
    public class PatientDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? BloodGroup { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdatePatientDto
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? BloodGroup { get; set; }
        public string? Address { get; set; }
        public string? EmergencyContact { get; set; }
    }
}
