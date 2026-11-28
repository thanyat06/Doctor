namespace DoctorAppointment.Api.DTOs
{
    public class AvailabilityDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int SlotDurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public List<string> AvailableSlots { get; set; } = new();
    }

    public class CreateAvailabilityDto
    {
        public int DoctorId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public string StartTime { get; set; } = "09:00"; // HH:mm
        public string EndTime { get; set; } = "17:00";   // HH:mm
        public int SlotDurationMinutes { get; set; } = 30;
    }

    public class UpdateAvailabilityDto
    {
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public int? SlotDurationMinutes { get; set; }
        public bool? IsActive { get; set; }
    }
}
