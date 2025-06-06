// XLead_Server/Interfaces/IContactRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.DTOs; // For ContactReadDto, ContactCreateDto
using XLead_Server.Models; // For Contact model

namespace XLead_Server.Interfaces
{
    public interface IContactRepository
    {
        // Task<IEnumerable<Contact>> GetAllContacts(); // This was in your file, decide if needed
        Task<Contact> AddContactAsync(ContactCreateDto dto);
        Task<IEnumerable<ContactReadDto>> GetAllContactsAsync();

       
        Task<Contact?> GetByFullNameAndCustomerIdAsync(string firstName, string? lastName, long customerId);
        Task<Contact?> UpdateContactAsync(long id, ContactUpdateDto dto);
        Task<Contact?> SoftDeleteContactAsync(long id);


    }
}