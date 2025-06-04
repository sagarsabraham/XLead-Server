
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;

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
        [ProducesResponseType(typeof(DealReadDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DealReadDto>> GetDealByIdAsync(long id)
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
        [ProducesResponseType(typeof(IEnumerable<DealReadDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DealReadDto>>> GetAllDealsAsync()
        {
            _logger.LogInformation("Fetching all deals");
            var deals = await _dealRepository.GetAllDealsAsync();
            return Ok(deals);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DealReadDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DealReadDto>> CreateDealAsync([FromBody] DealCreateDto dealCreateDto)
        {
            _logger.LogInformation("Attempting to create a new deal with payload: {@DealCreateDto}", dealCreateDto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for DealCreateDto: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var createdDealDto = await _dealRepository.AddDealAsync(dealCreateDto);
                if (createdDealDto == null)
                {
                    _logger.LogError("Failed to create deal, repository returned null unexpectedly.");
                    return Problem(
                        detail: "Failed to create deal. The operation did not result in a new deal record.",
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Creation Failed");
                }
                _logger.LogInformation($"Deal created successfully with ID {createdDealDto.Id}");
                return CreatedAtAction(nameof(GetDealByIdAsync), new { id = createdDealDto.Id }, createdDealDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while creating deal: {Message}", ex.Message);
                return Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid Operation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating deal: {Message}", ex.Message);
                return Problem(
                    detail: "An unexpected error occurred while creating the deal. Please try again later.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Server Error");
            }
        }

        [HttpGet("dashboard-metrics")] 
        [ProducesResponseType(typeof(DashboardMetricsDto), 200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DashboardMetricsDto>> GetDashboardMetrics()
        {
            _logger.LogInformation("Fetching dashboard metrics");
            try
            {
                var metrics = await _dealRepository.GetDashboardMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard metrics: {Message}", ex.Message);
                return Problem("An unexpected error occurred while fetching dashboard metrics.", statusCode: 500);
            }
        }
    }
}
