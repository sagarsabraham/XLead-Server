// Controllers/DealStageController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Models;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealStageController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public DealStageController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/DealStage
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetStages()
        {
            // For a dropdown, we want unique stage names/display names
            var stages = await _context.DealStages
                .Select(s => new { s.Id, s.StageName, s.DisplayName })
                .Distinct()
                .ToListAsync();

            return Ok(stages);
        }
    }
}