using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class DuRepository : IDuRepository
    {
        private ApiDbContext _context;

        public DuRepository(ApiDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<DU>> GetDus()
        {
            return await _context.DUs.ToListAsync();
        }
    }
}
