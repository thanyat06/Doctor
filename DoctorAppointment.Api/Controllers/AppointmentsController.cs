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
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientService _patientService;
        private readonly IDoctorService _doctorService;

        public AppointmentsController(
            IAppointmentService appointmentService,
            IPatientService patientService,
            IDoctorService doctorService)
        {
            _appointmentService = appointmentService;
            _patientService = patientService;
            _doctorService = doctorService;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] AppointmentFilterDto filter)
        {
            var appointments = await _appointmentService.GetAllAppointmentsAsync(filter);
            return Ok(appointments);
        }

        [Authorize]
        [HttpGet("my")]
        [ProducesResponseType(typeof(IEnumerable<AppointmentResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyAppointments()
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            if (role == UserRole.Patient.ToString())
            {
                var patient = await _patientService.GetPatientByUserIdAsync(userId);
                if (patient == null)
                {
                    return NotFound(new { message = "Patient profile not found." });
                }
                var appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(patient.Id);
                return Ok(appointments);
            }
            else if (role == UserRole.Doctor.ToString())
            {
                var doctor = await _doctorService.GetDoctorByUserIdAsync(userId);
                if (doctor == null)
                {
                    return NotFound(new { message = "Doctor profile not found." });
                }
                var appointments = await _appointmentService.GetAppointmentsByDoctorIdAsync(doctor.Id);
                return Ok(appointments);
            }
            else if (role == UserRole.Admin.ToString())
            {
                var appointments = await _appointmentService.GetAllAppointmentsAsync();
                return Ok(appointments);
            }

            return Forbid();
        }

        [Authorize]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound(new { message = $"Appointment with ID {id} not found." });
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("UserId")?.Value;

            if (role == UserRole.Patient.ToString() && int.TryParse(userIdStr, out var userId))
            {
                var patient = await _patientService.GetPatientByUserIdAsync(userId);
                if (patient == null || patient.Id != appointment.PatientId)
                {
                    return Forbid();
                }
            }
            else if (role == UserRole.Doctor.ToString() && int.TryParse(userIdStr, out var doctorUserId))
            {
                var doctor = await _doctorService.GetDoctorByUserIdAsync(doctorUserId);
                if (doctor == null || doctor.Id != appointment.DoctorId)
                {
                    return Forbid();
                }
            }

            return Ok(appointment);
        }

        [Authorize(Roles = "Patient")]
        [HttpPost("book")]
        [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Book([FromBody] BookAppointmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var created = await _appointmentService.BookAppointmentAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize]
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var updated = await _appointmentService.UpdateStatusAsync(id, dto, userId, role);
            return Ok(updated);
        }

        [Authorize(Roles = "Doctor")]
        [HttpPut("{id}/notes")]
        [ProducesResponseType(typeof(AppointmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> AddNotes(int id, [FromBody] DoctorAddNotesDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var updated = await _appointmentService.AddDoctorNotesAsync(id, dto, userId);
            return Ok(updated);
        }

        [Authorize]
        [HttpPut("{id}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Cancel(int id, [FromBody] CancelAppointmentDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("UserId")?.Value;

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var success = await _appointmentService.CancelAppointmentAsync(id, dto.CancellationReason, userId, role);
            if (!success)
            {
                return NotFound(new { message = $"Appointment with ID {id} not found." });
            }

            return Ok(new { message = "Appointment cancelled successfully." });
        }
    }
}
