// Controllers/DomainController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Models;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DomainController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public DomainController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Domain
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetDomains()
        {
            var domains = await _context.Domains
                .Select(d => new { d.Id, d.DomainName })
                .ToListAsync();

            return Ok(domains);
        }
    }
}