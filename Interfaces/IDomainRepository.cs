using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IDomainRepository
    {
        Task<IEnumerable<Domain>> GetAllDomains();

    }
}
