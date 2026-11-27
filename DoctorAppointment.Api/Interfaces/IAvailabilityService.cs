using DoctorAppointment.Backend.DTOs;

namespace DoctorAppointment.Backend.Interfaces
{
    public interface IAvailabilityService
    {
        Task<IEnumerable<AvailabilityResponseDto>> GetAvailabilitiesByDoctorIdAsync(int doctorId);
        Task<AvailabilityResponseDto> SetAvailabilityAsync(CreateAvailabilityDto dto);
        Task<AvailabilityResponseDto> UpdateAvailabilityAsync(int id, UpdateAvailabilityDto dto);
        Task<bool> DeleteAvailabilityAsync(int id);
        Task<DoctorSlotsResponseDto> GetAvailableSlotsAsync(int doctorId, DateTime date);
    }
}
