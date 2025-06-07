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
        [ProducesResponseType(typeof(IEnumerable<PrivilegeReadDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PrivilegeReadDto>>> GetPrivilegesByUserIdAsync(long userId)
        {
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(userId);


            if (privileges == null)
            {
                return NotFound($"Privileges for user ID {userId} not found or user does not exist.");
            }
            return Ok(privileges);
        }
    }
}
