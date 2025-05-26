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
        public async Task<IEnumerable<Contact>> GetAllContacts()
        {
            return await _context.Contacts.ToListAsync();
        }
        public async Task<Contact> AddContactAsync(ContactCreateDto dto)
        {
            var contact = _mapper.Map<Contact>(dto);
            contact.IsActive = true;
            contact.CreatedBy = 1;
            contact.UpdatedAt = DateTime.UtcNow;

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();
            return contact;
        }
        public async Task<IEnumerable<ContactReadDto>> GetAllContactsAsync()
        {
            var contacts = await _context.Contacts.Include(c => c.Company).ToListAsync();
            return _mapper.Map<IEnumerable<ContactReadDto>>(contacts);
        }
    }
}