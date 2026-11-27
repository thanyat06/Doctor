using Microsoft.EntityFrameworkCore;
using DoctorAppointment.Backend.Data;
using DoctorAppointment.Backend.DTOs;
using DoctorAppointment.Backend.Enums;
using DoctorAppointment.Backend.Interfaces;
using DoctorAppointment.Backend.Models;

namespace DoctorAppointment.Backend.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly ApplicationDbContext _context;

        public AvailabilityService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AvailabilityResponseDto>> GetAvailabilitiesByDoctorIdAsync(int doctorId)
        {
            var doctorExists = await _context.Doctors.AnyAsync(d => d.Id == doctorId);
            if (!doctorExists)
            {
                throw new KeyNotFoundException($"Doctor with ID {doctorId} not found.");
            }

            return await _context.DoctorAvailabilities
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Where(a => a.DoctorId == doctorId)
                .OrderBy(a => a.DayOfWeek)
                .ThenBy(a => a.StartTime)
                .Select(a => MapToResponseDto(a))
                .ToListAsync();
        }

        public async Task<AvailabilityResponseDto> SetAvailabilityAsync(CreateAvailabilityDto dto)
        {
            if (!dto.DoctorId.HasValue)
            {
                throw new ArgumentException("DoctorId is required.");
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId.Value);

            if (doctor == null)
            {
                throw new KeyNotFoundException($"Doctor with ID {dto.DoctorId.Value} not found.");
            }

            if (dto.StartTime >= dto.EndTime)
            {
                throw new ArgumentException("StartTime must be before EndTime.");
            }

            // Check if availability for the same doctor and day overlaps
            var hasOverlap = await _context.DoctorAvailabilities
                .AnyAsync(a => a.DoctorId == dto.DoctorId.Value
                            && a.DayOfWeek == dto.DayOfWeek
                            && a.IsActive
                            && ((dto.StartTime >= a.StartTime && dto.StartTime < a.EndTime)
                                || (dto.EndTime > a.StartTime && dto.EndTime <= a.EndTime)
                                || (dto.StartTime <= a.StartTime && dto.EndTime >= a.EndTime)));

            if (hasOverlap)
            {
                throw new InvalidOperationException($"An active availability window already overlaps with {dto.DayOfWeek} {dto.StartTime}-{dto.EndTime} for this doctor.");
            }

            var availability = new DoctorAvailability
            {
                DoctorId = dto.DoctorId.Value,
                DayOfWeek = dto.DayOfWeek,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                SlotDurationMinutes = dto.SlotDurationMinutes > 0 ? dto.SlotDurationMinutes : 30,
                IsActive = dto.IsActive
            };

            await _context.DoctorAvailabilities.AddAsync(availability);
            await _context.SaveChangesAsync();

            availability.Doctor = doctor;
            return MapToResponseDto(availability);
        }

        public async Task<AvailabilityResponseDto> UpdateAvailabilityAsync(int id, UpdateAvailabilityDto dto)
        {
            var availability = await _context.DoctorAvailabilities
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (availability == null)
            {
                throw new KeyNotFoundException($"Doctor availability with ID {id} not found.");
            }

            var newDay = dto.DayOfWeek ?? availability.DayOfWeek;
            var newStart = dto.StartTime ?? availability.StartTime;
            var newEnd = dto.EndTime ?? availability.EndTime;

            if (newStart >= newEnd)
            {
                throw new ArgumentException("StartTime must be before EndTime.");
            }

            availability.DayOfWeek = newDay;
            availability.StartTime = newStart;
            availability.EndTime = newEnd;

            if (dto.SlotDurationMinutes.HasValue && dto.SlotDurationMinutes.Value > 0)
            {
                availability.SlotDurationMinutes = dto.SlotDurationMinutes.Value;
            }

            if (dto.IsActive.HasValue)
            {
                availability.IsActive = dto.IsActive.Value;
            }

            await _context.SaveChangesAsync();

            return MapToResponseDto(availability);
        }

        public async Task<bool> DeleteAvailabilityAsync(int id)
        {
            var availability = await _context.DoctorAvailabilities.FindAsync(id);
            if (availability == null)
            {
                return false;
            }

            _context.DoctorAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<DoctorSlotsResponseDto> GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null)
            {
                throw new KeyNotFoundException($"Doctor with ID {doctorId} not found.");
            }

            var dayOfWeek = date.DayOfWeek;

            var availabilities = await _context.DoctorAvailabilities
                .Where(a => a.DoctorId == doctorId && a.DayOfWeek == dayOfWeek && a.IsActive)
                .ToListAsync();

            var targetDate = date.Date;

            // Fetch existing booked appointments for that doctor on the specified date
            var bookedAppointments = await _context.Appointments
                .Where(a => a.DoctorId == doctorId
                         && a.AppointmentDate.Date == targetDate
                         && a.Status != AppointmentStatus.Cancelled
                         && a.Status != AppointmentStatus.Rejected)
                .Select(a => new { a.StartTime, a.EndTime })
                .ToListAsync();

            var slots = new List<TimeSlotDto>();

            foreach (var availability in availabilities)
            {
                var current = availability.StartTime;
                var slotDuration = TimeSpan.FromMinutes(availability.SlotDurationMinutes);

                while (current + slotDuration <= availability.EndTime)
                {
                    var slotEnd = current + slotDuration;

                    var isBooked = bookedAppointments.Any(b =>
                        (current >= b.StartTime && current < b.EndTime) ||
                        (slotEnd > b.StartTime && slotEnd <= b.EndTime) ||
                        (current <= b.StartTime && slotEnd >= b.EndTime));

                    slots.Add(new TimeSlotDto
                    {
                        StartTime = current,
                        EndTime = slotEnd,
                        IsAvailable = !isBooked
                    });

                    current = slotEnd;
                }
            }

            return new DoctorSlotsResponseDto
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.User.FullName,
                Date = targetDate,
                AvailableSlots = slots.OrderBy(s => s.StartTime).ToList()
            };
        }

        private static AvailabilityResponseDto MapToResponseDto(DoctorAvailability availability)
        {
            return new AvailabilityResponseDto
            {
                Id = availability.Id,
                DoctorId = availability.DoctorId,
                DoctorName = availability.Doctor?.User?.FullName ?? string.Empty,
                DayOfWeek = availability.DayOfWeek,
                StartTime = availability.StartTime,
                EndTime = availability.EndTime,
                SlotDurationMinutes = availability.SlotDurationMinutes,
                IsActive = availability.IsActive
            };
        }
    }
}
