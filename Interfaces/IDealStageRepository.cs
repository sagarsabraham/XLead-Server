using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IDealStageRepository
    {
        Task<IEnumerable<DealStage>> GetAllDealStages();
    }
}
