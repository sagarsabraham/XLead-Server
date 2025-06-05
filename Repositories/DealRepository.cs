using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        private readonly ILogger<DealRepository> _logger;
        private const string StageClosedWon = "Closed Won";
        private const string StageClosedLost = "Closed Lost";

        public DealRepository(
            ApiDbContext context,
            ICustomerRepository customerRepository,
            IContactRepository contactRepository,
            IMapper mapper,
            ILogger<DealRepository> logger)
        {
            _context = context;
            _customerRepository = customerRepository;
            _contactRepository = contactRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DealReadDto?> AddDealAsync(DealCreateDto dto)
        {
            // Validate required foreign keys exist
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

            // 1. Find or Create Customer
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

            // 2. Find or Create Contact
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
                // Update existing contact with new details if provided
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

            // 3. Create Deal entity using AutoMapper
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
                .Select(deal => _mapper.Map<DealReadDto>(deal))
                .ToListAsync();
        }


        public async Task<IEnumerable<PipelineStageDataDto>> GetOpenPipelineAmountsByStageAsync()
        {
            _logger.LogInformation("Fetching open pipeline amounts by stage.");

            // Ensure your Deal model has a navigation property to DealStage (e.g., public DealStage DealStage { get; set; })
            // And DealStage has a name property (e.g., public string StageName { get; set; })
            // And Deal has an amount property (e.g., public decimal DealAmount { get; set; } or Amount)

            var openPipelineData = await _context.Deals
                .Include(d => d.dealStage) // Include DealStage to access its name
                .Where(d => d.dealStage.StageName != StageClosedWon && d.dealStage.StageName != StageClosedLost) // Filter out closed deals
                .GroupBy(d => d.dealStage.StageName) // Group by stage name
                .Select(g => new PipelineStageDataDto
                {
                    StageName = g.Key,         // The stage name
                    TotalAmount = g.Sum(d => d.DealAmount) // Sum of DealAmount for that stage
                                                           // Ensure DealAmount is the correct property name for deal's value in your Deal model
                })
                .OrderBy(s => s.StageName) // Optional: Order by stage name
                .ToListAsync();

            _logger.LogInformation($"Successfully fetched {openPipelineData.Count} stages with open pipeline amounts.");
            return openPipelineData;
        }




        public async Task<DashboardMetricsDto> GetDashboardMetricsAsync()
        {
            var metrics = new DashboardMetricsDto();
            var now = DateTime.UtcNow;

            var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var currentMonthEnd = currentMonthStart.AddMonths(1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);
            var previousMonthEnd = currentMonthStart;

           
            var dealProjections = await _context.Deals
                .Where(d => (d.CreatedAt >= previousMonthStart && d.CreatedAt < currentMonthEnd) || 
                             (d.ClosingDate >= previousMonthStart && d.ClosingDate < currentMonthEnd))
                .Select(d => new
                {
                    d.Id,
                    d.CreatedAt,      
                    d.ClosingDate,    
                    d.DealAmount,         
                    DealStageName = d.dealStage != null ? d.dealStage.StageName : null 
                })
                .ToListAsync();

            _logger.LogInformation($"Fetched {dealProjections.Count} deal projections for dashboard metrics (CreatedAt for Open, ClosingDate for Closed).");

         

            // 1. Open Pipelines - This Month:
          
            var currentMonthOpenDealsCount = dealProjections
                .Count(d => d.CreatedAt >= currentMonthStart && d.CreatedAt < currentMonthEnd &&
                             d.DealStageName != StageClosedWon && d.DealStageName != StageClosedLost);

        
            var previousMonthOpenDealsCount = dealProjections
                .Count(d => d.CreatedAt >= previousMonthStart && d.CreatedAt < previousMonthEnd &&
                             d.DealStageName != StageClosedWon && d.DealStageName != StageClosedLost);

            metrics.OpenPipelines.Value = currentMonthOpenDealsCount.ToString();
            metrics.OpenPipelines.PercentageChange = CalculatePercentageChange((double)currentMonthOpenDealsCount, (double)previousMonthOpenDealsCount);
            metrics.OpenPipelines.IsPositiveTrend = currentMonthOpenDealsCount >= previousMonthOpenDealsCount;

            // 2. Pipelines Won - This Month (ClosingDate in current month AND stage is Closed Won)
            var currentMonthWonDealsCount = dealProjections
                .Count(d => d.ClosingDate >= currentMonthStart && d.ClosingDate < currentMonthEnd &&
                             d.DealStageName == StageClosedWon);
            var previousMonthWonDealsCount = dealProjections
                .Count(d => d.ClosingDate >= previousMonthStart && d.ClosingDate < previousMonthEnd &&
                             d.DealStageName == StageClosedWon);

            metrics.PipelinesWon.Value = currentMonthWonDealsCount.ToString();
            metrics.PipelinesWon.PercentageChange = CalculatePercentageChange((double)currentMonthWonDealsCount, (double)previousMonthWonDealsCount);
            metrics.PipelinesWon.IsPositiveTrend = currentMonthWonDealsCount >= previousMonthWonDealsCount;

            // 3. Pipelines Lost - This Month (ClosingDate in current month AND stage is Closed Lost)
            var currentMonthLostDealsCount = dealProjections
                .Count(d => d.ClosingDate >= currentMonthStart && d.ClosingDate < currentMonthEnd &&
                             d.DealStageName == StageClosedLost);
            var previousMonthLostDealsCount = dealProjections
                .Count(d => d.ClosingDate >= previousMonthStart && d.ClosingDate < previousMonthEnd &&
                             d.DealStageName == StageClosedLost);

            metrics.PipelinesLost.Value = currentMonthLostDealsCount.ToString();
            metrics.PipelinesLost.PercentageChange = CalculatePercentageChange((double)currentMonthLostDealsCount, (double)previousMonthLostDealsCount);
            metrics.PipelinesLost.IsPositiveTrend = currentMonthLostDealsCount <= previousMonthLostDealsCount;

            // 4. Revenue Won - This Month (Sum Amount for deals ClosingDate in current month AND stage is Closed Won)
            var currentMonthRevenueWonSum = dealProjections
                .Where(d => d.ClosingDate >= currentMonthStart && d.ClosingDate < currentMonthEnd &&
                             d.DealStageName == StageClosedWon)
                .Sum(d => d.DealAmount);
            var previousMonthRevenueWonSum = dealProjections
                .Where(d => d.ClosingDate >= previousMonthStart && d.ClosingDate < previousMonthEnd &&
                             d.DealStageName == StageClosedWon)
                .Sum(d => d.DealAmount);

            metrics.RevenueWon.Value = currentMonthRevenueWonSum.ToString("C", CultureInfo.GetCultureInfo("en-US"));
            metrics.RevenueWon.PercentageChange = CalculatePercentageChange(currentMonthRevenueWonSum, previousMonthRevenueWonSum);
            metrics.RevenueWon.IsPositiveTrend = currentMonthRevenueWonSum >= previousMonthRevenueWonSum;

            _logger.LogInformation("Calculated dashboard metrics: {@Metrics}", metrics);
            return metrics;
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

        public async Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueWonAsync(int numberOfMonths)
        {
            _logger.LogInformation($"Fetching monthly revenue won for the last {numberOfMonths} months.");

        
            var startDate = DateTime.UtcNow.AddMonths(-(numberOfMonths - 1)); 
            var firstDayOfStartDateMonth = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var monthlyRevenueData = await _context.Deals
                .Where(d => d.dealStage.StageName == StageClosedWon && // Only "Closed Won" deals
                             d.ClosingDate >= firstDayOfStartDateMonth) // Within the specified date range
                .GroupBy(d => new { d.ClosingDate.Year, d.ClosingDate.Month }) // Group by Year and Month of ClosingDate
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(d => d.DealAmount) // Sum of DealAmount
                                                            // Ensure DealAmount is the correct property name in your Deal model
                })
                .OrderBy(r => r.Year)
                .ThenBy(r => r.Month)
                .ToListAsync(); // Fetch data from DB

           
            var result = monthlyRevenueData.Select(r => new MonthlyRevenueDto
            {
                
                MonthYear = new DateTime(r.Year, r.Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                TotalRevenue = r.TotalRevenue
            }).ToList();

           

            _logger.LogInformation($"Successfully fetched {result.Count} months of revenue data.");
            return result;
        }


    }
}