
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
        private readonly ILogger<CustomerContactController> _logger;
        public CustomerContactController(
            ICustomerRepository customerService,
            IContactRepository contactService,
            IMapper mapper,
            IUserPrivilegeRepository userPrivilegeRepository,
            ILogger<CustomerContactController> logger)
        {
            _customerService = customerService;
            _contactService = contactService;
            _mapper = mapper;
            _userPrivilegeRepository = userPrivilegeRepository;
            _logger = logger;
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

      
        [HttpGet("customers/{requestingUserId}")] 
        public async Task<IActionResult> GetCustomers(long requestingUserId)
        {
            _logger.LogInformation("Attempting to get customers for requesting user ID: {RequestingUserId}", requestingUserId);
           
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(requestingUserId);
            if (privileges == null || !privileges.Any(p => p.PrivilegeName == "ViewCustomers" || p.PrivilegeName == "ViewTeamCustomers"))
            {
                _logger.LogWarning("User {RequestingUserId} lacks necessary privilege to view customers.", requestingUserId);
                return Forbid("User lacks privilege to view customers.");
            }

            var customers = await _customerService.GetAllCustomersAsync(requestingUserId);
            return Ok(customers);
        }

       
        [HttpGet("contacts/{requestingUserId}")] 
        public async Task<IActionResult> GetContacts(long requestingUserId)
        {
            _logger.LogInformation("Attempting to get contacts for requesting user ID: {RequestingUserId}", requestingUserId);
           
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(requestingUserId);
            if (privileges == null || !privileges.Any(p => p.PrivilegeName == "ViewContacts" || p.PrivilegeName == "ViewTeamContacts")) 
            {
                _logger.LogWarning("User {RequestingUserId} lacks necessary privilege to view contacts.", requestingUserId);
                return Forbid("User lacks privilege to view contacts.");
            }

            var contacts = await _contactService.GetAllContactsAsync(requestingUserId); 
            return Ok(contacts);
        }

      

      

        [HttpPut("customer/{id}")]
        public async Task<IActionResult> UpdateCustomer(long id, [FromBody] CustomerUpdateDto dto)
        {
            _logger.LogInformation("Attempting to update customer ID {CustomerId} by User ID {PerformingUserId}", id, dto.UpdatedBy);

            if (dto.UpdatedBy <= 0)
            {
                return BadRequest("Performing User ID must be specified.");
            }

            var customerToUpdate = await _customerService.GetCustomerByIdAsync(id); 
            if (customerToUpdate == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            
            if (customerToUpdate.CreatedBy != dto.UpdatedBy)
            {
                _logger.LogWarning("User {PerformingUserId} is not the creator of customer {CustomerId} (Creator: {CreatorId}). Update forbidden.", dto.UpdatedBy, id, customerToUpdate.CreatedBy);
                return Forbid("User is not authorized to update this customer.");
            }

            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(dto.UpdatedBy);
            if (!privileges.Any(p => p.PrivilegeName == "EditOwnCustomer"))
            {
                _logger.LogWarning("User {PerformingUserId} lacks 'EditOwnCustomer' privilege for customer ID {CustomerId}.", dto.UpdatedBy, id);
                return Forbid("User lacks privilege to edit this customer.");
            }

            _logger.LogInformation("User {PerformingUserId} authorized to update customer {CustomerId}.", dto.UpdatedBy, id);
            try
            {
             
                var updatedCustomer = await _customerService.UpdateCustomerAsync(id, dto); 
                var resultDto = _mapper.Map<CustomerReadDto>(updatedCustomer);
                return Ok(resultDto);
            }
            catch (ArgumentException ex) 
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred while updating the customer.");
            }
        }

        [HttpPut("contact/{id}")]
        public async Task<IActionResult> UpdateContact(long id, [FromBody] ContactUpdateDto dto)
        {
            _logger.LogInformation("Attempting to update contact ID {ContactId} by User ID {PerformingUserId}", id, dto.UpdatedBy);

            if (dto.UpdatedBy <= 0)
            {
                return BadRequest("Performing User ID must be specified.");
            }

            var contactToUpdate = await _contactService.GetContactByIdAsync(id);
            if (contactToUpdate == null)
            {
                return NotFound($"Contact with ID {id} not found.");
            }

            if (contactToUpdate.CreatedBy != dto.UpdatedBy)
            {
                _logger.LogWarning("User {PerformingUserId} is not the creator of contact {ContactId} (Creator: {CreatorId}). Update forbidden.", dto.UpdatedBy, id, contactToUpdate.CreatedBy);
                return Forbid("User is not authorized to update this contact.");
            }

            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(dto.UpdatedBy);
            if (!privileges.Any(p => p.PrivilegeName == "EditOwnContact")) 
            {
                _logger.LogWarning("User {PerformingUserId} lacks 'EditOwnContact' privilege for contact ID {ContactId}.", dto.UpdatedBy, id);
                return Forbid("User lacks privilege to edit this contact.");
            }

            _logger.LogInformation("User {PerformingUserId} authorized to update contact {ContactId}.", dto.UpdatedBy, id);
            try
            {
                var updatedContact = await _contactService.UpdateContactAsync(id, dto);
                var resultDto = _mapper.Map<ContactReadDto>(updatedContact);
                return Ok(resultDto);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating contact {ContactId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "An internal error occurred while updating contact.");
            }
        }

        [HttpDelete("customer/{id}")]
        public async Task<IActionResult> SoftDeleteCustomer(long id, [FromQuery] long performingUserId) // Get performing user from query
        {
            _logger.LogInformation("Attempting to soft delete customer ID {CustomerId} by User ID {PerformingUserId}", id, performingUserId);

            if (performingUserId <= 0)
            {
                return BadRequest("Performing User ID must be specified.");
            }

            var customerToDelete = await _customerService.GetCustomerByIdAsync(id);
            if (customerToDelete == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            if (customerToDelete.CreatedBy != performingUserId)
            {
                _logger.LogWarning("User {PerformingUserId} is not the creator of customer {CustomerId} (Creator: {CreatorId}). Delete forbidden.", performingUserId, id, customerToDelete.CreatedBy);
                return Forbid("User is not authorized to delete this customer.");
            }

            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(performingUserId);
            if (!privileges.Any(p => p.PrivilegeName == "DeleteOwnCustomer")) 
            {
                _logger.LogWarning("User {PerformingUserId} lacks 'DeleteOwnCustomer' privilege for customer ID {CustomerId}.", performingUserId, id);
                return Forbid("User lacks privilege to delete this customer.");
            }

            _logger.LogInformation("User {PerformingUserId} authorized to delete customer {CustomerId}.", performingUserId, id);
            var result = await _customerService.SoftDeleteCustomerAsync(id, performingUserId); 
            if (result == null) 
            {
                return NotFound($"Customer with ID {id} not found during delete operation.");
            }
            return NoContent();
        }

        [HttpDelete("contact/{id}")]
        public async Task<IActionResult> SoftDeleteContact(long id, [FromQuery] long performingUserId) 
        {
            _logger.LogInformation("Attempting to soft delete contact ID {ContactId} by User ID {PerformingUserId}", id, performingUserId);
            if (performingUserId <= 0)
            {
                return BadRequest("Performing User ID must be specified.");
            }

            var contactToDelete = await _contactService.GetContactByIdAsync(id);
            if (contactToDelete == null)
            {
                return NotFound($"Contact with ID {id} not found.");
            }

            if (contactToDelete.CreatedBy != performingUserId)
            {
                _logger.LogWarning("User {PerformingUserId} is not the creator of contact {ContactId} (Creator: {CreatorId}). Delete forbidden.", performingUserId, id, contactToDelete.CreatedBy);
                return Forbid("User is not authorized to delete this contact.");
            }

            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(performingUserId);
            if (!privileges.Any(p => p.PrivilegeName == "DeleteOwnContact")) 
            {
                _logger.LogWarning("User {PerformingUserId} lacks 'DeleteOwnContact' privilege for contact ID {ContactId}.", performingUserId, id);
                return Forbid("User lacks privilege to delete this contact.");
            }

            _logger.LogInformation("User {PerformingUserId} authorized to delete contact {ContactId}.", performingUserId, id);
            var result = await _contactService.SoftDeleteContactAsync(id, performingUserId);
            if (result == null)
            {
                return NotFound($"Contact with ID {id} not found during delete operation.");
            }
            return NoContent();
        }
        

      

    }
}