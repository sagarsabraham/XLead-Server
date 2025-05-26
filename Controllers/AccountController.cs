using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Models;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public AccountController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Account
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Account>>> GetAccounts()
        {
            // Only select the needed fields for the dropdown
            var accounts = await _context.Accounts
                .Select(a => new { a.Id, a.AccountName })
                .ToListAsync();

            return Ok(accounts);
        }
    }
}