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
    public class RevenueTypeController : ControllerBase
    {
        private readonly IRevenueType _revenueTypeRepository;

        public RevenueTypeController(IRevenueType revenueTypeRepository)
        {
            _revenueTypeRepository = revenueTypeRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<RevenueType>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<RevenueType>>> GetAllRevenueTypesAsync()
        {
            
            var revenueTypes = await _revenueTypeRepository.GetRevenueTypes(); 
            if (revenueTypes == null)
            {
                return NotFound("Revenue type data is currently unavailable or no revenue types found.");
            }
            return Ok(revenueTypes);
        }
    }
}