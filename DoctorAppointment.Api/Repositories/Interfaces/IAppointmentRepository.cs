using DoctorAppointment.Api.Enums;
using DoctorAppointment.Api.Models;

namespace DoctorAppointment.Api.Repositories.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAsync(int? doctorId = null, int? patientId = null, AppointmentStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<Appointment?> GetByIdAsync(int id);
        Task<Appointment> CreateAsync(Appointment appointment);
        Task<Appointment> UpdateAsync(Appointment appointment);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsSlotTakenAsync(int doctorId, DateTime date, TimeOnly timeSlot, int? excludeId = null);
    }
}
