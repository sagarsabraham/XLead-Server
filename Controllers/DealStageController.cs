
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealStageController : ControllerBase
    {
        private readonly IDealStageRepository _dealStageRepository;

        public DealStageController(IDealStageRepository dealStageRepository)
        {
            _dealStageRepository = dealStageRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DealStage>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<DealStage>>> GetAllDealStagesAsync()
        {
            var dealStages = await _dealStageRepository.GetAllDealStages();
            if (dealStages == null)
            {
                return NotFound("Deal stage data is currently unavailable or no deal stages found.");
            }
            return Ok(dealStages);
        }
    }
}
