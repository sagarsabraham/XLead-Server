
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
    public class DomainController : ControllerBase
    {
        private readonly IDomainRepository _domainRepository;

        public DomainController(IDomainRepository domainRepository)
        {
            _domainRepository = domainRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Domain>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Domain>>> GetAllDomainsAsync()
        {
            var domains = await _domainRepository.GetAllDomains();
            if (domains == null)
            {
                return NotFound("Domain data is currently unavailable or no domains found.");
            }
            return Ok(domains);
        }
    }
}
