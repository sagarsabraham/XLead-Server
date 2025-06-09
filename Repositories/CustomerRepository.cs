using AutoMapper;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Helpers;
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
        private async Task ValidateGlobalUniqueness(CustomerCreateDto dto, long? customerIdToExclude = null)
        {
            
            if (!string.IsNullOrWhiteSpace(dto.CustomerPhoneNumber))
            {
                var normalizedPhone = NormalizationHelper.NormalizePhoneNumber(dto.CustomerPhoneNumber);

                var customerPhoneQuery = _context.Customers.AsQueryable();
                if (customerIdToExclude.HasValue) customerPhoneQuery = customerPhoneQuery.Where(c => c.Id != customerIdToExclude.Value);
                if (await customerPhoneQuery.AnyAsync(c => EF.Functions.Like(c.CustomerPhoneNumber.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", ""), $"%{normalizedPhone}%"))) // Example of in-DB normalization
                {
                    throw new ArgumentException($"The phone number '{dto.CustomerPhoneNumber}' is already in use by another customer.");
                }

                
                if (await _context.Contacts.AnyAsync(c => EF.Functions.Like(c.PhoneNumber.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", ""), $"%{normalizedPhone}%")))
                {
                    throw new ArgumentException($"The phone number '{dto.CustomerPhoneNumber}' is already in use by a contact.");
                }
            }

            
            if (!string.IsNullOrWhiteSpace(dto.Website))
            {
                var normalizedWebsite = NormalizationHelper.NormalizeWebsite(dto.Website);
                var customerWebsiteQuery = _context.Customers.AsQueryable();
                if (customerIdToExclude.HasValue) customerWebsiteQuery = customerWebsiteQuery.Where(c => c.Id != customerIdToExclude.Value);
                if (await customerWebsiteQuery.AnyAsync(c => c.Website != null && c.Website.ToLower() == normalizedWebsite))
                {
                    throw new ArgumentException($"The website '{dto.Website}' is already in use by another customer.");
                }
            }
        }
        public async Task<IEnumerable<CustomerReadDto>> GetAllCustomersAsync()
        {
            
            var customers = await _context.Customers
                .Where(c => c.IsHidden != true)
                .ToListAsync();
            return _mapper.Map<IEnumerable<CustomerReadDto>>(customers);
        }

        public async Task<Customer> AddCustomerAsync(CustomerCreateDto dto)
        {
            var normalizedName = dto.CustomerName.Trim().ToUpper();
            if (await _context.Customers.AnyAsync(c => c.CustomerName.ToUpper() == normalizedName))
            {
                throw new ArgumentException($"A customer with the name '{dto.CustomerName}' already exists.");
            }
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
            await ValidateGlobalUniqueness(dto);
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
            var normalizedName = dto.CustomerName.Trim().ToUpper();
            if (await _context.Customers.AnyAsync(c => c.Id != id && c.CustomerName.ToUpper() == normalizedName))
            {
                throw new ArgumentException($"Another customer with the name '{dto.CustomerName}' already exists.");

            }
            var checkDto = new CustomerCreateDto // Adapt to the validation method's signature
            {
                CustomerPhoneNumber = dto.PhoneNo,
                Website = dto.Website
            };
            await ValidateGlobalUniqueness(checkDto, id);
            bool wasActive = existingCustomer.IsActive;
            existingCustomer.CustomerName = dto.CustomerName;
            existingCustomer.CustomerPhoneNumber = dto.PhoneNo;
            existingCustomer.Website = dto.Website;
            existingCustomer.IndustryVerticalId = dto.IndustryVerticalId;
            existingCustomer.IsActive = dto.IsActive;
            existingCustomer.UpdatedAt = DateTime.UtcNow;
            existingCustomer.UpdatedBy = dto.UpdatedBy;
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