// Controllers/RegionController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Models;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegionController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public RegionController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Region
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Region>>> GetRegions()
        {
            var regions = await _context.Regions
                .Select(r => new { r.Id, r.RegionName })
                .ToListAsync();

            return Ok(regions);
        }
    }
}