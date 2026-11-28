using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Enums;

namespace DoctorAppointment.Api.Interfaces
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentDto>> GetAllAsync(int? doctorId = null, int? patientId = null, AppointmentStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<AppointmentDto?> GetByIdAsync(int id);
        Task<AppointmentDto> BookAsync(BookAppointmentDto dto, int patientUserId);
        Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentDto dto);
        Task<bool> CancelAsync(int id);
    }
}
