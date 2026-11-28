namespace DoctorAppointment.Api.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int SpecializationId { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public int ExperienceYears { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public User User { get; set; } = null!;
        public Specialization Specialization { get; set; } = null!;
        public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
