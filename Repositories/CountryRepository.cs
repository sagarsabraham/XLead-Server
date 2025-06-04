using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private ApiDbContext _context;

        public CountryRepository(ApiDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Country>> GetCountries()
        {
            return await _context.Countries.ToListAsync();
        }
    }
}
