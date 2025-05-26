using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class RegionRepository : IRegionRepository
    {
        private ApiDbContext _context;
        public RegionRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Region>> GetAllRegions()
        {
            return await _context.Regions.ToListAsync();
        }
    }
}
