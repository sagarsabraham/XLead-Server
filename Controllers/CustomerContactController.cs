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
        private readonly IUserPrivilegeRepository _userPrivilegeRepository;

        public CustomerContactController(
            ICustomerRepository customerService,
            IContactRepository contactService,
            IMapper mapper,
            IUserPrivilegeRepository userPrivilegeRepository)
        {
            _customerService = customerService;
            _contactService = contactService;
            _mapper = mapper;
            _userPrivilegeRepository = userPrivilegeRepository;
        }

        [HttpPost("customer")]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerCreateDto dto)
        {
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(dto.CreatedBy);
            if (!privileges.Any(p => p.PrivilegeName == "CreateCustomer"))
            {
                return Forbid("User lacks CreateCustomer privilege");
            }

            try
            {
                var result = await _customerService.AddCustomerAsync(dto);
                var resultDto = _mapper.Map<CustomerReadDto>(result);
                return Ok(resultDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

       
        [HttpPost("contact")]
        public async Task<IActionResult> AddContact([FromBody] ContactCreateDto dto)
        {
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(dto.CreatedBy);
            if (!privileges.Any(p => p.PrivilegeName == "CreateContact"))
            {
                return Forbid("User lacks CreateContact privilege");
            }

            var customer = await _customerService.GetByNameAsync(dto.CustomerName);
            if (customer == null) return BadRequest("Customer not found");

            dto.CustomerId = customer.Id;

            try
            {
                var contactEntity = await _contactService.AddContactAsync(dto);
                var contactDto = _mapper.Map<ContactReadDto>(contactEntity);
                return Ok(contactDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
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
        [HttpPut("customer/{id}")]

        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerUpdateDto dto)

        {

            try

            {

                var updatedCustomer = await _customerService.UpdateCustomerAsync(id, dto);

                if (updatedCustomer == null)

                {

                    return NotFound($"Customer with ID {id} not found.");

                }

                var resultDto = _mapper.Map<CustomerReadDto>(updatedCustomer);

                return Ok(resultDto);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { message = "An internal error occurred while updating the customer.", details = ex.ToString() });

            }

        }


        [HttpPut("contact/{id}")]

        public async Task<IActionResult> UpdateContact(long id, [FromBody] ContactUpdateDto dto)

        {

            try

            {

                var updatedContact = await _contactService.UpdateContactAsync(id, dto);

                if (updatedContact == null)

                {

                    return NotFound($"Contact with ID {id} not found.");

                }

                var resultDto = _mapper.Map<ContactReadDto>(updatedContact);

                return Ok(resultDto);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { message = "An internal error occurred while updating the contact.", details = ex.ToString() });

            }

        }

        [HttpDelete("customer/{id}")]

        public async Task<IActionResult> SoftDeleteCustomer(long id)

        {


            var result = await _customerService.SoftDeleteCustomerAsync(id);

            if (result == null)

            {

                return NotFound($"Customer with ID {id} not found.");

            }

            return NoContent();

        }

        [HttpDelete("contact/{id}")]

        public async Task<IActionResult> SoftDeleteContact(long id)

        {


            var result = await _contactService.SoftDeleteContactAsync(id);

            if (result == null)

            {

                return NotFound($"Contact with ID {id} not found.");

            }

            return NoContent(); // Success

        }

    }

}
 
    
