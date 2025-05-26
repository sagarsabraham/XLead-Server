using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IDuRepository
    {
        Task<IEnumerable<DU>> GetDus();
    }
}
