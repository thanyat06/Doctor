using Microsoft.EntityFrameworkCore;
using DoctorAppointment.Backend.Data;
using DoctorAppointment.Backend.DTOs;
using DoctorAppointment.Backend.Interfaces;
using DoctorAppointment.Backend.Models;

namespace DoctorAppointment.Backend.Services
{
    public class PatientService : IPatientService
    {
        private readonly ApplicationDbContext _context;

        public PatientService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PatientResponseDto>> GetAllPatientsAsync()
        {
            return await _context.Patients
                .Include(p => p.User)
                .OrderBy(p => p.User.FullName)
                .Select(p => MapToResponseDto(p))
                .ToListAsync();
        }

        public async Task<PatientResponseDto?> GetPatientByIdAsync(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            return patient == null ? null : MapToResponseDto(patient);
        }

        public async Task<PatientResponseDto?> GetPatientByUserIdAsync(int userId)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            return patient == null ? null : MapToResponseDto(patient);
        }

        public async Task<PatientResponseDto> UpdatePatientAsync(int id, UpdatePatientDto dto)
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
            {
                throw new KeyNotFoundException($"Patient with ID {id} not found.");
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                patient.User.FullName = dto.FullName.Trim();
                patient.User.UpdatedAt = DateTime.UtcNow;
            }

            if (dto.PhoneNumber != null)
            {
                patient.User.PhoneNumber = dto.PhoneNumber.Trim();
                patient.User.UpdatedAt = DateTime.UtcNow;
            }

            if (dto.DateOfBirth.HasValue)
            {
                patient.DateOfBirth = dto.DateOfBirth.Value;
            }

            if (dto.Gender != null)
            {
                patient.Gender = dto.Gender.Trim();
            }

            if (dto.BloodGroup != null)
            {
                patient.BloodGroup = dto.BloodGroup.Trim();
            }

            if (dto.Address != null)
            {
                patient.Address = dto.Address.Trim();
            }

            if (dto.EmergencyContact != null)
            {
                patient.EmergencyContact = dto.EmergencyContact.Trim();
            }

            await _context.SaveChangesAsync();

            return MapToResponseDto(patient);
        }

        private static PatientResponseDto MapToResponseDto(Patient patient)
        {
            return new PatientResponseDto
            {
                Id = patient.Id,
                UserId = patient.UserId,
                FullName = patient.User.FullName,
                Email = patient.User.Email,
                PhoneNumber = patient.User.PhoneNumber,
                DateOfBirth = patient.DateOfBirth,
                Gender = patient.Gender,
                BloodGroup = patient.BloodGroup,
                Address = patient.Address,
                EmergencyContact = patient.EmergencyContact
            };
        }
    }
}
