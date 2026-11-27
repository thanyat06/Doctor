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
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availabilityService;
        private readonly IDoctorService _doctorService;

        public AvailabilityController(IAvailabilityService availabilityService, IDoctorService doctorService)
        {
            _availabilityService = availabilityService;
            _doctorService = doctorService;
        }

        [HttpGet("doctor/{doctorId}")]
        [ProducesResponseType(typeof(IEnumerable<AvailabilityResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByDoctorId(int doctorId)
        {
            var availabilities = await _availabilityService.GetAvailabilitiesByDoctorIdAsync(doctorId);
            return Ok(availabilities);
        }

        [HttpGet("doctor/{doctorId}/slots")]
        [ProducesResponseType(typeof(DoctorSlotsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateTime? date)
        {
            var targetDate = date ?? DateTime.UtcNow.Date;
            var slots = await _availabilityService.GetAvailableSlotsAsync(doctorId, targetDate);
            return Ok(slots);
        }

        [Authorize(Roles = "Doctor,Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(AvailabilityResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> SetAvailability([FromBody] CreateAvailabilityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst("UserId")?.Value;

            if (role == UserRole.Doctor.ToString() && int.TryParse(userIdStr, out var userId))
            {
                var currentDoctor = await _doctorService.GetDoctorByUserIdAsync(userId);
                if (currentDoctor == null)
                {
                    return Forbid();
                }
                dto.DoctorId = currentDoctor.Id;
            }

            if (!dto.DoctorId.HasValue)
            {
                return BadRequest(new { message = "DoctorId must be provided." });
            }

            var created = await _availabilityService.SetAvailabilityAsync(dto);
            return CreatedAtAction(nameof(GetByDoctorId), new { doctorId = created.DoctorId }, created);
        }

        [Authorize(Roles = "Doctor,Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(AvailabilityResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateAvailability(int id, [FromBody] UpdateAvailabilityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _availabilityService.UpdateAvailabilityAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Roles = "Doctor,Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            var deleted = await _availabilityService.DeleteAvailabilityAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Availability record with ID {id} not found." });
            }

            return NoContent();
        }
    }
}
