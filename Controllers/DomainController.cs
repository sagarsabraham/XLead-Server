using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.Interfaces;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DomainController : ControllerBase
    {
        private IDomainRepository _domainRepository;
        public DomainController(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        // GET: api/Domain
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var domains = await _domainRepository.GetAllDomains();
            if (domains == null)
            {
                return NotFound();
            }
            return Ok(domains);
        }
    }
}
