using DoctorAppointment.Api.Data;
using DoctorAppointment.Api.Models;
using DoctorAppointment.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DoctorAppointment.Api.Repositories
{
    public class SpecializationRepository : ISpecializationRepository
    {
        private readonly ApplicationDbContext _context;

        public SpecializationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Specialization>> GetAllAsync()
        {
            return await _context.Specializations
                .Include(s => s.Doctors)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Specialization?> GetByIdAsync(int id)
        {
            return await _context.Specializations
                .Include(s => s.Doctors).ThenInclude(d => d.User)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Specialization> CreateAsync(Specialization specialization)
        {
            _context.Specializations.Add(specialization);
            await _context.SaveChangesAsync();
            return specialization;
        }

        public async Task<Specialization> UpdateAsync(Specialization specialization)
        {
            _context.Specializations.Update(specialization);
            await _context.SaveChangesAsync();
            return specialization;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var spec = await _context.Specializations.FindAsync(id);
            if (spec == null) return false;
            _context.Specializations.Remove(spec);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Specializations.AnyAsync(s => s.Id == id);
        }

        public async Task<bool> NameExistsAsync(string name)
        {
            return await _context.Specializations.AnyAsync(s => s.Name.ToLower() == name.ToLower());
        }
    }
}
