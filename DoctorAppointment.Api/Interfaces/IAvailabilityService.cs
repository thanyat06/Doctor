
using DoctorAppointment.Api.DTOs;

namespace DoctorAppointment.Api.Interfaces
{
    public interface IAvailabilityService
    {
        Task<IEnumerable<AvailabilityDto>> GetByDoctorIdAsync(int doctorId);
        Task<AvailabilityDto?> GetByIdAsync(int id);
        Task<AvailabilityDto> CreateAsync(CreateAvailabilityDto dto);
        Task<AvailabilityDto?> UpdateAsync(int id, UpdateAvailabilityDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<string>> GetAvailableSlotsAsync(int doctorId, DateTime date);
    }
}
