using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.Interfaces;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicelineController : ControllerBase
    {
        private IServiceline _serviceRepository;
        public ServicelineController(IServiceline serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }

        [HttpGet]

        public async Task<IActionResult> Get()
        {
            var serviceTypes = await _serviceRepository.GetServiceTypes();
            if (serviceTypes == null)
            {
                return NotFound();
            }
            return Ok(serviceTypes);
        }
    }
}
