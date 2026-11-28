using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Enums;
using DoctorAppointment.Api.Interfaces;
using DoctorAppointment.Api.Models;
using DoctorAppointment.Api.Repositories.Interfaces;

namespace DoctorAppointment.Api.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IDoctorRepository _doctorRepository;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IPatientRepository patientRepository,
            IDoctorRepository doctorRepository)
        {
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _doctorRepository = doctorRepository;
        }

        public async Task<IEnumerable<AppointmentDto>> GetAllAsync(int? doctorId = null, int? patientId = null, AppointmentStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var appointments = await _appointmentRepository.GetAllAsync(doctorId, patientId, status, fromDate, toDate);
            return appointments.Select(MapToDto);
        }

        public async Task<AppointmentDto?> GetByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            return appointment == null ? null : MapToDto(appointment);
        }

        public async Task<AppointmentDto> BookAsync(BookAppointmentDto dto, int patientUserId)
        {
            var patient = await _patientRepository.GetByUserIdAsync(patientUserId)
                ?? throw new InvalidOperationException("Patient profile not found.");

            var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId)
                ?? throw new InvalidOperationException("Doctor not found.");

            if (!doctor.IsAvailable)
                throw new InvalidOperationException("Doctor is not currently available.");

            var timeOnly = TimeOnly.Parse(dto.TimeSlot);
            var slotTaken = await _appointmentRepository.IsSlotTakenAsync(dto.DoctorId, dto.AppointmentDate, timeOnly);
            if (slotTaken)
                throw new InvalidOperationException("This time slot is already booked.");

            var appointment = new Appointment
            {
                DoctorId = dto.DoctorId,
                PatientId = patient.Id,
                AppointmentDate = dto.AppointmentDate.Date,
                TimeSlot = timeOnly,
                ConsultationMode = dto.ConsultationMode,
                Reason = dto.Reason,
                Status = AppointmentStatus.Pending
            };

            var created = await _appointmentRepository.CreateAsync(appointment);
            var full = await _appointmentRepository.GetByIdAsync(created.Id);
            return MapToDto(full!);
        }

        public async Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null) return null;

            if (dto.AppointmentDate.HasValue) appointment.AppointmentDate = dto.AppointmentDate.Value.Date;
            if (dto.TimeSlot != null) appointment.TimeSlot = TimeOnly.Parse(dto.TimeSlot);
            if (dto.ConsultationMode.HasValue) appointment.ConsultationMode = dto.ConsultationMode.Value;
            if (dto.Reason != null) appointment.Reason = dto.Reason;
            if (dto.Notes != null) appointment.Notes = dto.Notes;
            if (dto.Status.HasValue) appointment.Status = dto.Status.Value;

            await _appointmentRepository.UpdateAsync(appointment);
            var updated = await _appointmentRepository.GetByIdAsync(id);
            return MapToDto(updated!);
        }

        public async Task<bool> CancelAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);
            if (appointment == null) return false;
            appointment.Status = AppointmentStatus.Cancelled;
            await _appointmentRepository.UpdateAsync(appointment);
            return true;
        }

        private static AppointmentDto MapToDto(Appointment a) => new AppointmentDto
        {
            Id = a.Id,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor?.User?.Name ?? string.Empty,
            SpecializationName = a.Doctor?.Specialization?.Name ?? string.Empty,
            PatientId = a.PatientId,
            PatientName = a.Patient?.User?.Name ?? string.Empty,
            AppointmentDate = a.AppointmentDate,
            TimeSlot = a.TimeSlot.ToString("HH:mm"),
            Status = a.Status,
            ConsultationMode = a.ConsultationMode,
            Reason = a.Reason,
            Notes = a.Notes,
            CreatedAt = a.CreatedAt
        };
    }
}
