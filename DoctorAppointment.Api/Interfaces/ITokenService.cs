using DoctorAppointment.Api.Models;

namespace DoctorAppointment.Api.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user, int? doctorId = null, int? patientId = null);
    }
}