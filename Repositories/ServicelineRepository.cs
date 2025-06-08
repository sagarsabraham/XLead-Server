using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class ServicelineRepository:IServiceline
    {
        private ApiDbContext _context;

        public ServicelineRepository(ApiDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ServiceLine>> GetServiceTypes()
        {
            return await _context.ServiceLines.ToListAsync();
        }
    }
}
