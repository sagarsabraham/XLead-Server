using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using XLead_Server.Data;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public CountController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Count/ClosedWonCount
        [HttpGet("ClosedWonCount")]
        public async Task<IActionResult> GetClosedWonCount()
        {
            var closedWonCount = await _context.Deals
                .Join(_context.DealStages,
                      d => d.DealStageId,
                      ds => ds.Id,
                      (d, ds) => new { d, ds })
                .Where(x => x.ds.DisplayName == "Closed Won")
                .CountAsync();

            return Ok(new { DealCount = closedWonCount });
        }
    }
}