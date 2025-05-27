// XLead_Server/Controllers/DealsController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using System.Collections.Generic; // For IEnumerable

namespace XLead_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DealsController : ControllerBase
    {
        private readonly IDealRepository _dealRepository;

        public DealsController(IDealRepository dealRepository)
        {
            _dealRepository = dealRepository;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DealReadDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DealReadDto>> GetDealById(long id)
        {
            var dealDto = await _dealRepository.GetDealByIdAsync(id);
            if (dealDto == null)
            {
                return NotFound($"Deal with ID {id} not found.");
            }
            return Ok(dealDto);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DealReadDto>), 200)]
        public async Task<ActionResult<IEnumerable<DealReadDto>>> GetAllDeals()
        {
            var deals = await _dealRepository.GetAllDealsAsync();
            return Ok(deals);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DealReadDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DealReadDto>> CreateDeal([FromBody] DealCreateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdDealDto = await _dealRepository.AddDealAsync(dto);
                if (createdDealDto == null)
                {
                    // This case should ideally be caught by exceptions in the repository
                    return Problem("Failed to create deal for an unknown reason.", statusCode: 500);
                }
                // Use nameof(GetDealById) for the location header
                return CreatedAtAction(nameof(GetDealById), new { id = createdDealDto.Id }, createdDealDto);
            }
            catch (InvalidOperationException ex) // Catch specific exceptions from repository
            {
                // Log ex
                return Problem(ex.Message, statusCode: 400); // Or 500 if it's truly a server issue
            }
            catch (Exception ex) // Catch-all for unexpected errors
            {
                // Log ex (important for debugging)
                // Consider a more generic error message for the client
                return Problem($"An unexpected error occurred: {ex.Message}", statusCode: 500);
            }
        }
    }
}