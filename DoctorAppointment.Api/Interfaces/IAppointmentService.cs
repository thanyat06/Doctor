using DoctorAppointment.Backend.DTOs;

namespace DoctorAppointment.Backend.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentResponseDto>> GetAllAppointmentsAsync(AppointmentFilterDto? filter = null);
        Task<AppointmentResponseDto?> GetAppointmentByIdAsync(int id);
        Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByPatientIdAsync(int patientId);
        Task<IEnumerable<AppointmentResponseDto>> GetAppointmentsByDoctorIdAsync(int doctorId);
        Task<AppointmentResponseDto> BookAppointmentAsync(int patientUserId, BookAppointmentDto dto);
        Task<AppointmentResponseDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto, int requestingUserId, string userRole);
        Task<AppointmentResponseDto> AddDoctorNotesAsync(int id, DoctorAddNotesDto dto, int doctorUserId);
        Task<bool> CancelAppointmentAsync(int id, string reason, int requestingUserId, string userRole);
    }
}