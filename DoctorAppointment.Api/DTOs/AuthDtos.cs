namespace DoctorAppointment.Api.DTOs
{
    // Auth DTOs
    public record RegisterDto(
        string Name,
        string Email,
        string Password,
        string Role, // "Doctor" or "Patient"
        string? PhoneNumber,
        int? SpecializationId,
        string? LicenseNumber,
        int? ExperienceYears,
        decimal? ConsultationFee,
        string? Bio
    );

    public record LoginDto(string Email, string Password);

    public record AuthResponseDto(
        string Token,
        string Name,
        string Email,
        string Role,
        int UserId,
        int? ProfileId
    );
}
