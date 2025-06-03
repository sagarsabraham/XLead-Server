using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.Interfaces;
using XLead_Server.Repositories;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IndustryVerticalController : ControllerBase
    {
        private IndustryVerticalRepository _industryverticalRepository;
        public IndustryVerticalController(IIndustryVertical industryVertical)
        {
            _industryverticalRepository = (IndustryVerticalRepository?)industryVertical;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var industry = await _industryverticalRepository.GetAllIndustryVertical();
            if (industry == null)
            {
                return NotFound();
            }
            return Ok(industry);
        }
    }
}
