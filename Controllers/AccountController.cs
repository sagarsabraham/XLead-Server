using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.Controllers;
using XLead_Server.Interfaces;
using XLead_Server.Repositories;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IAccountRepository _accountRepository;
        public AccountController(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var accounts = await _accountRepository.GetAllAccounts();
            if (accounts == null)
            {
                return NotFound();
            }
            return Ok(accounts);
        }
    }
}