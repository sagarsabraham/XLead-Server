// Interfaces/IAccountRepository.cs
using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IAccountRepository
    {
        Task<IEnumerable<Account>> GetAllAccounts();

    }
}
