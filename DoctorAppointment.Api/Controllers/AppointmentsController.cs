using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Enums;
using DoctorAppointment.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoctorAppointment.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? doctorId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] AppointmentStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var appointments = await _appointmentService.GetAllAsync(doctorId, patientId, status, fromDate, toDate);
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null) return NotFound(new { message = "Appointment not found." });
            return Ok(appointment);
        }

        [HttpPost("book")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Book([FromBody] BookAppointmentDto dto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized();

            var appointment = await _appointmentService.BookAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDto dto)
        {
            var appointment = await _appointmentService.UpdateAsync(id, dto);
            if (appointment == null) return NotFound(new { message = "Appointment not found." });
            return Ok(appointment);
        }

        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> Cancel(int id)
        {
            var result = await _appointmentService.CancelAsync(id);
            if (!result) return NotFound(new { message = "Appointment not found." });
            return Ok(new { message = "Appointment cancelled successfully." });
        }
    }
}
