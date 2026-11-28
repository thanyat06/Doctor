using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Enums;
using DoctorAppointment.Api.Interfaces;
using DoctorAppointment.Api.Models;
using DoctorAppointment.Api.Repositories.Interfaces;

namespace DoctorAppointment.Api.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IAppointmentRepository _appointmentRepository;

        public AvailabilityService(IAvailabilityRepository availabilityRepository, IAppointmentRepository appointmentRepository)
        {
            _availabilityRepository = availabilityRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<IEnumerable<AvailabilityDto>> GetByDoctorIdAsync(int doctorId)
        {
            var avails = await _availabilityRepository.GetByDoctorIdAsync(doctorId);
            return avails.Select(a => MapToDto(a, GenerateSlots(a.StartTime, a.EndTime, a.SlotDurationMinutes)));
        }

        public async Task<AvailabilityDto?> GetByIdAsync(int id)
        {
            var avail = await _availabilityRepository.GetByIdAsync(id);
            if (avail == null) return null;
            var slots = GenerateSlots(avail.StartTime, avail.EndTime, avail.SlotDurationMinutes);
            return MapToDto(avail, slots);
        }

        public async Task<AvailabilityDto> CreateAsync(CreateAvailabilityDto dto)
        {
            var startTime = TimeOnly.Parse(dto.StartTime);
            var endTime = TimeOnly.Parse(dto.EndTime);

            var existing = await _availabilityRepository.GetByDoctorAndDayAsync(dto.DoctorId, dto.DayOfWeek);
            if (existing != null)
                throw new InvalidOperationException($"Availability for {dto.DayOfWeek} already exists.");

            var avail = new DoctorAvailability
            {
                DoctorId = dto.DoctorId,
                DayOfWeek = dto.DayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                SlotDurationMinutes = dto.SlotDurationMinutes,
                IsActive = true
            };
            var created = await _availabilityRepository.CreateAsync(avail);
            var full = await _availabilityRepository.GetByIdAsync(created.Id);
            return MapToDto(full!, GenerateSlots(full!.StartTime, full.EndTime, full.SlotDurationMinutes));
        }

        public async Task<AvailabilityDto?> UpdateAsync(int id, UpdateAvailabilityDto dto)
        {
            var avail = await _availabilityRepository.GetByIdAsync(id);
            if (avail == null) return null;

            if (dto.StartTime != null) avail.StartTime = TimeOnly.Parse(dto.StartTime);
            if (dto.EndTime != null) avail.EndTime = TimeOnly.Parse(dto.EndTime);
            if (dto.SlotDurationMinutes.HasValue) avail.SlotDurationMinutes = dto.SlotDurationMinutes.Value;
            if (dto.IsActive.HasValue) avail.IsActive = dto.IsActive.Value;

            await _availabilityRepository.UpdateAsync(avail);
            var updated = await _availabilityRepository.GetByIdAsync(id);
            return MapToDto(updated!, GenerateSlots(updated!.StartTime, updated.EndTime, updated.SlotDurationMinutes));
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _availabilityRepository.DeleteAsync(id);
        }

        public async Task<List<string>> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            var avail = await _availabilityRepository.GetByDoctorAndDayAsync(doctorId, dayOfWeek);
            if (avail == null || !avail.IsActive) return new List<string>();

            var allSlots = GenerateSlots(avail.StartTime, avail.EndTime, avail.SlotDurationMinutes);
            var appointments = await _appointmentRepository.GetAllAsync(doctorId: doctorId, fromDate: date.Date, toDate: date.Date);
            var bookedSlots = appointments
                .Where(a => a.Status != AppointmentStatus.Cancelled)
                .Select(a => a.TimeSlot.ToString("HH:mm"))
                .ToHashSet();

            return allSlots.Where(s => !bookedSlots.Contains(s)).ToList();
        }

        private static List<string> GenerateSlots(TimeOnly start, TimeOnly end, int durationMinutes)
        {
            var slots = new List<string>();
            var current = start;
            while (current.AddMinutes(durationMinutes) <= end)
            {
                slots.Add(current.ToString("HH:mm"));
                current = current.AddMinutes(durationMinutes);
            }
            return slots;
        }

        private static AvailabilityDto MapToDto(DoctorAvailability da, List<string> slots) => new AvailabilityDto
        {
            Id = da.Id,
            DoctorId = da.DoctorId,
            DoctorName = da.Doctor?.User?.Name ?? string.Empty,
            DayOfWeek = da.DayOfWeek.ToString(),
            StartTime = da.StartTime.ToString("HH:mm"),
            EndTime = da.EndTime.ToString("HH:mm"),
            SlotDurationMinutes = da.SlotDurationMinutes,
            IsActive = da.IsActive,
            AvailableSlots = slots
        };
    }
}
