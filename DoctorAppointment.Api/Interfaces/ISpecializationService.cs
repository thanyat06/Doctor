using DoctorAppointment.Api.DTOs;

namespace DoctorAppointment.Api.Interfaces
{
    public interface ISpecializationService
    {
        Task<IEnumerable<SpecializationDto>> GetAllAsync();
        Task<SpecializationDto?> GetByIdAsync(int id);
        Task<SpecializationDto> CreateAsync(CreateSpecializationDto dto);
        Task<SpecializationDto?> UpdateAsync(int id, UpdateSpecializationDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
