using Microsoft.EntityFrameworkCore;
using DoctorAppointment.Backend.Data;
using DoctorAppointment.Backend.DTOs;
using DoctorAppointment.Backend.Enums;
using DoctorAppointment.Backend.Interfaces;
using DoctorAppointment.Backend.Models;

namespace DoctorAppointment.Backend.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ApplicationDbContext _context;

        public AppointmentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync(AppointmentFilterDto? filter = null)
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .AsQueryable();

            if (filter != null)
            {
                if (filter.DoctorId.HasValue)
                {
                    query = query.Where(a => a.DoctorId == filter.DoctorId.Value);
                }

                if (filter.PatientId.HasValue)
                {
                    query = query.Where(a => a.PatientId == filter.PatientId.Value);
                }

                if (filter.Status.HasValue)
                {
                    query = query.Where(a => a.Status == filter.Status.Value);
                }

                if (filter.FromDate.HasValue)
                {
                    var from = filter.FromDate.Value.Date;
                    query = query.Where(a => a.AppointmentDate.Date >= from);
                }

                if (filter.ToDate.HasValue)
                {
                    var to = filter.ToDate.Value.Date;
                    query = query.Where(a => a.AppointmentDate.Date <= to);
                }
            }

            return await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => MapToResponseDto(a))
                .ToListAsync();
        }

        public async Task<AppointmentResponseDto?> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .FirstOrDefaultAsync(a => a.Id == id);

            return appointment == null ? null : MapToResponseDto(appointment);
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByPatientIdAsync(int patientId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => MapToResponseDto(a))
                .ToListAsync();
        }

        public async Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByDoctorIdAsync(int doctorId)
        {
            return await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .Where(a => a.DoctorId == doctorId)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Select(a => MapToResponseDto(a))
                .ToListAsync();
        }

        public async Task<AppointmentResponseDto> BookAppointmentAsync(int patientUserId, BookAppointmentDto dto)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == patientUserId);

            if (patient == null)
            {
                throw new KeyNotFoundException("Patient profile not found for the logged-in user.");
            }

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);

            if (doctor == null)
            {
                throw new KeyNotFoundException($"Doctor with ID {dto.DoctorId} not found.");
            }

            if (!doctor.IsAvailable)
            {
                throw new InvalidOperationException("Doctor is currently not available for bookings.");
            }

            if (dto.StartTime >= dto.EndTime)
            {
                throw new ArgumentException("Appointment StartTime must be earlier than EndTime.");
            }

            var targetDate = dto.AppointmentDate.Date;
            var appointmentDateTime = targetDate.Add(dto.StartTime);
            if (appointmentDateTime <= DateTime.UtcNow)
            {
                throw new InvalidOperationException("Appointments can only be scheduled for a future date and time.");
            }

            // Check if doctor has active availability for that day and slot
            var dayOfWeek = targetDate.DayOfWeek;
            var isWithinAvailability = await _context.DoctorAvailabilities
                .AnyAsync(a => a.DoctorId == dto.DoctorId
                            && a.DayOfWeek == dayOfWeek
                            && a.IsActive
                            && dto.StartTime >= a.StartTime
                            && dto.EndTime <= a.EndTime);

            if (!isWithinAvailability)
            {
                throw new InvalidOperationException($"The requested time slot is outside of Dr. {doctor.User.FullName}'s working hours on {dayOfWeek}.");
            }

            // Check for double booking with the doctor
            var doctorSlotConflict = await _context.Appointments
                .AnyAsync(a => a.DoctorId == dto.DoctorId
                            && a.AppointmentDate.Date == targetDate
                            && a.Status != AppointmentStatus.Cancelled
                            && a.Status != AppointmentStatus.Rejected
                            && ((dto.StartTime >= a.StartTime && dto.StartTime < a.EndTime)
                                || (dto.EndTime > a.StartTime && dto.EndTime <= a.EndTime)
                                || (dto.StartTime <= a.StartTime && dto.EndTime >= a.EndTime)));

            if (doctorSlotConflict)
            {
                throw new InvalidOperationException("The requested slot is already booked for this doctor. Please choose a different time.");
            }

            // Check for conflict with patient's other appointments
            var patientSlotConflict = await _context.Appointments
                .AnyAsync(a => a.PatientId == patient.Id
                            && a.AppointmentDate.Date == targetDate
                            && a.Status != AppointmentStatus.Cancelled
                            && a.Status != AppointmentStatus.Rejected
                            && ((dto.StartTime >= a.StartTime && dto.StartTime < a.EndTime)
                                || (dto.EndTime > a.StartTime && dto.EndTime <= a.EndTime)
                                || (dto.StartTime <= a.StartTime && dto.EndTime >= a.EndTime)));

            if (patientSlotConflict)
            {
                throw new InvalidOperationException("You already have an existing appointment during this time window.");
            }

            var appointmentNumber = $"APT-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

            var appointment = new Appointment
            {
                AppointmentNumber = appointmentNumber,
                PatientId = patient.Id,
                DoctorId = doctor.Id,
                AppointmentDate = targetDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Status = AppointmentStatus.Pending,
                ConsultationMode = dto.ConsultationMode,
                ReasonForVisit = dto.ReasonForVisit.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Appointments.AddAsync(appointment);
            await _context.SaveChangesAsync();

            appointment.Patient = patient;
            appointment.Doctor = doctor;

            return MapToResponseDto(appointment);
        }

        public async Task<AppointmentResponseDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto, int requestingUserId, string userRole)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with ID {id} not found.");
            }

            // Check role permissions
            if (userRole == UserRole.Doctor.ToString())
            {
                if (appointment.Doctor.UserId != requestingUserId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to update status for another doctor's appointment.");
                }
            }
            else if (userRole == UserRole.Patient.ToString())
            {
                if (appointment.Patient.UserId != requestingUserId)
                {
                    throw new UnauthorizedAccessException("You are not authorized to modify another patient's appointment.");
                }

                if (dto.Status != AppointmentStatus.Cancelled)
                {
                    throw new InvalidOperationException("Patients are only permitted to cancel their appointments.");
                }
            }

            appointment.Status = dto.Status;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (dto.Status == AppointmentStatus.Cancelled && !string.IsNullOrWhiteSpace(dto.Remarks))
            {
                appointment.CancellationReason = dto.Remarks.Trim();
            }

            await _context.SaveChangesAsync();

            return MapToResponseDto(appointment);
        }

        public async Task<AppointmentResponseDto> AddDoctorNotesAsync(int id, DoctorAddNotesDto dto, int doctorUserId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                    .ThenInclude(p => p.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.Specialization)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                throw new KeyNotFoundException($"Appointment with ID {id} not found.");
            }

            if (appointment.Doctor.UserId != doctorUserId)
            {
                throw new UnauthorizedAccessException("Only the assigned doctor can add consultation notes to this appointment.");
            }

            appointment.DoctorNotes = dto.DoctorNotes.Trim();
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponseDto(appointment);
        }

        public async Task<bool> CancelAppointmentAsync(int id, string reason, int requestingUserId, string userRole)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return false;
            }

            if (userRole == UserRole.Doctor.ToString() && appointment.Doctor.UserId != requestingUserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to cancel this appointment.");
            }

            if (userRole == UserRole.Patient.ToString() && appointment.Patient.UserId != requestingUserId)
            {
                throw new UnauthorizedAccessException("You are not authorized to cancel this appointment.");
            }

            if (appointment.Status == AppointmentStatus.Completed)
            {
                throw new InvalidOperationException("A completed appointment cannot be cancelled.");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancellationReason = reason.Trim();
            appointment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        private static AppointmentResponseDto MapToResponseDto(Appointment appointment)
        {
            return new AppointmentResponseDto
            {
                Id = appointment.Id,
                AppointmentNumber = appointment.AppointmentNumber,
                PatientId = appointment.PatientId,
                PatientName = appointment.Patient?.User?.FullName ?? string.Empty,
                PatientPhoneNumber = appointment.Patient?.User?.PhoneNumber,
                DoctorId = appointment.DoctorId,
                DoctorName = appointment.Doctor?.User?.FullName ?? string.Empty,
                SpecializationName = appointment.Doctor?.Specialization?.Name ?? string.Empty,
                ConsultationFee = appointment.Doctor?.ConsultationFee ?? 0m,
                AppointmentDate = appointment.AppointmentDate,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                Status = appointment.Status,
                ConsultationMode = appointment.ConsultationMode,
                ReasonForVisit = appointment.ReasonForVisit,
                DoctorNotes = appointment.DoctorNotes,
                CancellationReason = appointment.CancellationReason,
                CreatedAt = appointment.CreatedAt,
                UpdatedAt = appointment.UpdatedAt
            };
        }
    }
}
