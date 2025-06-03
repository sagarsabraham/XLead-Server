using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPrivilegesController : ControllerBase
    {
        private readonly IUserPrivilegeRepository _userPrivilegeRepository;

        public UserPrivilegesController(IUserPrivilegeRepository userPrivilegeRepository)
        {
            _userPrivilegeRepository = userPrivilegeRepository;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<IEnumerable<PrivilegeReadDto>>> GetPrivilegesByUser(long userId)
        {
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(userId);
            return Ok(privileges);
        }
    }
}
