using System.ComponentModel.DataAnnotations;

namespace DoctorAppointment.Backend.DTOs
{
    public class AvailabilityResponseDto
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DayOfWeek DayOfWeek { get; set; }
        public string DayOfWeekName => DayOfWeek.ToString();
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDurationMinutes { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateAvailabilityDto
    {
        public int? DoctorId { get; set; }

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Range(10, 180)]
        public int SlotDurationMinutes { get; set; } = 30;

        public bool IsActive { get; set; } = true;
    }

    public class UpdateAvailabilityDto
    {
        public DayOfWeek? DayOfWeek { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        [Range(10, 180)]
        public int? SlotDurationMinutes { get; set; }

        public bool? IsActive { get; set; }
    }

    public class TimeSlotDto
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string FormattedSlot => $"{StartTime:hh\\:mm} - {EndTime:hh\\:mm}";
        public bool IsAvailable { get; set; }
    }

    public class DoctorSlotsResponseDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string DayOfWeek => Date.DayOfWeek.ToString();
        public List<TimeSlotDto> AvailableSlots { get; set; } = new List<TimeSlotDto>();
    }
}
