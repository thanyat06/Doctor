using Microsoft.EntityFrameworkCore;
using DoctorAppointment.Backend.Data;
using DoctorAppointment.Backend.DTOs;
using DoctorAppointment.Backend.Interfaces;
using DoctorAppointment.Backend.Models;

namespace DoctorAppointment.Backend.Services
{
    public class SpecializationService : ISpecializationService
    {
        private readonly ApplicationDbContext _context;

        public SpecializationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SpecializationResponseDto>> GetAllSpecializationsAsync()
        {
            return await _context.Specializations
                .Select(s => new SpecializationResponseDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    DoctorCount = s.Doctors.Count
                })
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<SpecializationResponseDto?> GetSpecializationByIdAsync(int id)
        {
            var specialization = await _context.Specializations
                .Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialization == null)
            {
                return null;
            }

            return new SpecializationResponseDto
            {
                Id = specialization.Id,
                Name = specialization.Name,
                Description = specialization.Description,
                DoctorCount = specialization.Doctors.Count
            };
        }

        public async Task<SpecializationResponseDto> CreateSpecializationAsync(CreateSpecializationDto dto)
        {
            var trimmedName = dto.Name.Trim();
            var exists = await _context.Specializations.AnyAsync(s => s.Name.ToLower() == trimmedName.ToLower());
            if (exists)
            {
                throw new InvalidOperationException($"Specialization '{trimmedName}' already exists.");
            }

            var specialization = new Specialization
            {
                Name = trimmedName,
                Description = dto.Description?.Trim()
            };

            await _context.Specializations.AddAsync(specialization);
            await _context.SaveChangesAsync();

            return new SpecializationResponseDto
            {
                Id = specialization.Id,
                Name = specialization.Name,
                Description = specialization.Description,
                DoctorCount = 0
            };
        }

        public async Task<SpecializationResponseDto> UpdateSpecializationAsync(int id, UpdateSpecializationDto dto)
        {
            var specialization = await _context.Specializations
                .Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialization == null)
            {
                throw new KeyNotFoundException($"Specialization with ID {id} not found.");
            }

            var trimmedName = dto.Name.Trim();
            var duplicateExists = await _context.Specializations
                .AnyAsync(s => s.Id != id && s.Name.ToLower() == trimmedName.ToLower());

            if (duplicateExists)
            {
                throw new InvalidOperationException($"Another specialization named '{trimmedName}' already exists.");
            }

            specialization.Name = trimmedName;
            specialization.Description = dto.Description?.Trim();

            await _context.SaveChangesAsync();

            return new SpecializationResponseDto
            {
                Id = specialization.Id,
                Name = specialization.Name,
                Description = specialization.Description,
                DoctorCount = specialization.Doctors.Count
            };
        }

        public async Task<bool> DeleteSpecializationAsync(int id)
        {
            var specialization = await _context.Specializations
                .Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (specialization == null)
            {
                return false;
            }

            if (specialization.Doctors.Any())
            {
                throw new InvalidOperationException($"Cannot delete specialization '{specialization.Name}' because it has active assigned doctors.");
            }

            _context.Specializations.Remove(specialization);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
