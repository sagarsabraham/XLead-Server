using XLead_Server.DTOs;
using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface ICompanyRepository
    {
        Task<Company> AddCompanyAsync(CompanyCreateDto dto);
        Task<Dictionary<string, List<string>>> GetCompanyContactMapAsync();
        Task<IEnumerable<CompanyReadDto>> GetAllCompaniesAsync();
    }
}
