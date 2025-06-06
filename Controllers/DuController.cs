
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DuController : ControllerBase
    {
        private readonly IDuRepository _duRepository;

        public DuController(IDuRepository duRepository)
        {
            _duRepository = duRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DU>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<DU>>> GetAllDusAsync()
        {
            var dus = await _duRepository.GetDus();
            if (dus == null)
            {
                return NotFound("DU data is currently unavailable or no DUs found.");
            }
            return Ok(dus);
        }
    }
}
