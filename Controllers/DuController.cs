using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.Interfaces;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DuController : ControllerBase
    {
        private IDuRepository _duRepository;
        public DuController(IDuRepository duRepository)
        {
            _duRepository = duRepository;
        }

        [HttpGet]

        public async Task<IActionResult> Get()
        {
            var dus = await _duRepository.GetDus();
            if (dus == null)
            {
                return NotFound();
            }
            return Ok(dus);
        }
    }
}
