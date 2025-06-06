using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class IndustryVerticalRepository:IIndustryVertical
    {
        private ApiDbContext _context;
        public IndustryVerticalRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<IndustrialVertical>> GetAllIndustryVertical()
        {
            return await _context.IndustrialVerticals.ToListAsync();
        }
    }
}
