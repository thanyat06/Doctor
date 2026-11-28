using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using DoctorAppointment.Api.Interfaces;
using DoctorAppointment.Api.Models;

namespace DoctorAppointment.Api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user, int? doctorId = null, int? patientId = null)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "DoctorAppointmentSecretKeyWithSufficientLengthForHS256Security123456";
            var issuer = _configuration["Jwt:Issuer"] ?? "DoctorAppointmentAPI";
            var audience = _configuration["Jwt:Audience"] ?? "DoctorAppointmentClients";
            var durationMinutes = int.TryParse(_configuration["Jwt:DurationInMinutes"], out var parsedDuration) ? parsedDuration : 1440;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("UserId", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (doctorId.HasValue)
            {
                claims.Add(new Claim("DoctorId", doctorId.Value.ToString()));
            }

            if (patientId.HasValue)
            {
                claims.Add(new Claim("PatientId", patientId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(durationMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
