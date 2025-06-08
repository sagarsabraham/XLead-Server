// XLead_Server/Repositories/CustomerRepository.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;

        public CustomerRepository(ApiDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CustomerReadDto>> GetAllCustomersAsync()
        {
            var customers = await _context.Customers
                .Where(c => c.IsHidden == null || c.IsHidden == false) 
                .ToListAsync();
            return _mapper.Map<IEnumerable<CustomerReadDto>>(customers);
        }
        public async Task<Customer> AddCustomerAsync(CustomerCreateDto dto)
        {
            // Validate IndustryVerticalId if provided
            if (dto.IndustryVerticalId.HasValue)
            {
                var verticalExists = await _context.IndustrialVerticals
                    .AnyAsync(iv => iv.Id == dto.IndustryVerticalId.Value);
                if (!verticalExists)
                {
                    throw new ArgumentException("Invalid IndustryVerticalId");
                }
            }

            var customer = _mapper.Map<Customer>(dto);
            customer.IsActive = true;
            customer.CreatedBy = dto.CreatedBy;
            customer.UpdatedBy = dto.CreatedBy;
            customer.UpdatedAt = DateTime.UtcNow;

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer?> GetByNameAsync(string customerName)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.CustomerName == customerName);
        }

        

        public async Task<Dictionary<string, CustomerContactMapDto>> GetCustomerContactMapAsync()
        {
            var customers = await _context.Customers
                .Include(c => c.Contacts)
                .ToListAsync();

            return customers.ToDictionary(
                c => c.CustomerName,
                c => new CustomerContactMapDto
                {
                    IsActive = c.IsActive,
                    IsHidden = c.IsHidden,
                    Contacts = c.Contacts.Select(ct => $"{ct.FirstName} {ct.LastName}".Trim()).ToList()
                }
            );
        }





        public async Task<Customer?> UpdateCustomerAsync(long id, CustomerUpdateDto dto)
    {
       
        var existingCustomer = await _context.Customers
            .Include(c => c.Contacts)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (existingCustomer == null)
        {
            return null;
        }

        bool wasActive = existingCustomer.IsActive;

        _mapper.Map(dto, existingCustomer);

     
        existingCustomer.UpdatedAt = DateTime.UtcNow;
       

       
        if (wasActive != existingCustomer.IsActive)
        {
           
            bool newStatus = existingCustomer.IsActive;

            foreach (var contact in existingCustomer.Contacts)
            {
               
                contact.IsActive = newStatus;

                
                contact.UpdatedAt = DateTime.UtcNow;
               
            }
        }

       
        await _context.SaveChangesAsync();

        return existingCustomer;
    }

    public async Task<Customer?> SoftDeleteCustomerAsync(long id)
        {
         
            var customerToSoftDelete = await _context.Customers
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customerToSoftDelete == null)
            {
                return null; 
            }

           
            customerToSoftDelete.IsHidden = true;
            customerToSoftDelete.IsActive = false;
            customerToSoftDelete.UpdatedAt = DateTime.UtcNow;
           

        
            foreach (var contact in customerToSoftDelete.Contacts)
            {
                contact.IsHidden = true;
                contact.IsActive = false;
                contact.UpdatedAt = DateTime.UtcNow;
               
            }

          
            await _context.SaveChangesAsync();

            return customerToSoftDelete;
        }



    }
}