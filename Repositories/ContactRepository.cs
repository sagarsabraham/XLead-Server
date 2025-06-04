// XLead_Server/Repositories/ContactRepository.cs
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
            var contacts = await _context.Contacts.ToListAsync();
            return _mapper.Map<IEnumerable<ContactReadDto>>(contacts);
        }

        public async Task<Contact?> GetByFullNameAndCustomerIdAsync(string firstName, string? lastName, long customerId)
        {
            return await _context.Contacts
                .FirstOrDefaultAsync(c => c.FirstName == firstName &&
                                         (lastName == null || c.LastName == lastName) &&
                                         c.CustomerId == customerId);
        }
    }
}