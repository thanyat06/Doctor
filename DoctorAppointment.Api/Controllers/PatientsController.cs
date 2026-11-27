using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoctorAppointment.Backend.DTOs;
using DoctorAppointment.Backend.Enums;
using DoctorAppointment.Backend.Interfaces;

namespace DoctorAppointment.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [Authorize(Roles = "Admin,Doctor")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PatientResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _patientService.GetAllPatientsAsync();
            return Ok(patients);
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PatientResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("UserId")?.Value;

            if (role == UserRole.Patient.ToString() && int.TryParse(userIdStr, out var userId))
            {
                var currentPatient = await _patientService.GetPatientByUserIdAsync(userId);
                if (currentPatient == null || currentPatient.Id != id)
                {
                    return Forbid();
                }
            }

            var patient = await _patientService.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound(new { message = $"Patient with ID {id} not found." });
            }

            return Ok(patient);
        }

        [Authorize(Roles = "Patient,Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(PatientResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("UserId")?.Value;

            if (role == UserRole.Patient.ToString() && int.TryParse(userIdStr, out var userId))
            {
                var currentPatient = await _patientService.GetPatientByUserIdAsync(userId);
                if (currentPatient == null || currentPatient.Id != id)
                {
                    return Forbid();
                }
            }

            var updated = await _patientService.UpdatePatientAsync(id, dto);
            return Ok(updated);
        }
    }
}
