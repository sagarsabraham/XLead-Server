using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace XLead_Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DealsController : ControllerBase
    {
        private readonly IDealRepository _dealRepository;
        private readonly ILogger<DealsController> _logger;
        private readonly IUserPrivilegeRepository _userPrivilegeRepository;

        public DealsController(
            IDealRepository dealRepository,
            ILogger<DealsController> logger,
            IUserPrivilegeRepository userPrivilegeRepository)
        {
            _dealRepository = dealRepository;
            _logger = logger;
            _userPrivilegeRepository = userPrivilegeRepository;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DealReadDto), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<DealReadDto>> GetDealById(long id, [FromQuery] int userId)
        {
            // Check privileges
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(userId);
            if (!privileges.Any(p => p.PrivilegeName == "ViewDeals" || p.PrivilegeName == "PipelineDetailAccess"))
            {
                _logger.LogWarning($"User {userId} lacks ViewDeals privilege");
                return Forbid("User lacks ViewDeals privilege");
            }

            _logger.LogInformation($"User {userId} fetching deal with ID {id}");
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
        [ProducesResponseType(403)]
        public async Task<ActionResult<IEnumerable<DealReadDto>>> GetAllDeals([FromQuery] int userId)
        {
            // Check privileges
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(userId);
            if (!privileges.Any(p => p.PrivilegeName == "ViewDeals" || p.PrivilegeName == "PipelineDetailAccess"))
            {
                _logger.LogWarning($"User {userId} lacks ViewDeals privilege");
                return Forbid("User lacks ViewDeals privilege");
            }

            _logger.LogInformation($"User {userId} fetching all deals");
            var deals = await _dealRepository.GetAllDealsAsync();
            return Ok(deals);
        }

        // In DealsController.cs - UpdateDealStage method
        [HttpPut("{id}/stage")]
        [ProducesResponseType(typeof(DealReadDto), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateDealStage(long id, [FromBody] DealUpdateDTO dto)
        {
            // Handle null UpdatedBy
            if (!dto.UpdatedBy.HasValue)
            {
                return BadRequest("UpdatedBy is required");
            }

            // Check privileges using UpdatedBy from DTO
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(dto.UpdatedBy.Value);
            if (!privileges.Any(p => p.PrivilegeName == "UpdateDealStage" || p.PrivilegeName == "PipelineDetailAccess"))
            {
                _logger.LogWarning($"User {dto.UpdatedBy.Value} lacks UpdateDealStage privilege");
                return Forbid("User lacks UpdateDealStage privilege");
            }

            _logger.LogInformation($"User {dto.UpdatedBy.Value} updating stage for deal {id}");
            var updatedDeal = await _dealRepository.UpdateDealStageAsync(id, dto);
            if (updatedDeal == null)
            {
                return NotFound($"Deal with ID {id} not found.");
            }

            return Ok(updatedDeal);
        }
        [HttpPost]
        [ProducesResponseType(typeof(DealReadDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DealReadDto>> CreateDeal([FromBody] DealCreateDto dto)
        {
            // Check privileges
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(dto.CreatedBy);
            if (!privileges.Any(p => p.PrivilegeName == "CreateDeal" || p.PrivilegeName == "PipelineDetailAccess"))
            {
                _logger.LogWarning($"User {dto.CreatedBy} lacks CreateDeal privilege");
                return Forbid("User lacks CreateDeal privilege");
            }

            _logger.LogInformation("User {UserId} attempting to create a new deal with payload: {@DealCreateDto}", dto.CreatedBy, dto);

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
                _logger.LogInformation($"Deal created successfully with ID {createdDealDto.Id} by user {dto.CreatedBy}");
                return CreatedAtAction(nameof(GetDealById), new { id = createdDealDto.Id }, createdDealDto);
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update exception while creating deal");
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                _logger.LogError($"Inner exception details: {innerMessage}");

                // Return more detailed error for debugging (remove in production)
                return Problem($"Database error: {innerMessage}", statusCode: 500);
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

        [HttpGet("{id}/stage-history")]
        [ProducesResponseType(typeof(IEnumerable<StageHistoryDto>), 200)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<StageHistoryDto>>> GetDealStageHistory(long id, [FromQuery] int userId)
        {
            // Check privileges
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(userId);
            if (!privileges.Any(p => p.PrivilegeName == "ViewDealHistory" || p.PrivilegeName == "PipelineDetailAccess"))
            {
                _logger.LogWarning($"User {userId} lacks ViewDealHistory privilege");
                return Forbid("User lacks ViewDealHistory privilege");
            }

            _logger.LogInformation($"User {userId} fetching stage history for deal {id}");
            var history = await _dealRepository.GetDealStageHistoryAsync(id);
            if (history == null || !history.Any())
            {
                return NotFound($"No stage history found for deal with ID {id}.");
            }
            return Ok(history);
        }
    }
}