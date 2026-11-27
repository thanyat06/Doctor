namespace DoctorAppointment.Api.Models
{
    public class Specialization
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
