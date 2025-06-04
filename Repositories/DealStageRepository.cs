using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class DealStageRepository : IDealStageRepository
    {
        private ApiDbContext _context;
        public DealStageRepository(ApiDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<DealStage>> GetAllDealStages()
        {
            return await _context.DealStages.ToListAsync();
        }
    }
}