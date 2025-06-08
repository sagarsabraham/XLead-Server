
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
        private readonly ILogger<CustomerRepository> _logger;
        private readonly IUserPrivilegeRepository _userPrivilegeRepository;
        public CustomerRepository(ApiDbContext context, IMapper mapper, ILogger<CustomerRepository> logger, IUserPrivilegeRepository userPrivilegeRepository)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _userPrivilegeRepository = userPrivilegeRepository;
        }
        private async Task<List<long>> GetRelevantCreatorIdsAsync(long requestingUserId, string overviewPrivilegeName)
        {
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(requestingUserId);
            bool hasOverviewPrivilege = privileges?.Any(p => p.PrivilegeName == overviewPrivilegeName) ?? false;

            if (hasOverviewPrivilege)
            {
                _logger.LogInformation("User {RequestingUserId} has '{OverviewPrivilegeName}'. Fetching data for subordinates.", requestingUserId, overviewPrivilegeName);
                var subordinateIds = await _context.Users
                    .Where(u => u.AssignedTo == requestingUserId)
                    .Select(u => u.Id)
                    .ToListAsync();

                
                return subordinateIds; 
            }
            else
            {
                _logger.LogInformation("User {RequestingUserId} does not have '{OverviewPrivilegeName}'. Fetching own data.", requestingUserId, overviewPrivilegeName);
                return new List<long> { requestingUserId };
            }
        }
       
        public async Task<IEnumerable<CustomerReadDto>> GetAllCustomersAsync(long requestingUserId)
        {
            
            var relevantCreatorIds = await GetRelevantCreatorIdsAsync(requestingUserId, "ViewTeamCustomers"); 

            if (!relevantCreatorIds.Any())
            {
                _logger.LogInformation("No relevant creator IDs for customers for user {RequestingUserId}.", requestingUserId);
                return Enumerable.Empty<CustomerReadDto>();
            }

            _logger.LogInformation("Fetching customers created by User IDs: [{RelevantCreatorIds}] for Requesting User: {RequestingUserId}", string.Join(",", relevantCreatorIds), requestingUserId);
            var customers = await _context.Customers
                .Where(c => (c.IsHidden == null || c.IsHidden == false) &&
                             relevantCreatorIds.Contains(c.CreatedBy))
                .ToListAsync();
            return _mapper.Map<IEnumerable<CustomerReadDto>>(customers);
        }
        public async Task<Customer> AddCustomerAsync(CustomerCreateDto dto)
        {
         
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





     public async Task<Customer?> GetCustomerByIdAsync(long id) 
{
    return await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
}

public async Task<Customer?> UpdateCustomerAsync(long id, CustomerUpdateDto dto)
{
    var existingCustomer = await _context.Customers
        .Include(c => c.Contacts) 
        .FirstOrDefaultAsync(c => c.Id == id);

    if (existingCustomer == null) return null;

    bool wasActive = existingCustomer.IsActive;
    _mapper.Map(dto, existingCustomer); 

    existingCustomer.UpdatedBy = dto.UpdatedBy; 
    existingCustomer.UpdatedAt = DateTime.UtcNow;

  
    await _context.SaveChangesAsync();
    return existingCustomer;
}

public async Task<Customer?> SoftDeleteCustomerAsync(long id, long performingUserId) 
{
    var customerToSoftDelete = await _context.Customers
        .Include(c => c.Contacts)
        .FirstOrDefaultAsync(c => c.Id == id);
    if (customerToSoftDelete == null) return null;

    customerToSoftDelete.IsHidden = true;
    customerToSoftDelete.IsActive = false;
    customerToSoftDelete.UpdatedBy = performingUserId; 
    customerToSoftDelete.UpdatedAt = DateTime.UtcNow;

    foreach (var contact in customerToSoftDelete.Contacts)
    {
        contact.IsHidden = true;
        contact.IsActive = false;
        contact.UpdatedBy = performingUserId; 
        contact.UpdatedAt = DateTime.UtcNow;
    }
    await _context.SaveChangesAsync();
    return customerToSoftDelete;
}


    }
}