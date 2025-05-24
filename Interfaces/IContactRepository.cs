using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IContactRepository
    {
        Task<IEnumerable<Contact>> GetAllContacts();
    }
}
