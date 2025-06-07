using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.DTOs;
using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IContactRepository
    {
       
        Task<Contact> AddContactAsync(ContactCreateDto dto);
        Task<IEnumerable<ContactReadDto>> GetAllContactsAsync();
        Task<Contact?> GetByFullNameAndCustomerIdAsync(string firstName, string? lastName, long customerId);
        Task<Contact?> UpdateContactAsync(long id, ContactUpdateDto dto);
        Task<Contact?> SoftDeleteContactAsync(long id);
    }
}