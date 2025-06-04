using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.Interfaces;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueTypeController : ControllerBase
    {
        private IRevenueType _revenuetypeRepository;
        public RevenueTypeController(IRevenueType revenuetypeRepository)
        {
            _revenuetypeRepository = revenuetypeRepository;
        }

        [HttpGet]

        public async Task<IActionResult> Get()
        {
            var revenueTypes = await _revenuetypeRepository.GetRevenueTypes();
            if (revenueTypes == null)
            {
                return NotFound();
            }
            return Ok(revenueTypes);
        }
    }
}
