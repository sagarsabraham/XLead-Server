using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface ICountryRepository
    {
        Task<IEnumerable<Country>> GetCountries();
    }
}
