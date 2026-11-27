using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoctorAppointment.Backend.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [Required]
        public int SpecializationId { get; set; }

        [ForeignKey(nameof(SpecializationId))]
        public Specialization Specialization { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Qualification { get; set; } = string.Empty;

        [Range(0, 70)]
        public int ExperienceYears { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 100000)]
        public decimal ConsultationFee { get; set; }

        [MaxLength(1000)]
        public string? Bio { get; set; }

        [MaxLength(250)]
        public string? ClinicAddress { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation
        public ICollection<DoctorAvailability> Availabilities { get; set; } = new List<DoctorAvailability>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
