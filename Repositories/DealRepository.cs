using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Controllers;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class DealRepository : IDealRepository
    {
        private readonly ApiDbContext _context;
        private readonly ICustomerRepository _customerRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DealsController> _logger;
        public DealRepository(
            ApiDbContext context,
            ICustomerRepository customerRepository,
            IContactRepository contactRepository,
            IMapper mapper,
            ILogger<DealsController> logger)
        {
            _context = context;
            _customerRepository = customerRepository;
            _contactRepository = contactRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DealReadDto?> AddDealAsync(DealCreateDto dto)
        {
         
            if (dto.RegionId.HasValue && !await _context.Regions.AnyAsync(r => r.Id == dto.RegionId.Value))
                throw new InvalidOperationException($"Region with ID {dto.RegionId.Value} does not exist.");
            if (dto.DealStageId.HasValue && !await _context.DealStages.AnyAsync(ds => ds.Id == dto.DealStageId.Value))
                throw new InvalidOperationException($"Deal Stage with ID {dto.DealStageId.Value} does not exist.");
            if (dto.RevenueTypeId.HasValue && !await _context.RevenueTypes.AnyAsync(rt => rt.Id == dto.RevenueTypeId.Value))
                throw new InvalidOperationException($"Revenue Type with ID {dto.RevenueTypeId.Value} does not exist.");
            if (dto.DuId.HasValue && !await _context.DUs.AnyAsync(du => du.Id == dto.DuId.Value))
                throw new InvalidOperationException($"DU with ID {dto.DuId.Value} does not exist.");
            if (dto.CountryId.HasValue && !await _context.Countries.AnyAsync(c => c.Id == dto.CountryId.Value))
                throw new InvalidOperationException($"Country with ID {dto.CountryId.Value} does not exist.");

           
            Customer? customer = await _customerRepository.GetByNameAsync(dto.CustomerName);
            if (customer == null)
            {
                var customerCreateDto = new CustomerCreateDto
                {
                    CustomerName = dto.CustomerName,
                    Website = null,
                    CustomerPhoneNumber = null,
                    CreatedBy = dto.CreatedBy
                };
                customer = await _customerRepository.AddCustomerAsync(customerCreateDto);
                if (customer == null)
                {
                    throw new InvalidOperationException($"Failed to create customer: {dto.CustomerName}");
                }
            }

          
            string firstName;
            string? lastName = null;
            var nameParts = dto.ContactFullName.Trim().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            firstName = nameParts.Length > 0 ? nameParts[0] : "Unknown";
            if (nameParts.Length > 1)
            {
                lastName = nameParts[1];
            }

            Contact? contact = await _contactRepository.GetByFullNameAndCustomerIdAsync(firstName, lastName, customer.Id);

            if (contact == null)
            {
                var contactCreateDto = new ContactCreateDto
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = dto.ContactEmail,
                    PhoneNumber = dto.ContactPhoneNumber,
                    Designation = dto.ContactDesignation,
                    CustomerId = customer.Id,
                    CustomerName = customer.CustomerName,
                    CreatedBy = dto.CreatedBy
                };
                contact = await _contactRepository.AddContactAsync(contactCreateDto);
                if (contact == null)
                {
                    throw new InvalidOperationException($"Failed to create contact: {dto.ContactFullName}");
                }
            }
            else
            {
               
                contact.Email = dto.ContactEmail ?? contact.Email;
                contact.PhoneNumber = dto.ContactPhoneNumber ?? contact.PhoneNumber;
                contact.Designation = dto.ContactDesignation ?? contact.Designation;
                _context.Contacts.Update(contact);
                await _context.SaveChangesAsync();
            }

            if (contact.Id == 0)
            {
                throw new InvalidOperationException($"Contact '{dto.ContactFullName}' for customer '{customer.CustomerName}' has an invalid ID after creation/retrieval.");
            }

            var deal = _mapper.Map<Deal>(dto);
            deal.ContactId = contact.Id;

            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();

            return await GetDealByIdAsync(deal.Id);
        }

        public async Task<DealReadDto?> GetDealByIdAsync(long id)
        {
            var deal = await _context.Deals
                .AsNoTracking()
                .Include(d => d.account)
                .Include(d => d.region)
                .Include(d => d.domain)
                .Include(d => d.revenueType)
                .Include(d => d.du)
                .Include(d => d.country)
                .Include(d => d.contact)
                .Include(d => d.dealStage)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null) return null;

            return _mapper.Map<DealReadDto>(deal);
        }

        public async Task<IEnumerable<DealReadDto>> GetAllDealsAsync() 
        {
          
            return await _context.Deals
                .AsNoTracking()
                .Include(d => d.account)
                .Include(d => d.region)
                .Include(d => d.domain)
                .Include(d => d.revenueType)
                .Include(d => d.du)
                .Include(d => d.country)
                .Include(d => d.contact)
                .Include(d => d.dealStage)
                .OrderByDescending(d => d.CreatedAt)
                .Select(deal => _mapper.Map<DealReadDto>(deal))
                .ToListAsync();
        }
        public async Task<DealReadDto?> UpdateDealStageAsync(long id, DealUpdateDto dto) 
        {
            var deal = await _context.Deals
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
            {
                return null;
            }

            var stage = await _context.DealStages
                .FirstOrDefaultAsync(s => s.StageName == dto.StageName);

            if (stage == null)
            {
                
                throw new InvalidOperationException($"Stage '{dto.StageName}' not found.");
            }

            deal.DealStageId = stage.Id;
            deal.UpdatedAt = DateTime.UtcNow;
            deal.UpdatedBy = dto.PerformedByUserId; 

            await _context.SaveChangesAsync();

          
            return await GetDealByIdAsync(deal.Id);
        }
      
        public async Task<IEnumerable<DealReadDto>> GetDealsByCreatorIdAsync(long creatorId)
        {
            
            return await _context.Deals
                .Where(d => d.CreatedBy == creatorId) 
                .AsNoTracking()
                .Include(d => d.account)
                .Include(d => d.region)
                .Include(d => d.domain)
                .Include(d => d.revenueType)
                .Include(d => d.du)
                .Include(d => d.country)
                .Include(d => d.contact)
                .Include(d => d.dealStage)
                .OrderByDescending(d => d.CreatedAt) 
                .Select(deal => _mapper.Map<DealReadDto>(deal))
                .ToListAsync();
        }
        // In XLead_Server.Repositories.DealRepository.cs
        // (Ensure ApiDbContext _context, IMapper _mapper, and ILogger<DealRepository> _logger are injected)

        public async Task<IEnumerable<DealManagerOverviewDto>> GetDealsForManagerAsync(long managerUserId)
        {
          

            // Step 1: Find all User IDs that are assigned to (report to) the managerUserId
            var salespersonIds = await _context.Users
                .Where(u => u.AssignedTo == managerUserId) // Key assumption: User.AssignedTo is the manager's ID
                .Select(u => u.Id)
                .ToListAsync();

            if (!salespersonIds.Any())
            {
                _logger.LogInformation("Repository: No salespersons found reporting to manager ID {ManagerUserId}", managerUserId);
                return Enumerable.Empty<DealManagerOverviewDto>();
            }

            _logger.LogInformation("Repository: Salespersons for manager {ManagerUserId}: {SalespersonIds}", managerUserId, string.Join(",", salespersonIds));

            // Step 2: Fetch deals created by these salespersons
            var deals = await _context.Deals
                .Where(d => salespersonIds.Contains(d.CreatedBy)) // Filter deals by creators who are the identified salespersons
                .Include(d => d.Creator)     // To get User.Name for SalespersonName
                .Include(d => d.dealStage)   // For StageName
                .Include(d => d.account)
                .Include(d => d.region)
                .Include(d => d.du)
                .Include(d => d.contact)
                // Add other .Include() statements if needed for other fields in DealManagerOverviewDto
                .OrderByDescending(d => d.ClosingDate) // Example ordering
                .ToListAsync();

            _logger.LogInformation("Repository: Found {DealCount} deals for salespersons under manager {ManagerUserId}", deals.Count, managerUserId);

            // Step 3: Project to DealManagerOverviewDto
            // Using manual projection for clarity, especially for SalespersonName
            var overviewDeals = deals.Select(deal => new DealManagerOverviewDto
            {
                Id = deal.Id,
                DealName = deal.DealName,
                DealAmount = deal.DealAmount,
                StageName = deal.dealStage?.StageName,
                ClosingDate = deal.ClosingDate,
                SalespersonId = deal.CreatedBy,
                SalespersonName = deal.Creator?.Name ?? $"User ID {deal.CreatedBy}", // Use User.Name
                AccountName = deal.account?.AccountName,
                RegionName = deal.region?.RegionName,
                DUName = deal.du?.DUName,
                ContactName = deal.contact != null ? $"{deal.contact.FirstName} {deal.contact.LastName}".Trim() : null,
                StartingDate = deal.StartingDate
                // Map other necessary fields...
            }).ToList();

            return overviewDeals;
        }
        // In XLead_Server.Repositories.DealRepository.cs

        public async Task<IEnumerable<ManagerStageCountDto>> GetStageCountsForManagerAsync(long managerUserId)
        {
            _logger.LogInformation("Repository: Fetching stage counts for manager ID {ManagerUserId}", managerUserId);

            // Step 1: Find all User IDs that are assigned to the managerUserId
            var salespersonIds = await _context.Users
                .Where(u => u.AssignedTo == managerUserId)
                .Select(u => u.Id)
                .ToListAsync();

            if (!salespersonIds.Any())
            {
                _logger.LogInformation("Repository: No salespersons found for stage counts, manager ID {ManagerUserId}", managerUserId);
                return Enumerable.Empty<ManagerStageCountDto>();
            }

            // Step 2: Fetch deals created by these salespersons, group by stage, and count
            var stageCounts = await _context.Deals
                .Where(d => salespersonIds.Contains(d.CreatedBy))
                .Include(d => d.dealStage) // Essential for getting dealStage.StageName
                .GroupBy(d => d.dealStage.StageName) // Group by the StageName
                .Select(g => new ManagerStageCountDto
                {
                    StageName = g.Key ?? "Unassigned Stage", // Handle if StageName could be null (shouldn't if FK enforced)
                    DealCount = g.Count()
                    // TotalAmount = g.Sum(d => d.DealAmount) // Optional
                })
                .OrderBy(s => s.StageName) // Optional: for consistent ordering
                .ToListAsync();

            _logger.LogInformation("Repository: Calculated {Count} stage groups for manager ID {ManagerUserId}", stageCounts.Count, managerUserId);
            return stageCounts;
        }
    }
}