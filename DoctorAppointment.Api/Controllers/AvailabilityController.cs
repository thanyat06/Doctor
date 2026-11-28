using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointment.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityService availabilityService)
        {
            _availabilityService = availabilityService;
        }

        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctor(int doctorId)
        {
            var avails = await _availabilityService.GetByDoctorIdAsync(doctorId);
            return Ok(avails);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var avail = await _availabilityService.GetByIdAsync(id);
            if (avail == null) return NotFound(new { message = "Availability not found." });
            return Ok(avail);
        }

        [HttpGet("slots")]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] int doctorId, [FromQuery] DateTime date)
        {
            var slots = await _availabilityService.GetAvailableSlotsAsync(doctorId, date);
            return Ok(slots);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Create([FromBody] CreateAvailabilityDto dto)
        {
            var avail = await _availabilityService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = avail.Id }, avail);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAvailabilityDto dto)
        {
            var avail = await _availabilityService.UpdateAsync(id, dto);
            if (avail == null) return NotFound(new { message = "Availability not found." });
            return Ok(avail);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _availabilityService.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Availability not found." });
            return NoContent();
        }
    }
}
