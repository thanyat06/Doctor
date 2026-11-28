using DoctorAppointment.Api.DTOs;

namespace DoctorAppointment.Api.Interfaces
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientDto>> GetAllAsync(string? search = null);
        Task<PatientDto?> GetByIdAsync(int id);
        Task<PatientDto?> GetByUserIdAsync(int userId);
        Task<PatientDto?> UpdateAsync(int id, UpdatePatientDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
