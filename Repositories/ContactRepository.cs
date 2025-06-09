using AutoMapper;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Helpers;
using XLead_Server.Interfaces;



using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class ContactRepository : IContactRepository
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public ContactRepository(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
        private async Task ValidateGlobalContactUniqueness(string email, string phoneNumber, long? contactIdToExclude = null)
        {
            
            if (!string.IsNullOrWhiteSpace(email))
            {
                var normalizedEmail = email.Trim().ToUpper();
                var contactEmailQuery = _context.Contacts.AsQueryable();
                if (contactIdToExclude.HasValue) contactEmailQuery = contactEmailQuery.Where(c => c.Id != contactIdToExclude.Value);

                if (await contactEmailQuery.AnyAsync(c => c.Email.ToUpper() == normalizedEmail))
                {
                    throw new ArgumentException($"The email '{email}' is already in use by another contact.");
                }
            }

        
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                var normalizedPhone = NormalizationHelper.NormalizePhoneNumber(phoneNumber);

                var contactPhoneQuery = _context.Contacts.AsQueryable();
                if (contactIdToExclude.HasValue) contactPhoneQuery = contactPhoneQuery.Where(c => c.Id != contactIdToExclude.Value);
                if (await contactPhoneQuery.AnyAsync(c => EF.Functions.Like(c.PhoneNumber.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", ""), $"%{normalizedPhone}%")))
                {
                    throw new ArgumentException($"The phone number '{phoneNumber}' is already in use by another contact.");
                }

                
                if (await _context.Customers.AnyAsync(c => EF.Functions.Like(c.CustomerPhoneNumber.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", ""), $"%{normalizedPhone}%")))
                {
                    throw new ArgumentException($"The phone number '{phoneNumber}' is already in use by a customer.");
                }
            }
        }

       
        public async Task<Contact> AddContactAsync(ContactCreateDto dto)

        {
            await ValidateGlobalContactUniqueness(dto.Email, dto.PhoneNumber);
           
            var contact = _mapper.Map<Contact>(dto);
            contact.CreatedAt = DateTime.UtcNow;
            contact.IsActive = true;

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }

        public async Task<IEnumerable<ContactReadDto>> GetAllContactsAsync()
        {
            
            var contacts = await _context.Contacts
                .Where(c => c.IsHidden != true)
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

        public async Task<Contact?> UpdateContactAsync(long id, ContactUpdateDto dto)
        {
            var existingContact = await _context.Contacts.FindAsync(id);

            if (existingContact == null)
            {
                return null;
            }

            await ValidateGlobalContactUniqueness(dto.Email, dto.PhoneNumber, id);

            existingContact.FirstName = dto.FirstName;
            existingContact.LastName = dto.LastName;
            existingContact.Designation = dto.Designation;
            existingContact.Email = dto.Email;
            existingContact.PhoneNumber = dto.PhoneNumber;
            existingContact.IsActive = dto.IsActive;
            existingContact.UpdatedAt = DateTime.UtcNow;
            existingContact.UpdatedBy = dto.UpdatedBy;


            await _context.SaveChangesAsync();
            return existingContact;

            
        }

        public async Task<Contact?> SoftDeleteContactAsync(long id)
        {
            var contactToDelete = await _context.Contacts.FindAsync(id);
            if (contactToDelete == null)
            {
                return null;
            }

            contactToDelete.IsHidden = true;
            contactToDelete.IsActive = false;
            contactToDelete.UpdatedAt = DateTime.UtcNow;


            await _context.SaveChangesAsync();
            return contactToDelete;
        }

    }


}