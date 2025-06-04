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
        private readonly ILogger<DealsController> _logger;

        public DealsController(IDealRepository dealRepository, ILogger<DealsController> logger)
        {
            _dealRepository = dealRepository;
            _logger = logger;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DealReadDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DealReadDto>> GetDealById(long id)
        {
            _logger.LogInformation($"Fetching deal with ID {id}");
            var dealDto = await _dealRepository.GetDealByIdAsync(id);
            if (dealDto == null)
            {
                _logger.LogWarning($"Deal with ID {id} not found.");
                return NotFound($"Deal with ID {id} not found.");
            }
            return Ok(dealDto);
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DealReadDto>), 200)]
        public async Task<ActionResult<IEnumerable<DealReadDto>>> GetAllDeals()
        {
            _logger.LogInformation("Fetching all deals");
            var deals = await _dealRepository.GetAllDealsAsync();
            return Ok(deals);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DealReadDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DealReadDto>> CreateDeal([FromBody] DealCreateDto dto)
        {
            _logger.LogInformation("Attempting to create a new deal with payload: {@DealCreateDto}", dto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for DealCreateDto: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var createdDealDto = await _dealRepository.AddDealAsync(dto);
                if (createdDealDto == null)
                {
                    _logger.LogError("Failed to create deal for an unknown reason.");
                    return Problem("Failed to create deal for an unknown reason.", statusCode: 500);
                }
                _logger.LogInformation($"Deal created successfully with ID {createdDealDto.Id}");
                return CreatedAtAction(nameof(GetDealById), new { id = createdDealDto.Id }, createdDealDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while creating deal: {Message}", ex.Message);
                return Problem(ex.Message, statusCode: 400);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating deal: {Message}", ex.Message);
                return Problem($"An unexpected error occurred: {ex.Message}", statusCode: 500);
            }
        }
    }
}