using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointment.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GetAll([FromQuery] string? search = null)
        {
            var patients = await _patientService.GetAllAsync(search);
            return Ok(patients);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            if (patient == null) return NotFound(new { message = "Patient not found." });
            return Ok(patient);
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var patient = await _patientService.GetByUserIdAsync(userId);
            if (patient == null) return NotFound(new { message = "Patient not found." });
            return Ok(patient);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Patient")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientDto dto)
        {
            var patient = await _patientService.UpdateAsync(id, dto);
            if (patient == null) return NotFound(new { message = "Patient not found." });
            return Ok(patient);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _patientService.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Patient not found." });
            return NoContent();
        }
    }
}
