using DoctorAppointment.Backend.DTOs;

namespace DoctorAppointment.Backend.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<UserDto> GetCurrentUserAsync(int userId);
    }
}
