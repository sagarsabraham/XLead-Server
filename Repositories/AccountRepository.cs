using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        private ApiDbContext _context;
        public AccountRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Account>> GetAllAccounts()
        {
            return await _context.Accounts.ToListAsync();
        }
    }
}
