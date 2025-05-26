using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class RevenueTypeRepository : IRevenueType
    {
        private ApiDbContext _context;

        public RevenueTypeRepository(ApiDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<RevenueType>> GetRevenueTypes()
        {
            return await _context.RevenueTypes.ToListAsync();
        }
    }
}
