using DoctorAppointment.Backend.Models;

namespace DoctorAppointment.Backend.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, int? doctorId = null, int? patientId = null);
    }
}
