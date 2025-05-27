using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.Interfaces;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private IRegionRepository _regionRepository;
        public RegionController(IRegionRepository regionRepository)
        {
            _regionRepository = regionRepository;
        }

        // GET: api/Region
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var regions = await _regionRepository.GetAllRegions();
            if (regions == null)
            {
                return NotFound();
            }
            return Ok(regions);
        }
    }
}
