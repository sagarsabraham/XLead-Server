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
        private readonly IUserPrivilegeRepository _userPrivilegeRepository;
        private readonly IStageHistoryRepository _stageHistoryRepository;

        public DealsController(IDealRepository dealRepository, ILogger<DealsController> logger, IUserPrivilegeRepository userPrivilegeRepository, IStageHistoryRepository stageHistoryRepository)
        {
            _dealRepository = dealRepository;
            _logger = logger;
            _userPrivilegeRepository = userPrivilegeRepository;
            _stageHistoryRepository = stageHistoryRepository;

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
        public async Task<ActionResult<IEnumerable<DealReadDto>>> GetAllDeals([FromQuery] long? createdByUserId)
        {
            if (createdByUserId.HasValue)
            {
                _logger.LogInformation("Fetching all deals for creator ID {CreatedByUserId}", createdByUserId.Value);

            }
            else
            {
                _logger.LogInformation("Fetching all deals (no creator filter)");

            }

            var deals = await _dealRepository.GetAllDealsAsync();
            return Ok(deals);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DealReadDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DealReadDto>> CreateDeal([FromBody] DealCreateDto dto)
        {


            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(dto.CreatedBy);
            if (privileges == null || !privileges.Any(p => p.PrivilegeName == "CreateDeal"))
            {
                _logger.LogWarning($"User {dto.CreatedBy} lacks 'CreateDeal' privilege.");

                return Forbid("User lacks the 'CreateDeal' privilege.");
            }
            _logger.LogInformation($"User {dto.CreatedBy} has 'CreateDeal' privilege. Proceeding with deal creation.");




            try
            {
                var createdDealDto = await _dealRepository.AddDealAsync(dto);

                if (createdDealDto == null)
                {
                    _logger.LogError("Failed to create deal for an unknown reason after repository call (repository returned null).");
                    return Problem("Failed to create deal. Repository indicated failure.", statusCode: 500);
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
                return Problem($"An unexpected error occurred while creating the deal: {ex.Message}", statusCode: 500);
            }
        }

        [HttpPut("{id}/stage")]
        [ProducesResponseType(typeof(DealReadDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> UpdateDealStage(long id, [FromBody] DealUpdateDTO dto)
        {
            _logger.LogInformation("Attempting to update stage for deal ID {DealId} with payload: {@DealUpdateDto}", id, dto);


            if (dto.PerformedByUserId <= 0)
            {
                _logger.LogWarning("PerformedByUserId is missing or invalid in DealUpdateDto.");
                return BadRequest("User identifier (PerformedByUserId) is required to check privileges and record history.");
            }

            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(dto.PerformedByUserId);
            if (privileges == null || !privileges.Any(p => p.PrivilegeName == "StageUpdate"))
            {
                _logger.LogWarning("User {UserId} lacks 'StageUpdate' privilege for deal ID {DealId}.", dto.PerformedByUserId, id);
                return Forbid("User lacks the 'StageUpdate' privilege.");
            }
            _logger.LogInformation("User {UserId} has 'StageUpdate' privilege for deal ID {DealId}. Proceeding.", dto.PerformedByUserId, id);


            try
            {

                var updatedDealDto = await _dealRepository.UpdateDealStageAsync(id, dto);

                if (updatedDealDto == null)
                {
                    _logger.LogWarning("Deal with ID {DealId} not found during stage update attempt.", id);
                    return NotFound($"Deal with ID {id} not found.");
                }

                _logger.LogInformation("Deal ID {DealId} stage updated successfully to {StageName}.", id, dto.StageName);


                try
                {
                    var stageHistoryCreateDto = new StageHistoryCreateDto
                    {
                        DealId = id,
                        StageName = dto.StageName,
                        CreatedBy = dto.PerformedByUserId
                    };

                    var createdStageHistory = await _stageHistoryRepository.CreateStageHistoryAsync(stageHistoryCreateDto);
                    _logger.LogInformation("Stage history created for Deal ID {DealId}, New Stage: {StageName}, History ID: {HistoryId}",
                        id, dto.StageName, createdStageHistory.Id);
                }
                catch (Exception ex_history)
                {

                    _logger.LogError(ex_history, "Failed to create stage history for Deal ID {DealId} after successful stage update. New Stage: {StageName}. Error: {ErrorMessage}",
                        id, dto.StageName, ex_history.Message);

                }


                return Ok(updatedDealDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while updating stage for deal ID {DealId}: {ErrorMessage}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating stage for deal ID {DealId}: {ErrorMessage}", id, ex.Message);
                return Problem($"An unexpected error occurred while updating deal stage: {ex.Message}", statusCode: 500);
            }
        }




        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DealReadDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<DealReadDto>> UpdateDeal(long id, [FromBody] DealEditDto dto)
        {
            _logger.LogInformation($"Attempting to update deal with ID {id} with payload: {@dto}", dto);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for DealEditDto: {@ModelState}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                var updatedDealDto = await _dealRepository.UpdateDealAsync(id, dto);
                if (updatedDealDto == null)
                {
                    _logger.LogWarning($"Deal with ID {id} not found.");
                    return NotFound($"Deal with ID {id} not found.");
                }
                _logger.LogInformation($"Deal with ID {id} updated successfully.");
                return Ok(updatedDealDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while updating deal: {Message}. Inner Exception: {InnerException}", ex.Message, ex.InnerException?.ToString());
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating deal: {Message}. Inner Exception: {InnerException}", ex.Message, ex.InnerException?.ToString());
                return Problem($"An unexpected error occurred: {ex.Message}", statusCode: 500);
            }
        }
    }
}