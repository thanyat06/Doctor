using Microsoft.EntityFrameworkCore;
using DoctorAppointment.Backend.Data;
using DoctorAppointment.Backend.DTOs;
using DoctorAppointment.Backend.Enums;
using DoctorAppointment.Backend.Interfaces;
using DoctorAppointment.Backend.Models;

namespace DoctorAppointment.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext context, ITokenService tokenService)
        {
            _context = context;
            _tokenService = tokenService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (emailExists)
            {
                throw new InvalidOperationException($"An account with email '{request.Email}' already exists.");
            }

            if (request.Role == UserRole.Admin)
            {
                throw new InvalidOperationException("Direct registration as Admin is not permitted.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Role = request.Role,
                PhoneNumber = request.PhoneNumber?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            int? doctorId = null;
            int? patientId = null;

            if (request.Role == UserRole.Doctor)
            {
                if (!request.SpecializationId.HasValue)
                {
                    throw new ArgumentException("SpecializationId is required for Doctor registration.");
                }

                var specializationExists = await _context.Specializations.AnyAsync(s => s.Id == request.SpecializationId.Value);
                if (!specializationExists)
                {
                    throw new KeyNotFoundException($"Specialization with ID {request.SpecializationId.Value} does not exist.");
                }

                var doctor = new Doctor
                {
                    UserId = user.Id,
                    SpecializationId = request.SpecializationId.Value,
                    Qualification = request.Qualification?.Trim() ?? "General Practitioner",
                    ExperienceYears = request.ExperienceYears ?? 0,
                    ConsultationFee = request.ConsultationFee ?? 50.00m,
                    Bio = request.Bio?.Trim(),
                    ClinicAddress = request.ClinicAddress?.Trim(),
                    IsAvailable = true
                };

                await _context.Doctors.AddAsync(doctor);
                await _context.SaveChangesAsync();
                doctorId = doctor.Id;
            }
            else if (request.Role == UserRole.Patient)
            {
                var patient = new Patient
                {
                    UserId = user.Id,
                    DateOfBirth = request.DateOfBirth,
                    Gender = request.Gender?.Trim(),
                    BloodGroup = request.BloodGroup?.Trim(),
                    Address = request.Address?.Trim(),
                    EmergencyContact = request.EmergencyContact?.Trim()
                };

                await _context.Patients.AddAsync(patient);
                await _context.SaveChangesAsync();
                patientId = patient.Id;
            }

            var token = _tokenService.GenerateToken(user, doctorId, patientId);

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    PhoneNumber = user.PhoneNumber,
                    DoctorId = doctorId,
                    PatientId = patientId,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .Include(u => u.Doctor)
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email address or password.");
            }

            int? doctorId = user.Doctor?.Id;
            int? patientId = user.Patient?.Id;

            var token = _tokenService.GenerateToken(user, doctorId, patientId);

            return new AuthResponseDto
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(24),
                User = new UserDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role.ToString(),
                    PhoneNumber = user.PhoneNumber,
                    DoctorId = doctorId,
                    PatientId = patientId,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task<UserDto> GetCurrentUserAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Doctor)
                .Include(u => u.Patient)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString(),
                PhoneNumber = user.PhoneNumber,
                DoctorId = user.Doctor?.Id,
                PatientId = user.Patient?.Id,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
