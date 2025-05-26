using XLead_Server.DTOs;
using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IContactRepository
    {
        Task<IEnumerable<Contact>> GetAllContacts();
        Task<Contact> AddContactAsync(ContactCreateDto dto);
        Task<IEnumerable<ContactReadDto>> GetAllContactsAsync();
    }
}