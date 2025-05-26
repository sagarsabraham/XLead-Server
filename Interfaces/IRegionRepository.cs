using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IRegionRepository
    {
        Task<IEnumerable<Region>> GetAllRegions();

    }
}
