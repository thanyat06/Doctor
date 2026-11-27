using DoctorAppointment.Backend.DTOs;

namespace DoctorAppointment.Backend.Interfaces
{
    public interface ISpecializationService
    {
        Task<IEnumerable<SpecializationResponseDto>> GetAllSpecializationsAsync();
        Task<SpecializationResponseDto?> GetSpecializationByIdAsync(int id);
        Task<SpecializationResponseDto> CreateSpecializationAsync(CreateSpecializationDto dto);
        Task<SpecializationResponseDto> UpdateSpecializationAsync(int id, UpdateSpecializationDto dto);
        Task<bool> DeleteSpecializationAsync(int id);
    }
}
