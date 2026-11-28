using DoctorAppointment.Api.Data;
using DoctorAppointment.Api.Enums;
using DoctorAppointment.Api.Models;
using DoctorAppointment.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointment.Api.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly ApplicationDbContext _context;

        public AppointmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync(
            int? doctorId = null,
            int? patientId = null,
            AppointmentStatus? status = null,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var query = _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .AsQueryable();

            if (doctorId.HasValue)
                query = query.Where(a => a.DoctorId == doctorId.Value);

            if (patientId.HasValue)
                query = query.Where(a => a.PatientId == patientId.Value);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= toDate.Value);

            return await query.OrderByDescending(a => a.AppointmentDate).ThenBy(a => a.TimeSlot).ToListAsync();
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.User)
                .Include(a => a.Doctor).ThenInclude(d => d.Specialization)
                .Include(a => a.Patient).ThenInclude(p => p.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Appointment> CreateAsync(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<Appointment> UpdateAsync(Appointment appointment)
        {
            appointment.UpdatedAt = DateTime.UtcNow;
            _context.Appointments.Update(appointment);
            await _context.SaveChangesAsync();
            return appointment;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return false;
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Appointments.AnyAsync(a => a.Id == id);
        }

        public async Task<bool> IsSlotTakenAsync(int doctorId, DateTime date, TimeOnly timeSlot, int? excludeId = null)
        {
            var query = _context.Appointments
                .Where(a => a.DoctorId == doctorId
                    && a.AppointmentDate.Date == date.Date
                    && a.TimeSlot == timeSlot
                    && a.Status != AppointmentStatus.Cancelled);

            if (excludeId.HasValue)
                query = query.Where(a => a.Id != excludeId.Value);

            return await query.AnyAsync();
        }
    }
}
