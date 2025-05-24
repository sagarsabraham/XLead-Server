using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.DTOs;
using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IDealRepository
    {
        Task<Deal> AddDealAsync(Deal deal);
        Task<DealReadDto?> GetDealByIdAsync(long id);
        Task<IEnumerable<Deal>> GetAllDealsAsync();
    }
}
