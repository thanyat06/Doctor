using DoctorAppointment.Api.Data;
using DoctorAppointment.Api.Models;
using DoctorAppointment.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointment.Api.Repositories
{
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly ApplicationDbContext _context;

        public AvailabilityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DoctorAvailability>> GetByDoctorIdAsync(int doctorId)
        {
            return await _context.DoctorAvailabilities
                .Include(da => da.Doctor).ThenInclude(d => d.User)
                .Where(da => da.DoctorId == doctorId)
                .OrderBy(da => da.DayOfWeek)
                .ToListAsync();
        }

        public async Task<DoctorAvailability?> GetByIdAsync(int id)
        {
            return await _context.DoctorAvailabilities
                .Include(da => da.Doctor).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(da => da.Id == id);
        }

        public async Task<DoctorAvailability> CreateAsync(DoctorAvailability availability)
        {
            _context.DoctorAvailabilities.Add(availability);
            await _context.SaveChangesAsync();
            return availability;
        }

        public async Task<DoctorAvailability> UpdateAsync(DoctorAvailability availability)
        {
            _context.DoctorAvailabilities.Update(availability);
            await _context.SaveChangesAsync();
            return availability;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var availability = await _context.DoctorAvailabilities.FindAsync(id);
            if (availability == null) return false;
            _context.DoctorAvailabilities.Remove(availability);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.DoctorAvailabilities.AnyAsync(da => da.Id == id);
        }

        public async Task<DoctorAvailability?> GetByDoctorAndDayAsync(int doctorId, DayOfWeek dayOfWeek)
        {
            return await _context.DoctorAvailabilities
                .FirstOrDefaultAsync(da => da.DoctorId == doctorId && da.DayOfWeek == dayOfWeek);
        }
    }
}
