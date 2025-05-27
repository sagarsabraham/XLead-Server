// XLead_Server/Interfaces/ICompanyRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.DTOs; // For CompanyReadDto, CompanyCreateDto
using XLead_Server.Models; // For Company model

namespace XLead_Server.Interfaces
{
    public interface ICompanyRepository
    {
        Task<Company> AddCompanyAsync(CompanyCreateDto dto);
        Task<Company?> GetByNameAsync(string companyName);
        Task<Dictionary<string, List<string>>> GetCompanyContactMapAsync();
        Task<IEnumerable<CompanyReadDto>> GetAllCompaniesAsync();
    }
}   