using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Enums;
using DoctorAppointment.Api.Interfaces;
using DoctorAppointment.Api.Models;
using DoctorAppointment.Api.Repositories.Interfaces;
using DoctorAppointment.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointment.Api.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly ApplicationDbContext _context;

        public DoctorService(IDoctorRepository doctorRepository, ApplicationDbContext context)
        {
            _doctorRepository = doctorRepository;
            _context = context;
        }

        public async Task<IEnumerable<DoctorDto>> GetAllAsync(string? search = null, int? specializationId = null, bool? isAvailable = null)
        {
            var doctors = await _doctorRepository.GetAllAsync(search, specializationId, isAvailable);
            return doctors.Select(MapToDto);
        }

        public async Task<DoctorDto?> GetByIdAsync(int id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            return doctor == null ? null : MapToDto(doctor);
        }

        public async Task<DoctorDto?> GetByUserIdAsync(int userId)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(userId);
            return doctor == null ? null : MapToDto(doctor);
        }

        public async Task<DoctorDto> CreateAsync(CreateDoctorDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already registered.");

            if (await _doctorRepository.LicenseExistsAsync(dto.LicenseNumber))
                throw new InvalidOperationException("License number already in use.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.Doctor
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var doctor = new Doctor
            {
                UserId = user.Id,
                SpecializationId = dto.SpecializationId,
                LicenseNumber = dto.LicenseNumber,
                ExperienceYears = dto.ExperienceYears,
                ConsultationFee = dto.ConsultationFee,
                Bio = dto.Bio,
                IsAvailable = true
            };
            var created = await _doctorRepository.CreateAsync(doctor);
            var full = await _doctorRepository.GetByIdAsync(created.Id);
            return MapToDto(full!);
        }

        public async Task<DoctorDto?> UpdateAsync(int id, UpdateDoctorDto dto)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            if (doctor == null) return null;

            if (dto.Name != null) doctor.User.Name = dto.Name;
            if (dto.SpecializationId.HasValue) doctor.SpecializationId = dto.SpecializationId.Value;
            if (dto.ExperienceYears.HasValue) doctor.ExperienceYears = dto.ExperienceYears.Value;
            if (dto.ConsultationFee.HasValue) doctor.ConsultationFee = dto.ConsultationFee.Value;
            if (dto.Bio != null) doctor.Bio = dto.Bio;
            if (dto.IsAvailable.HasValue) doctor.IsAvailable = dto.IsAvailable.Value;
            if (dto.LicenseNumber != null) doctor.LicenseNumber = dto.LicenseNumber;

            await _doctorRepository.UpdateAsync(doctor);
            var updated = await _doctorRepository.GetByIdAsync(id);
            return MapToDto(updated!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _doctorRepository.DeleteAsync(id);
        }

        private static DoctorDto MapToDto(Doctor d) => new DoctorDto
        {
            Id = d.Id,
            UserId = d.UserId,
            Name = d.User.Name,
            Email = d.User.Email,
            LicenseNumber = d.LicenseNumber,
            SpecializationId = d.SpecializationId,
            SpecializationName = d.Specialization?.Name ?? string.Empty,
            ExperienceYears = d.ExperienceYears,
            ConsultationFee = d.ConsultationFee,
            Bio = d.Bio,
            ProfileImageUrl = d.ProfileImageUrl,
            IsAvailable = d.IsAvailable,
            CreatedAt = d.CreatedAt
        };
    }
}
