using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointment.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int? specializationId = null,
            [FromQuery] bool? isAvailable = null)
        {
            var doctors = await _doctorService.GetAllAsync(search, specializationId, isAvailable);
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null) return NotFound(new { message = "Doctor not found." });
            return Ok(doctor);
        }

        [HttpGet("by-user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var doctor = await _doctorService.GetByUserIdAsync(userId);
            if (doctor == null) return NotFound(new { message = "Doctor not found." });
            return Ok(doctor);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDoctorDto dto)
        {
            var doctor = await _doctorService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = doctor.Id }, doctor);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorDto dto)
        {
            var doctor = await _doctorService.UpdateAsync(id, dto);
            if (doctor == null) return NotFound(new { message = "Doctor not found." });
            return Ok(doctor);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _doctorService.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Doctor not found." });
            return NoContent();
        }
    }
}
