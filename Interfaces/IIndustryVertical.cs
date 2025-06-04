using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IIndustryVertical
    {
        Task<IEnumerable<IndustrialVertical>> GetAllIndustryVertical();
    }
}
