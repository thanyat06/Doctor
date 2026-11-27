using Microsoft.EntityFrameworkCore;
using DoctorAppointment.Backend.Data;
using DoctorAppointment.Backend.DTOs;
using DoctorAppointment.Backend.Enums;
using DoctorAppointment.Backend.Interfaces;
using DoctorAppointment.Backend.Models;

namespace DoctorAppointment.Backend.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly ApplicationDbContext _context;

        public DoctorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorResponseDto>> GetAllDoctorsAsync(DoctorFilterDto? filter = null)
        {
            var query = _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .AsQueryable();

            if (filter != null)
            {
                if (filter.SpecializationId.HasValue)
                {
                    query = query.Where(d => d.SpecializationId == filter.SpecializationId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
                {
                    var search = filter.SearchQuery.Trim().ToLower();
                    query = query.Where(d => d.User.FullName.ToLower().Contains(search)
                                          || d.Specialization.Name.ToLower().Contains(search)
                                          || (d.ClinicAddress != null && d.ClinicAddress.ToLower().Contains(search))
                                          || d.Qualification.ToLower().Contains(search));
                }

                if (filter.MinExperience.HasValue)
                {
                    query = query.Where(d => d.ExperienceYears >= filter.MinExperience.Value);
                }

                if (filter.MaxFee.HasValue)
                {
                    query = query.Where(d => d.ConsultationFee <= filter.MaxFee.Value);
                }

                if (filter.IsAvailable.HasValue)
                {
                    query = query.Where(d => d.IsAvailable == filter.IsAvailable.Value);
                }
            }

            return await query
                .OrderBy(d => d.User.FullName)
                .Select(d => MapToResponseDto(d))
                .ToListAsync();
        }

        public async Task<DoctorResponseDto?> GetDoctorByIdAsync(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == id);

            return doctor == null ? null : MapToResponseDto(doctor);
        }

        public async Task<DoctorResponseDto?> GetDoctorByUserIdAsync(int userId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            return doctor == null ? null : MapToResponseDto(doctor);
        }

        public async Task<DoctorResponseDto> CreateDoctorAsync(CreateDoctorDto dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                throw new InvalidOperationException($"Email '{dto.Email}' is already registered.");
            }

            var specialization = await _context.Specializations.FindAsync(dto.SpecializationId);
            if (specialization == null)
            {
                throw new KeyNotFoundException($"Specialization with ID {dto.SpecializationId} not found.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                FullName = dto.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Role = UserRole.Doctor,
                PhoneNumber = dto.PhoneNumber?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var doctor = new Doctor
            {
                UserId = user.Id,
                SpecializationId = dto.SpecializationId,
                Qualification = dto.Qualification.Trim(),
                ExperienceYears = dto.ExperienceYears,
                ConsultationFee = dto.ConsultationFee,
                Bio = dto.Bio?.Trim(),
                ClinicAddress = dto.ClinicAddress?.Trim(),
                IsAvailable = dto.IsAvailable
            };

            await _context.Doctors.AddAsync(doctor);
            await _context.SaveChangesAsync();

            doctor.User = user;
            doctor.Specialization = specialization;

            return MapToResponseDto(doctor);
        }

        public async Task<DoctorResponseDto> UpdateDoctorAsync(int id, UpdateDoctorDto dto)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialization)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
            {
                throw new KeyNotFoundException($"Doctor with ID {id} not found.");
            }

            if (!string.IsNullOrWhiteSpace(dto.FullName))
            {
                doctor.User.FullName = dto.FullName.Trim();
                doctor.User.UpdatedAt = DateTime.UtcNow;
            }

            if (dto.PhoneNumber != null)
            {
                doctor.User.PhoneNumber = dto.PhoneNumber.Trim();
                doctor.User.UpdatedAt = DateTime.UtcNow;
            }

            if (dto.SpecializationId.HasValue && dto.SpecializationId.Value != doctor.SpecializationId)
            {
                var specialization = await _context.Specializations.FindAsync(dto.SpecializationId.Value);
                if (specialization == null)
                {
                    throw new KeyNotFoundException($"Specialization with ID {dto.SpecializationId.Value} not found.");
                }
                doctor.SpecializationId = dto.SpecializationId.Value;
                doctor.Specialization = specialization;
            }

            if (!string.IsNullOrWhiteSpace(dto.Qualification))
            {
                doctor.Qualification = dto.Qualification.Trim();
            }

            if (dto.ExperienceYears.HasValue)
            {
                doctor.ExperienceYears = dto.ExperienceYears.Value;
            }

            if (dto.ConsultationFee.HasValue)
            {
                doctor.ConsultationFee = dto.ConsultationFee.Value;
            }

            if (dto.Bio != null)
            {
                doctor.Bio = dto.Bio.Trim();
            }

            if (dto.ClinicAddress != null)
            {
                doctor.ClinicAddress = dto.ClinicAddress.Trim();
            }

            if (dto.IsAvailable.HasValue)
            {
                doctor.IsAvailable = dto.IsAvailable.Value;
            }

            await _context.SaveChangesAsync();

            return MapToResponseDto(doctor);
        }

        public async Task<bool> DeleteDoctorAsync(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
            {
                return false;
            }

            _context.Users.Remove(doctor.User);
            await _context.SaveChangesAsync();
            return true;
        }

        private static DoctorResponseDto MapToResponseDto(Doctor doctor)
        {
            return new DoctorResponseDto
            {
                Id = doctor.Id,
                UserId = doctor.UserId,
                FullName = doctor.User.FullName,
                Email = doctor.User.Email,
                PhoneNumber = doctor.User.PhoneNumber,
                SpecializationId = doctor.SpecializationId,
                SpecializationName = doctor.Specialization?.Name ?? string.Empty,
                Qualification = doctor.Qualification,
                ExperienceYears = doctor.ExperienceYears,
                ConsultationFee = doctor.ConsultationFee,
                Bio = doctor.Bio,
                ClinicAddress = doctor.ClinicAddress,
                IsAvailable = doctor.IsAvailable
            };
        }
    }
}
