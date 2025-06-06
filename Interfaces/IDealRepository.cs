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
    }
}