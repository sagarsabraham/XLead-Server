// XLead_Server/Interfaces/ICompanyRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.DTOs; // For CompanyReadDto, CompanyCreateDto
using XLead_Server.Models; // For Company model

namespace XLead_Server.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer> AddCustomerAsync(CustomerCreateDto dto);
        Task<Customer?> GetByNameAsync(string customerName);
        Task<Dictionary<string, List<string>>> GetCustomerContactMapAsync();
        Task<IEnumerable<CustomerReadDto>> GetAllCustomersAsync();
        Task<Customer?> UpdateCustomerAsync(long id, CustomerUpdateDto dto);
        Task<bool> DeleteCustomerAsync(long id);
    }
}   