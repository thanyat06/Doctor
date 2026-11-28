using DoctorAppointment.Api.Data;
using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Enums;
using DoctorAppointment.Api.Interfaces;
using DoctorAppointment.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointment.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenService _tokenService;

        public AuthService(ApplicationDbContext context, TokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                throw new InvalidOperationException("Email already registered.");

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
                throw new InvalidOperationException("Invalid role. Use 'Doctor' or 'Patient'.");

            if (role == UserRole.Admin)
                throw new InvalidOperationException("Cannot register as Admin.");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = role,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            int profileId = 0;

            if (role == UserRole.Doctor)
            {
                if (dto.SpecializationId == null || dto.LicenseNumber == null)
                    throw new InvalidOperationException("SpecializationId and LicenseNumber are required for Doctor registration.");

                var doctor = new Doctor
                {
                    UserId = user.Id,
                    SpecializationId = dto.SpecializationId.Value,
                    LicenseNumber = dto.LicenseNumber,
                    ExperienceYears = dto.ExperienceYears ?? 0,
                    ConsultationFee = dto.ConsultationFee ?? 500,
                    Bio = dto.Bio,
                    IsAvailable = true
                };
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
                profileId = doctor.Id;
            }
            else if (role == UserRole.Patient)
            {
                var patient = new Patient
                {
                    UserId = user.Id,
                    PhoneNumber = dto.PhoneNumber ?? string.Empty
                };
                _context.Patients.Add(patient);
                await _context.SaveChangesAsync();
                profileId = patient.Id;
            }

            var token = _tokenService.GenerateToken(user, profileId);
            return new AuthResponseDto(token, user.Name, user.Email, user.Role.ToString(), user.Id, profileId);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
                ?? throw new UnauthorizedAccessException("Invalid credentials.");

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is disabled.");

            int profileId = 0;
            if (user.Role == UserRole.Doctor)
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                profileId = doctor?.Id ?? 0;
            }
            else if (user.Role == UserRole.Patient)
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == user.Id);
                profileId = patient?.Id ?? 0;
            }

            var token = _tokenService.GenerateToken(user, profileId);
            return new AuthResponseDto(token, user.Name, user.Email, user.Role.ToString(), user.Id, profileId);
        }
    }
}
