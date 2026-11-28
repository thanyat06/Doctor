using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Interfaces;
using DoctorAppointment.Api.Models;
using DoctorAppointment.Api.Repositories.Interfaces;

namespace DoctorAppointment.Api.Services
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<PatientDto>> GetAllAsync(string? search = null)
        {
            var patients = await _patientRepository.GetAllAsync(search);
            return patients.Select(MapToDto);
        }

        public async Task<PatientDto?> GetByIdAsync(int id)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            return patient == null ? null : MapToDto(patient);
        }

        public async Task<PatientDto?> GetByUserIdAsync(int userId)
        {
            var patient = await _patientRepository.GetByUserIdAsync(userId);
            return patient == null ? null : MapToDto(patient);
        }

        public async Task<PatientDto?> UpdateAsync(int id, UpdatePatientDto dto)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null) return null;

            if (dto.Name != null) patient.User.Name = dto.Name;
            if (dto.PhoneNumber != null) patient.PhoneNumber = dto.PhoneNumber;
            if (dto.DateOfBirth.HasValue) patient.DateOfBirth = dto.DateOfBirth.Value;
            if (dto.BloodGroup != null) patient.BloodGroup = dto.BloodGroup;
            if (dto.Address != null) patient.Address = dto.Address;
            if (dto.EmergencyContact != null) patient.EmergencyContact = dto.EmergencyContact;

            await _patientRepository.UpdateAsync(patient);
            var updated = await _patientRepository.GetByIdAsync(id);
            return MapToDto(updated!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _patientRepository.DeleteAsync(id);
        }

        private static PatientDto MapToDto(Patient p) => new PatientDto
        {
            Id = p.Id,
            UserId = p.UserId,
            Name = p.User.Name,
            Email = p.User.Email,
            PhoneNumber = p.PhoneNumber,
            DateOfBirth = p.DateOfBirth,
            BloodGroup = p.BloodGroup,
            Address = p.Address,
            EmergencyContact = p.EmergencyContact,
            CreatedAt = p.CreatedAt
        };
    }
}
