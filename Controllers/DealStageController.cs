using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.Controllers;
using XLead_Server.Interfaces;
using XLead_Server.Repositories;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealStageController : ControllerBase
    {
        private IDealStageRepository _dealStageRepository;
        public DealStageController(IDealStageRepository dealStageRepository)
        {
            _dealStageRepository = dealStageRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var dealStages = await _dealStageRepository.GetAllDealStages();
            if (dealStages == null)
            {
                return NotFound();
            }
            return Ok(dealStages);
        }
    }
}

