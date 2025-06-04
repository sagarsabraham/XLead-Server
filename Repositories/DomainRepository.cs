using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class DomainRepository : IDomainRepository
    {
        private ApiDbContext _context;
        public DomainRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Domain>> GetAllDomains()
        {
            return await _context.Domains.ToListAsync();
        }
    }
}
