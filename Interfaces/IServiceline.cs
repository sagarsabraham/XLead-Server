using XLead_Server.Models;
using XLead_Server.Repositories;

namespace XLead_Server.Interfaces
{
    public interface IServiceline
    {
        Task<IEnumerable<ServiceLine>> GetServiceTypes();
    }
}
