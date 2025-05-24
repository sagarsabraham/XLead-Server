using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyContactController : ControllerBase
    {
        private readonly ICompanyRepository _companyService;
        private readonly IContactRepository _contactService;

        public CompanyContactController(ICompanyRepository companyService, IContactRepository contactService)
        {
            _companyService = companyService;
            _contactService = contactService;
        }

        [HttpPost("company")]
        public async Task<IActionResult> AddCompany([FromBody] CompanyCreateDto dto)
        {
            var result = await _companyService.AddCompanyAsync(dto);
            return Ok(result);
        }

        [HttpPost("contact")]
        public async Task<IActionResult> AddContact([FromBody] ContactCreateDto dto)
        {
            var result = await _contactService.AddContactAsync(dto);
            return Ok(result);
        }

        [HttpGet("company-contact-map")]
        public async Task<IActionResult> GetCompanyContactMap()
        {
            var map = await _companyService.GetCompanyContactMapAsync();
            return Ok(map);
        }
        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts()
        {
            var contacts = await _contactService.GetAllContactsAsync();
            return Ok(contacts);
        }

    }
}
