using DoctorAppointment.Api.Models;

namespace DoctorAppointment.Api.Repositories.Interfaces
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetAllAsync(string? search = null);
        Task<Patient?> GetByIdAsync(int id);
        Task<Patient?> GetByUserIdAsync(int userId);
        Task<Patient> CreateAsync(Patient patient);
        Task<Patient> UpdateAsync(Patient patient);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
