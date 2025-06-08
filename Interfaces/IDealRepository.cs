using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.DTOs;
using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IDealRepository
    {
        Task<DealReadDto?> AddDealAsync(DealCreateDto dealCreateDto);
        Task<DealReadDto?> GetDealByIdAsync(long id);
        Task<IEnumerable<DealReadDto>> GetAllDealsAsync();
        Task<DealReadDto> UpdateDealStageAsync(long id, DealUpdateDto dealUpdateDto);
        Task<IEnumerable<DealReadDto>> GetDealsByCreatorIdAsync(long creatorId);
      
        Task<IEnumerable<DealManagerOverviewDto>> GetDealsForManagerAsync(long managerUserId);
        Task<IEnumerable<ManagerStageCountDto>> GetStageCountsForManagerAsync(long managerUserId);
        Task<DashboardMetricsDto> GetDashboardMetricsAsync(long requestingUserId);
        Task<IEnumerable<PipelineStageDataDto>> GetOpenPipelineAmountsByStageAsync(long requestingUserId);
        Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueWonAsync(long requestingUserId, int numberOfMonths);
        Task<IEnumerable<TopCustomerDto>> GetTopCustomersByRevenueAsync(long requestingUserId, int count);
        Task<DealReadDto?> UpdateDealAsync(long id, DealEditDto dto);
        Task<IEnumerable<StageHistoryDto>> GetDealStageHistoryAsync(long dealId);
        Task<DealReadDto?> UpdateDealDescriptionAsync(long id, DealDescriptionUpdateDto dto);

    }
}