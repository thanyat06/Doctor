using DoctorAppointment.Api.DTOs;

namespace DoctorAppointment.Api.Interfaces
{
    public interface IDoctorService
    {
        Task<IEnumerable<DoctorDto>> GetAllAsync(string? search = null, int? specializationId = null, bool? isAvailable = null);
        Task<DoctorDto?> GetByIdAsync(int id);
        Task<DoctorDto?> GetByUserIdAsync(int userId);
        Task<DoctorDto> CreateAsync(CreateDoctorDto dto);
        Task<DoctorDto?> UpdateAsync(int id, UpdateDoctorDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
