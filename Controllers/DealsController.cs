using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using System.Collections.Generic; 

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
        public async Task<IActionResult> UpdateDealStage(long id, [FromBody] DealUpdateDto dto)
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
        [HttpGet("{id}/history")]
        [ProducesResponseType(typeof(IEnumerable<StageHistoryReadDto>), 200)]
        [ProducesResponseType(404)] 
        [ProducesResponseType(500)]
        public async Task<ActionResult<IEnumerable<StageHistoryReadDto>>> GetDealStageHistory(long id)
        {
            _logger.LogInformation("Fetching stage history for deal ID {DealId}", id);

         

            try
            {
                var history = await _stageHistoryRepository.GetStageHistoryByDealIdAsync(id);
                if (history == null || !history.Any())
                {
                    _logger.LogInformation("No stage history found for deal ID {DealId}", id);
                    return Ok(Enumerable.Empty<StageHistoryReadDto>());
                }
                return Ok(history);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching stage history for deal ID {DealId}: {ErrorMessage}", id, ex.Message);
                return Problem("An unexpected error occurred while fetching stage history.", statusCode: 500);
            }
        }

        [HttpGet("byCreator/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<DealReadDto>), 200)]
        [ProducesResponseType(404)] 
        public async Task<ActionResult<IEnumerable<DealReadDto>>> GetDealsByCreator(long userId)
        {
            _logger.LogInformation("Fetching deals for creator ID {UserId}", userId);

        
          

            var deals = await _dealRepository.GetDealsByCreatorIdAsync(userId);

            if (deals == null || !deals.Any())
            {
                _logger.LogInformation("No deals found for creator ID {UserId}", userId);
                return Ok(Enumerable.Empty<DealReadDto>());
            }
            return Ok(deals);
        }
     
    [HttpGet("manager-overview-deals/{managerId}")]
    [ProducesResponseType(typeof(IEnumerable<DealManagerOverviewDto>), 200)]
    [ProducesResponseType(403)] 
    [ProducesResponseType(404)] 
    public async Task<ActionResult<IEnumerable<DealManagerOverviewDto>>> GetManagerOverviewDealsList(long managerId)
    {
        _logger.LogInformation("Attempting to fetch manager overview deals for Manager ID: {ManagerIdFromRoute}", managerId);

        
        var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(managerId);
        if (privileges == null || !privileges.Any(p => p.PrivilegeName == "overview"))
        {
            _logger.LogWarning("User with ID {ManagerIdFromRoute} lacks 'Overview' privilege or does not exist.", managerId);
            return Forbid($"User ID {managerId} lacks the required 'Overview' privilege to view this data.");
        }

        _logger.LogInformation("User {ManagerIdFromRoute} confirmed to have 'Overview' privilege. Fetching deals for their direct reports.", managerId);
        var deals = await _dealRepository.GetDealsForManagerAsync(managerId); 

        if (deals == null || !deals.Any())
        {
            _logger.LogInformation("No deals found for manager overview (Manager ID: {ManagerIdFromRoute}).", managerId);
            return Ok(Enumerable.Empty<DealManagerOverviewDto>());
        }
        return Ok(deals);
    }


    [HttpGet("manager-overview-stage-counts/{managerId}")]
    [ProducesResponseType(typeof(IEnumerable<ManagerStageCountDto>), 200)]
    [ProducesResponseType(403)] 
    [ProducesResponseType(404)] 
    public async Task<ActionResult<IEnumerable<ManagerStageCountDto>>> GetManagerOverviewStageCountsData(long managerId)
    {
        _logger.LogInformation("Attempting to fetch manager overview stage counts for Manager ID: {ManagerIdFromRoute}", managerId);

      
        var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(managerId);
        if (privileges == null || !privileges.Any(p => p.PrivilegeName == "overview"))
        {
            _logger.LogWarning("User with ID {ManagerIdFromRoute} lacks 'Overview' privilege for stage counts or does not exist.", managerId);
            return Forbid($"User ID {managerId} lacks the required 'Overview' privilege to view this data.");
        }

        _logger.LogInformation("User {ManagerIdFromRoute} confirmed to have 'Overview' privilege. Fetching stage counts for their direct reports.", managerId);
        var counts = await _dealRepository.GetStageCountsForManagerAsync(managerId); 

        return Ok(counts);
    }


        [HttpGet("top-customers-by-revenue/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<TopCustomerDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<TopCustomerDto>>> GetTopCustomersData(
        long userId, 
        [FromQuery] int count = 5)
        {
            _logger.LogInformation($"Fetching top {count} customers by revenue for User ID: {userId}.");

            try
            {
                var topCustomers = await _dealRepository.GetTopCustomersByRevenueAsync(userId, count); 
                return Ok(topCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching top customers by revenue: {Message}", ex.Message);
                return Problem(
                    detail: "An unexpected error occurred while fetching top customers data.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Server Error");
            }
        }
        [HttpGet("dashboard-metrics/{userId}")]
        [ProducesResponseType(typeof(DashboardMetricsDto), 200)] // Assuming DTO is DashboardMetricsDto
        [ProducesResponseType(403)] // User lacks general permission to view any dashboard
        [ProducesResponseType(500)]
        public async Task<ActionResult<DashboardMetricsDto>> GetDashboardMetrics(long userId)
        {
            _logger.LogInformation($"Fetching dashboard metrics. Requesting User/Context User ID: {userId}");

            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(userId);
            if (privileges == null || !privileges.Any(p => p.PrivilegeName == "ViewOwnDashboard" || p.PrivilegeName == "Dashboard Overview")) // Example
            {
                _logger.LogWarning($"User {userId} lacks necessary privileges to view dashboard metrics.");
                return Forbid("User lacks permission to view dashboard metrics.");
            }

            try
            {
              
                var metrics = await _dealRepository.GetDashboardMetricsAsync(userId);
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching dashboard metrics for User ID {UserId}: {Message}", userId, ex.Message);
                return Problem("An unexpected error occurred while fetching dashboard metrics.", statusCode: 500);
            }
        }
        [HttpGet("open-pipeline-stages/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<PipelineStageDataDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PipelineStageDataDto>>> GetOpenPipelineStageData(long userId) 
        {
            _logger.LogInformation($"Fetching data for open pipeline stage graph for User ID: {userId}.");
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(userId);
            if (privileges == null || !privileges.Any(p => p.PrivilegeName == "ViewOwnDashboard" || p.PrivilegeName == "Dashboard Overview")) // Example
            {
                _logger.LogWarning($"User {userId} lacks necessary privileges to view dashboard metrics.");
                return Forbid("User lacks permission to view dashboard metrics.");
            }

            try
            {
                var stageData = await _dealRepository.GetOpenPipelineAmountsByStageAsync(userId); 
                return Ok(stageData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching open pipeline stage data: {Message}", ex.Message);
                return Problem(
                    detail: "An unexpected error occurred while fetching data for the pipeline stage graph.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Server Error");
            }
        }

        [HttpGet("monthly-revenue-won/{userId}")]
        [ProducesResponseType(typeof(IEnumerable<MonthlyRevenueDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<MonthlyRevenueDto>>> GetMonthlyRevenueData(
    long userId, 
    [FromQuery] int months = 12)
        {
            _logger.LogInformation($"Fetching monthly revenue data for the last {months} months for User ID: {userId}.");
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(userId);
            if (privileges == null || !privileges.Any(p => p.PrivilegeName == "ViewOwnDashboard" || p.PrivilegeName == "Dashboard Overview")) // Example
            {
                _logger.LogWarning($"User {userId} lacks necessary privileges to view dashboard metrics.");
                return Forbid("User lacks permission to view dashboard metrics.");
            }

            try
            {
                var revenueData = await _dealRepository.GetMonthlyRevenueWonAsync(userId, months); 
                return Ok(revenueData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching monthly revenue data: {Message}", ex.Message);
                return Problem(
                    detail: "An unexpected error occurred while fetching monthly revenue data.",
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Server Error");
            }
        }

    }
}