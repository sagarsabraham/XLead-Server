using XLead_Server.DTOs;

namespace XLead_Server.Interfaces
{
    public interface IStageHistoryRepository
    {
        Task<StageHistoryReadDto> CreateStageHistoryAsync(StageHistoryCreateDto stageHistoryDto);
        Task<IEnumerable<StageHistoryReadDto>> GetStageHistoryByDealIdAsync(long dealId);
    }
}
