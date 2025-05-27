// Controllers/UserController.cs
using Microsoft.AspNetCore.Mvc;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserReadDto>>> GetUsers()
        {
            var users = await _userRepository.GetAllUsersAsync();
            return Ok(users);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserReadDto>> GetUser(long id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // GET: api/User/name/5
        [HttpGet("name/{id}")]
        public async Task<ActionResult<string>> GetUserName(long id)
        {
            var userName = await _userRepository.GetUserNameByIdAsync(id);
            return Ok(userName);
        }

        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<UserReadDto>> CreateUser(UserCreateDto userCreateDto)
        {
            var user = await _userRepository.CreateUserAsync(userCreateDto);
            var readDto = await _userRepository.GetUserByIdAsync(user.Id);

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, readDto);
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(long id, UserCreateDto userUpdateDto)
        {
            var user = await _userRepository.UpdateUserAsync(id, userUpdateDto);

            if (user == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(long id)
        {
            await _userRepository.DeleteUserAsync(id);
            return NoContent();
        }
    }
}