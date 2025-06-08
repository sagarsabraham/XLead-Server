
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        private readonly IUserPrivilegeRepository _userPrivilegeRepository; 
        private readonly ILogger<ContactRepository> _logger;

        public ContactRepository(ApiDbContext context, IMapper mapper, IUserPrivilegeRepository userPrivilegeRepository, ILogger<ContactRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _userPrivilegeRepository = userPrivilegeRepository;
            _logger = logger;
        }
        private async Task<List<long>> GetRelevantCreatorIdsAsync(long requestingUserId, string overviewPrivilegeName)
        {
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(requestingUserId);
            bool hasOverviewPrivilege = privileges?.Any(p => p.PrivilegeName == overviewPrivilegeName) ?? false;

            if (hasOverviewPrivilege)
            {
                _logger.LogInformation("User {RequestingUserId} has '{OverviewPrivilegeName}'. Fetching data for subordinates.", requestingUserId, overviewPrivilegeName);
                var subordinateIds = await _context.Users
                    .Where(u => u.AssignedTo == requestingUserId)
                    .Select(u => u.Id)
                    .ToListAsync();
                return subordinateIds;
            }
            else
            {
                _logger.LogInformation("User {RequestingUserId} does not have '{OverviewPrivilegeName}'. Fetching own data.", requestingUserId, overviewPrivilegeName);
                return new List<long> { requestingUserId };
            }
        }

        public async Task<Contact> AddContactAsync(ContactCreateDto dto)
        {
            Console.WriteLine($"Received Designation: {dto.Designation}");
            var contact = _mapper.Map<Contact>(dto);
            contact.CreatedAt = DateTime.UtcNow;
            contact.IsActive = true;

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

       
        public async Task<IEnumerable<ContactReadDto>> GetAllContactsAsync(long requestingUserId)
        {
            
            var relevantCreatorIds = await GetRelevantCreatorIdsAsync(requestingUserId, "ViewTeamContacts"); 

            if (!relevantCreatorIds.Any())
            {
                _logger.LogInformation("No relevant creator IDs for contacts for user {RequestingUserId}.", requestingUserId);
                return Enumerable.Empty<ContactReadDto>();
            }

            _logger.LogInformation("Fetching contacts created by User IDs: [{RelevantCreatorIds}] for Requesting User: {RequestingUserId}", string.Join(",", relevantCreatorIds), requestingUserId);
            var contacts = await _context.Contacts
                .Where(c => (c.IsHidden == null || c.IsHidden == false) &&
                             relevantCreatorIds.Contains(c.CreatedBy)) 
                .ToListAsync();
            return _mapper.Map<IEnumerable<ContactReadDto>>(contacts);
        }
        public async Task<Contact?> GetByFullNameAndCustomerIdAsync(string firstName, string? lastName, long customerId)
        {
            return await _context.Contacts
                .FirstOrDefaultAsync(c => c.FirstName == firstName &&
                                         (lastName == null || c.LastName == lastName) &&
                                         c.CustomerId == customerId);
        }
      
        public async Task<Contact?> GetContactByIdAsync(long id)
        {
            _logger.LogInformation("Fetching contact entity with ID: {ContactId}", id);
            
            return await _context.Contacts.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id && (c.IsHidden == null || c.IsHidden == false));
        }

       
        public async Task<Contact?> UpdateContactAsync(long id, ContactUpdateDto dto)
        {
            _logger.LogInformation("Attempting to update contact ID {ContactId} by User ID {PerformingUserId}", id, dto.UpdatedBy);
            var existingContact = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id);

            if (existingContact == null || existingContact.IsHidden == true) 
            {
                _logger.LogWarning("Contact with ID {ContactId} not found or is hidden, cannot update.", id);
                return null;
            }

           
            _mapper.Map(dto, existingContact);

            existingContact.UpdatedBy = dto.UpdatedBy; 
            existingContact.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated contact ID {ContactId}", id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while updating contact ID {ContactId}", id);
                throw; 
            }
            return existingContact;
        }

    
        public async Task<Contact?> SoftDeleteContactAsync(long id, long performingUserId)
        {
            _logger.LogInformation("Attempting to soft delete contact ID {ContactId} by User ID {PerformingUserId}", id, performingUserId);
            var contactToDelete = await _context.Contacts.FirstOrDefaultAsync(c => c.Id == id);

            if (contactToDelete == null || contactToDelete.IsHidden == true) 
            {
                _logger.LogWarning("Contact with ID {ContactId} not found or already hidden, cannot soft delete.", id);
                return null;
            }

            contactToDelete.IsHidden = true;
            contactToDelete.IsActive = false; 
            contactToDelete.UpdatedBy = performingUserId;
            contactToDelete.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Successfully soft-deleted contact ID {ContactId}", id);
            return contactToDelete;
        }

    }
}