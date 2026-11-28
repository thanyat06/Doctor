using DoctorAppointment.Api.Models;

namespace DoctorAppointment.Api.Repositories.Interfaces
{
    public interface IAvailabilityRepository
    {
        Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(int doctorId);
        Task<DoctorAvailability?> GetByIdAsync(int id);
        Task<DoctorAvailability> CreateAsync(DoctorAvailability availability);
        Task<DoctorAvailability> UpdateAsync(DoctorAvailability availability);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<DoctorAvailability?> GetByDoctorAndDayAsync(int doctorId, DayOfWeek dayOfWeek);
    }
}
