
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.Interfaces;
using XLead_Server.Models;
using XLead_Server.Repositories;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndustryVerticalController : ControllerBase
    {
        private readonly IIndustryVertical _industryVerticalRepository;

        public IndustryVerticalController(IIndustryVertical industryVerticalRepository)
        {
            _industryVerticalRepository = industryVerticalRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<IndustrialVertical>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<IndustrialVertical>>> GetAllIndustryVerticalsAsync()
        {
            var industryVerticals = await _industryVerticalRepository.GetAllIndustryVertical();
            if (industryVerticals == null)
            {
                return NotFound("Industry vertical data is currently unavailable or no industry verticals found.");
            }
            return Ok(industryVerticals);
        }
    }
}
