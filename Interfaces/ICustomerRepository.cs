using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.DTOs;
using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface ICustomerRepository
    {
        Task<Customer> AddCustomerAsync(CustomerCreateDto dto);
        Task<Customer?> GetByNameAsync(string customerName);
        Task<Dictionary<string, CustomerContactMapDto>> GetCustomerContactMapAsync();
        Task<IEnumerable<CustomerReadDto>> GetAllCustomersAsync();
        Task<Customer?> UpdateCustomerAsync(long id, CustomerUpdateDto dto);

        Task<Customer?> SoftDeleteCustomerAsync(long id);
    }
}