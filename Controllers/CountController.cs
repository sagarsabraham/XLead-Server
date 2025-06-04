using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using XLead_Server.Data;
using XLead_Server.Models;
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
        [HttpGet("ClosedWonCount")]
        public async Task<IActionResult> GetClosedWonCount()
        {
            var closedWonCount = await _context.DealStages
                .Where(ds => ds.DisplayName.ToLower() == "closed won")
                .Select(ds => ds.Id)
                .Distinct()
                .CountAsync();

            return Ok(new { ClosedWonCounts = closedWonCount });
        }

    }
}




            



