using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Interfaces;
using DoctorAppointment.Api.Models;
using DoctorAppointment.Api.Repositories.Interfaces;

namespace DoctorAppointment.Api.Services
{
    public class SpecializationService : ISpecializationService
    {
        private readonly ISpecializationRepository _specializationRepository;

        public SpecializationService(ISpecializationRepository specializationRepository)
        {
            _specializationRepository = specializationRepository;
        }

        public async Task<IEnumerable<SpecializationDto>> GetAllAsync()
        {
            var specs = await _specializationRepository.GetAllAsync();
            return specs.Select(MapToDto);
        }

        public async Task<SpecializationDto?> GetByIdAsync(int id)
        {
            var spec = await _specializationRepository.GetByIdAsync(id);
            return spec == null ? null : MapToDto(spec);
        }

        public async Task<SpecializationDto> CreateAsync(CreateSpecializationDto dto)
        {
            if (await _specializationRepository.NameExistsAsync(dto.Name))
                throw new InvalidOperationException("Specialization name already exists.");

            var spec = new Specialization
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = true
            };
            var created = await _specializationRepository.CreateAsync(spec);
            var full = await _specializationRepository.GetByIdAsync(created.Id);
            return MapToDto(full!);
        }

        public async Task<SpecializationDto?> UpdateAsync(int id, UpdateSpecializationDto dto)
        {
            var spec = await _specializationRepository.GetByIdAsync(id);
            if (spec == null) return null;

            if (dto.Name != null) spec.Name = dto.Name;
            if (dto.Description != null) spec.Description = dto.Description;
            if (dto.IsActive.HasValue) spec.IsActive = dto.IsActive.Value;

            await _specializationRepository.UpdateAsync(spec);
            var updated = await _specializationRepository.GetByIdAsync(id);
            return MapToDto(updated!);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _specializationRepository.DeleteAsync(id);
        }

        private static SpecializationDto MapToDto(Specialization s) => new SpecializationDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            IsActive = s.IsActive,
            DoctorCount = s.Doctors?.Count ?? 0
        };
    }
}
