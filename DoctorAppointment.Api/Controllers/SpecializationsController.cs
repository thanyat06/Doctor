using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DoctorAppointment.Backend.DTOs;
using DoctorAppointment.Backend.Interfaces;

namespace DoctorAppointment.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecializationsController : ControllerBase
    {
        private readonly ISpecializationService _specializationService;

        public SpecializationsController(ISpecializationService specializationService)
        {
            _specializationService = specializationService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<SpecializationResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var specializations = await _specializationService.GetAllSpecializationsAsync();
            return Ok(specializations);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SpecializationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var specialization = await _specializationService.GetSpecializationByIdAsync(id);
            if (specialization == null)
            {
                return NotFound(new { message = $"Specialization with ID {id} not found." });
            }

            return Ok(specialization);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ProducesResponseType(typeof(SpecializationResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create([FromBody] CreateSpecializationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = await _specializationService.CreateSpecializationAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(SpecializationResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSpecializationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updated = await _specializationService.UpdateSpecializationAsync(id, dto);
            return Ok(updated);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _specializationService.DeleteSpecializationAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Specialization with ID {id} not found." });
            }

            return NoContent();
        }
    }
}
