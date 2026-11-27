using DoctorAppointment.Api.DTOs;
using DoctorAppointment.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorAppointment.Api.Controllers
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
        public async Task<IActionResult> GetAll()
        {
            var specs = await _specializationService.GetAllAsync();
            return Ok(specs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var spec = await _specializationService.GetByIdAsync(id);
            if (spec == null) return NotFound(new { message = "Specialization not found." });
            return Ok(spec);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateSpecializationDto dto)
        {
            var spec = await _specializationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = spec.Id }, spec);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSpecializationDto dto)
        {
            var spec = await _specializationService.UpdateAsync(id, dto);
            if (spec == null) return NotFound(new { message = "Specialization not found." });
            return Ok(spec);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _specializationService.DeleteAsync(id);
            if (!result) return NotFound(new { message = "Specialization not found." });
            return NoContent();
        }
    }
}
