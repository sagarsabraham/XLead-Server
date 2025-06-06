using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Controllers;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Models;
using Microsoft.Extensions.Logging;

namespace XLead_Server.Repositories
{
    public class DealRepository : IDealRepository
    {
        private readonly ApiDbContext _context;
        private readonly ICustomerRepository _customerRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DealsController> _logger;
        private const string StageClosedWon = "Closed Won";
        private const string StageClosedLost = "Closed Lost";
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
        

        public async Task<IEnumerable<DealManagerOverviewDto>> GetDealsForManagerAsync(long managerUserId)
        {
          

    
            var salespersonIds = await _context.Users
                .Where(u => u.AssignedTo == managerUserId) 
                .Select(u => u.Id)
                .ToListAsync();

            if (!salespersonIds.Any())
            {
                _logger.LogInformation("Repository: No salespersons found reporting to manager ID {ManagerUserId}", managerUserId);
                return Enumerable.Empty<DealManagerOverviewDto>();
            }

            _logger.LogInformation("Repository: Salespersons for manager {ManagerUserId}: {SalespersonIds}", managerUserId, string.Join(",", salespersonIds));

          
            var deals = await _context.Deals
                .Where(d => salespersonIds.Contains(d.CreatedBy)) 
                .Include(d => d.Creator)     
                .Include(d => d.dealStage)   
                .Include(d => d.account)
                .Include(d => d.region)
                .Include(d => d.du)
                .Include(d => d.contact)
              
                .OrderByDescending(d => d.ClosingDate)
                .ToListAsync();

            _logger.LogInformation("Repository: Found {DealCount} deals for salespersons under manager {ManagerUserId}", deals.Count, managerUserId);

            
            // Using manual projection for clarity, especially for SalespersonName
            var overviewDeals = deals.Select(deal => new DealManagerOverviewDto
            {
                Id = deal.Id,
                DealName = deal.DealName,
                DealAmount = deal.DealAmount,
                StageName = deal.dealStage?.StageName,
                ClosingDate = deal.ClosingDate,
                SalespersonId = deal.CreatedBy,
                SalespersonName = deal.Creator?.Name ?? $"User ID {deal.CreatedBy}", 
                AccountName = deal.account?.AccountName,
                RegionName = deal.region?.RegionName,
                DUName = deal.du?.DUName,
                ContactName = deal.contact != null ? $"{deal.contact.FirstName} {deal.contact.LastName}".Trim() : null,
                StartingDate = deal.StartingDate
               
            }).ToList();

            return overviewDeals;
        }
      

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
                .Include(d => d.dealStage)
                .GroupBy(d => d.dealStage.StageName)
                .Select(g => new ManagerStageCountDto
                {
                    StageName = g.Key ?? "Unassigned Stage",
                    DealCount = g.Count()
                    
                })
                .OrderBy(s => s.StageName) 
                .ToListAsync();

            _logger.LogInformation("Repository: Calculated {Count} stage groups for manager ID {ManagerUserId}", stageCounts.Count, managerUserId);
            return stageCounts;
        }

        public async Task<IEnumerable<TopCustomerDto>> GetTopCustomersByRevenueAsync(long userId, int count)
        {
            _logger.LogInformation($"Fetching top {count} customers by revenue won for User ID: {userId}.");

            var userDealsQuery = _context.Deals.Where(d => d.CreatedBy == userId); 

            var topCustomersData = await userDealsQuery 
                           .Where(d => d.dealStage.StageName == StageClosedWon && d.contact != null && d.contact.customer != null)
                           .Include(d => d.contact)
                               .ThenInclude(c => c.customer)
                           .GroupBy(d => new { d.contact.customer.Id, d.contact.customer.CustomerName })
                           .Select(g => new TopCustomerDto
                           {
                               CustomerName = g.Key.CustomerName,
                               TotalRevenueWon = g.Sum(d => d.DealAmount)
                           })
                           .OrderByDescending(c => c.TotalRevenueWon)
                           .Take(count)
                           .ToListAsync();

            _logger.LogInformation($"Successfully fetched top customers data for User ID: {userId}. Count: {topCustomersData.Count}");
            return topCustomersData;
        }




        private double CalculatePercentageChange(double currentValue, double previousValue)
        {
            if (previousValue == 0)
            {
                if (currentValue == 0) return 0.0;
                return currentValue > 0 ? 100.0 : -100.0;
            }
            return Math.Round(((currentValue - previousValue) / previousValue) * 100.0, 2);
        }

        private double CalculatePercentageChange(decimal currentValue, decimal previousValue)
        {
            if (previousValue == 0m)
            {
                if (currentValue == 0m) return 0.0;
                return currentValue > 0m ? 100.0 : -100.0;
            }
            return Math.Round(((double)(currentValue - previousValue) / (double)previousValue) * 100.0, 2);
        }

        public async Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueWonAsync(long userId, int numberOfMonths)
        {
            _logger.LogInformation($"Fetching monthly revenue won for the last {numberOfMonths} months for User ID: {userId}.");

            var startDate = DateTime.UtcNow.AddMonths(-(numberOfMonths - 1));
            var firstDayOfStartDateMonth = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var userDealsQuery = _context.Deals.Where(d => d.CreatedBy == userId); 

            var monthlyRevenueData = await userDealsQuery 
                .Where(d => d.dealStage.StageName == StageClosedWon &&
                             d.ClosingDate >= firstDayOfStartDateMonth)
                .Include(d => d.dealStage) 
                .GroupBy(d => new { d.ClosingDate.Year, d.ClosingDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(d => d.DealAmount)
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month)
                .ToListAsync();

            var result = monthlyRevenueData.Select(r => new MonthlyRevenueDto
            {
                MonthYear = new DateTime(r.Year, r.Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                TotalRevenue = r.TotalRevenue
            }).ToList();

            _logger.LogInformation($"Successfully fetched {result.Count} months of revenue data for User ID: {userId}.");
            return result;
        }
        public async Task<IEnumerable<PipelineStageDataDto>> GetOpenPipelineAmountsByStageAsync(long userId)
        {
            _logger.LogInformation($"Fetching open pipeline amounts by stage for User ID: {userId}.");

            var userDealsQuery = _context.Deals.Where(d => d.CreatedBy == userId); 

            var openPipelineData = await userDealsQuery 
                .Include(d => d.dealStage)
                .Where(d => d.dealStage.StageName != StageClosedWon && d.dealStage.StageName != StageClosedLost)
                .GroupBy(d => d.dealStage.StageName)
                .Select(g => new PipelineStageDataDto
                {
                    StageName = g.Key,
                    TotalAmount = g.Sum(d => d.DealAmount)
                })
                .OrderBy(s => s.StageName)
                .ToListAsync();

            _logger.LogInformation($"Successfully fetched {openPipelineData.Count} stages with open pipeline amounts for User ID: {userId}.");
            return openPipelineData;
        }
        public async Task<DashboardMetricsDto> GetDashboardMetricsAsync(long userId)
        {
            _logger.LogInformation($"Calculating dashboard metrics for User ID: {userId}.");
            var metrics = new DashboardMetricsDto();
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var currentMonthEnd = currentMonthStart.AddMonths(1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);

           
            var userDealsQuery = _context.Deals.Where(d => d.CreatedBy == userId); 

           
            var dealProjections = await userDealsQuery 
                .Where(d => (d.CreatedAt >= previousMonthStart && d.CreatedAt < currentMonthEnd) ||
                             (d.ClosingDate >= previousMonthStart && d.ClosingDate < currentMonthEnd))
                .Include(d => d.dealStage)
                .Select(d => new
                {
                    d.Id,
                    d.CreatedAt,
                    d.ClosingDate,
                    d.DealAmount,
                    DealStageName = d.dealStage != null ? d.dealStage.StageName : null
                })
                .ToListAsync();

            _logger.LogInformation($"Fetched {dealProjections.Count} deal projections for User ID {userId} dashboard metrics.");

     
            var currentMonthOpenDealsCount = dealProjections
                .Count(d => d.CreatedAt >= currentMonthStart && d.CreatedAt < currentMonthEnd &&
                             d.DealStageName != StageClosedWon && d.DealStageName != StageClosedLost);
           
            var previousMonthOpenDealsCount = dealProjections
                .Count(d => d.CreatedAt >= previousMonthStart && d.CreatedAt < currentMonthStart && 
                             d.DealStageName != StageClosedWon && d.DealStageName != StageClosedLost);
            metrics.OpenPipelines.Value = currentMonthOpenDealsCount.ToString();
            metrics.OpenPipelines.PercentageChange = CalculatePercentageChange((double)currentMonthOpenDealsCount, (double)previousMonthOpenDealsCount);
            metrics.OpenPipelines.IsPositiveTrend = currentMonthOpenDealsCount >= previousMonthOpenDealsCount;

            var currentMonthWonDealsCount = dealProjections
                .Count(d => d.ClosingDate >= currentMonthStart && d.ClosingDate < currentMonthEnd &&
                             d.DealStageName == StageClosedWon);
            var previousMonthWonDealsCount = dealProjections
                .Count(d => d.ClosingDate >= previousMonthStart && d.ClosingDate < currentMonthStart && 
                             d.DealStageName == StageClosedWon);
            metrics.PipelinesWon.Value = currentMonthWonDealsCount.ToString();
            metrics.PipelinesWon.PercentageChange = CalculatePercentageChange((double)currentMonthWonDealsCount, (double)previousMonthWonDealsCount);
            metrics.PipelinesWon.IsPositiveTrend = currentMonthWonDealsCount >= previousMonthWonDealsCount;

            var currentMonthLostDealsCount = dealProjections
                .Count(d => d.ClosingDate >= currentMonthStart && d.ClosingDate < currentMonthEnd &&
                             d.DealStageName == StageClosedLost);
            var previousMonthLostDealsCount = dealProjections
                .Count(d => d.ClosingDate >= previousMonthStart && d.ClosingDate < currentMonthStart && 
                             d.DealStageName == StageClosedLost);
            metrics.PipelinesLost.Value = currentMonthLostDealsCount.ToString();
            metrics.PipelinesLost.PercentageChange = CalculatePercentageChange((double)currentMonthLostDealsCount, (double)previousMonthLostDealsCount);
            metrics.PipelinesLost.IsPositiveTrend = currentMonthLostDealsCount <= previousMonthLostDealsCount; 

            var currentMonthRevenueWonSum = dealProjections
                .Where(d => d.ClosingDate >= currentMonthStart && d.ClosingDate < currentMonthEnd &&
                             d.DealStageName == StageClosedWon)
                .Sum(d => d.DealAmount);
            var previousMonthRevenueWonSum = dealProjections
                .Where(d => d.ClosingDate >= previousMonthStart && d.ClosingDate < currentMonthStart && 
                             d.DealStageName == StageClosedWon)
                .Sum(d => d.DealAmount);
            metrics.RevenueWon.Value = currentMonthRevenueWonSum.ToString("C", CultureInfo.GetCultureInfo("en-US"));
            metrics.RevenueWon.PercentageChange = CalculatePercentageChange(currentMonthRevenueWonSum, previousMonthRevenueWonSum);
            metrics.RevenueWon.IsPositiveTrend = currentMonthRevenueWonSum >= previousMonthRevenueWonSum;


            _logger.LogInformation("Calculated dashboard metrics for User ID {UserId}: {@Metrics}", userId, metrics);
            return metrics;
        }


    }
}