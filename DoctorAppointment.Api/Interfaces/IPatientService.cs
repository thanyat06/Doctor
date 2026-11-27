using DoctorAppointment.Backend.DTOs;

namespace DoctorAppointment.Backend.Interfaces
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientResponseDto>> GetAllPatientsAsync();
        Task<PatientResponseDto?> GetPatientByIdAsync(int id);
        Task<PatientResponseDto?> GetPatientByUserIdAsync(int userId);
        Task<PatientResponseDto> UpdatePatientAsync(int id, UpdatePatientDto dto);
    }
}
