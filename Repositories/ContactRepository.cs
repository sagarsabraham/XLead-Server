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

        public ContactRepository(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
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

        public async Task<IEnumerable<ContactReadDto>> GetAllContactsAsync()
        {
            // FIX: Filter out records where IsHidden is explicitly true.
            // This correctly includes records where IsHidden is false or null.
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


            existingContact.FirstName = dto.FirstName;
            existingContact.LastName = dto.LastName;
            existingContact.Designation = dto.Designation;
            existingContact.Email = dto.Email;
            existingContact.PhoneNumber = dto.PhoneNumber;
            existingContact.IsActive = dto.IsActive;


            existingContact.UpdatedAt = DateTime.UtcNow;


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {

                Console.WriteLine(ex.ToString());
                throw;
            }

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