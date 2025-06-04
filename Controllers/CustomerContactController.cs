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
    public class CustomerContactController : ControllerBase
    {
        private readonly ICustomerRepository _customerService;
        private readonly IContactRepository _contactService;
        private readonly IMapper _mapper;

        public CustomerContactController(ICustomerRepository customerService, IContactRepository contactService, IMapper mapper)
        {
            _customerService = customerService;
            _contactService = contactService;
            _mapper = mapper;
        }

        [HttpPost("customer")]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerCreateDto dto)
        {

            var result = await _customerService.AddCustomerAsync(dto);
            return Ok(result);
        }
       

        [HttpPost("contact")]
        public async Task<IActionResult> AddContact([FromBody] ContactCreateDto dto)
        {
            var customer = await _customerService.GetByNameAsync(dto.CustomerName);
            if (customer == null) return BadRequest("Customer not found");

            dto.CustomerId = customer.Id;

            var contactEntity = await _contactService.AddContactAsync(dto);

            var contactDto = _mapper.Map<ContactReadDto>(contactEntity); // ✅ safer
            return Ok(contactDto);
        }




        [HttpGet("customer-contact-map")]
        public async Task<IActionResult> GetCustomerContactMap()
        {
            var map = await _customerService.GetCustomerContactMapAsync();
            return Ok(map);
        }
        [HttpGet("customers")]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _customerService.GetAllCustomersAsync();
            return Ok(customers);
        }

        [HttpGet("contacts")]
        public async Task<IActionResult> GetContacts()
        {
            var contacts = await _contactService.GetAllContactsAsync();
            return Ok(contacts);
        }

    }
}
