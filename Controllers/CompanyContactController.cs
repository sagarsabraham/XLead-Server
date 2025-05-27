using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyContactController : ControllerBase
    {
        private readonly ICompanyRepository _companyService;
        private readonly IContactRepository _contactService;
        private readonly IMapper _mapper;

        public CompanyContactController(ICompanyRepository companyService, IContactRepository contactService, IMapper mapper)
        {
            _companyService = companyService;
            _contactService = contactService;
            _mapper = mapper;
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
            var company = await _companyService.GetByNameAsync(dto.CompanyName);
            if (company == null) return BadRequest("Company not found");

            dto.CompanyId = company.Id;

            var contactEntity = await _contactService.AddContactAsync(dto);

            var contactDto = _mapper.Map<ContactReadDto>(contactEntity); // ✅ safer
            return Ok(contactDto);
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
