
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
    public class RegionController : ControllerBase
    {
        private readonly IRegionRepository _regionRepository;

        public RegionController(IRegionRepository regionRepository)
        {
            _regionRepository = regionRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Region>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Region>>> GetAllRegionsAsync()
        {
            var regions = await _regionRepository.GetAllRegions();
            if (regions == null)
            {
                return NotFound("Region data is currently unavailable or no regions found.");
            }
            return Ok(regions);
        }
    }
}
