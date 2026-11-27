using DoctorAppointment.Api.Models;

namespace DoctorAppointment.Api.Repositories.Interfaces
{
    public interface ISpecializationRepository
    {
        Task<IEnumerable<Specialization>> GetAllAsync();
        Task<Specialization?> GetByIdAsync(int id);
        Task<Specialization> CreateAsync(Specialization specialization);
        Task<Specialization> UpdateAsync(Specialization specialization);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> NameExistsAsync(string name);
    }
}
