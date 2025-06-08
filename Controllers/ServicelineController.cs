using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.Interfaces;
using XLead_Server.Models;
using XLead_Server.Repositories;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceLineController : ControllerBase
    {
        private readonly IServiceline _serviceLineRepository;

        public ServiceLineController(IServiceline serviceLineRepository)
        {
            _serviceLineRepository = serviceLineRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ServiceLine>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<ServiceLine>>> GetAllServiceLinesAsync()
        {
            var serviceLines = await _serviceLineRepository.GetServiceTypes();
            if (serviceLines == null)
            {
                return NotFound("Service line data is currently unavailable or no service lines found.");
            }
            return Ok(serviceLines);
        }
    }
}