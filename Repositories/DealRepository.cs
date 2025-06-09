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
        private readonly IUserPrivilegeRepository _userPrivilegeRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IContactRepository _contactRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DealRepository> _logger;
        private const string StageClosedWon = "Closed Won";
        private const string StageClosedLost = "Closed Lost";

        public DealRepository(
            ApiDbContext context,
            IUserPrivilegeRepository userPrivilegeRepository,
            ICustomerRepository customerRepository,
            IContactRepository contactRepository,
            IMapper mapper,
            ILogger<DealRepository> logger)
        {
            _context = context;
            _userPrivilegeRepository = userPrivilegeRepository;
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

        public async Task<IEnumerable<DealReadDto>> GetDealsByCreatorIdAsync(long creatorId)
        {
            return await _context.Deals
                .Where(d => d.CreatedBy == creatorId) // Filter by the CreatedBy field
                .AsNoTracking()
                .Include(d => d.account)
                .Include(d => d.region)
                .Include(d => d.domain)
                .Include(d => d.revenueType)
                .Include(d => d.du)
                .Include(d => d.country)
                .Include(d => d.contact)
                .Include(d => d.dealStage)
                .OrderByDescending(d => d.CreatedAt) // Optional: order by creation date or other criteria
                .Select(deal => _mapper.Map<DealReadDto>(deal))
                .ToListAsync();
        }

        public async Task<DealReadDto?> UpdateDealStageAsync(long id, DealUpdateDto dto)
        {
            // 1. Find the deal by ID
            var deal = await _context.Deals
                .Include(d => d.dealStage)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
            {
                _logger.LogWarning("Deal with ID {DealId} not found in UpdateDealStageAsync.", id);
                return null; // Deal not found
            }

            // 2. Find the stage by StageName
            var newStage = await _context.DealStages
                .FirstOrDefaultAsync(s => s.StageName == dto.StageName);

            if (newStage == null)
            {
                _logger.LogWarning("Stage '{StageName}' not found for deal ID {DealId}.", dto.StageName, id);
                throw new InvalidOperationException($"Stage '{dto.StageName}' not found.");
            }

            // 3. Check if stage is actually changing
            if (deal.DealStageId != newStage.Id)
            {
                // 4. Create stage history record
                var stageHistory = new StageHistory
                {
                    DealId = deal.Id,
                    DealStageId = newStage.Id,
                    CreatedBy = dto.UpdatedBy ?? 1, // You should pass the current user ID
                    CreatedAt = DateTime.UtcNow
                };
                _context.StageHistories.Add(stageHistory);

                // 5. Update the deal's stage
                deal.DealStageId = newStage.Id;
                deal.UpdatedAt = DateTime.UtcNow;
                deal.UpdatedBy = dto.UpdatedBy;
            }

            // 6. Save changes to the database
            await _context.SaveChangesAsync();

            // 7. Retrieve the updated deal with all related data
            return await GetDealByIdAsync(deal.Id);
        }

        public async Task<IEnumerable<PipelineStageDataDto>> GetOpenPipelineAmountsByStageAsync(long requestingUserId)
        {
            var relevantCreatorIds = await GetRelevantDealCreatorIdsAsync(requestingUserId);
            if (!relevantCreatorIds.Any()) return Enumerable.Empty<PipelineStageDataDto>();

            _logger.LogInformation($"Fetching open pipeline amounts by stage for relevant creator IDs: [{string.Join(",", relevantCreatorIds)}].");

            var openPipelineData = await _context.Deals
                .Where(d => relevantCreatorIds.Contains(d.CreatedBy))
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
            return openPipelineData;
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

        public async Task<IEnumerable<MonthlyRevenueDto>> GetMonthlyRevenueWonAsync(long requestingUserId, int numberOfMonths)
        {
            var relevantCreatorIds = await GetRelevantDealCreatorIdsAsync(requestingUserId);

            if (!relevantCreatorIds.Any()) return Enumerable.Empty<MonthlyRevenueDto>();

            _logger.LogInformation($"Fetching monthly revenue won for relevant creator IDs: [{string.Join(",", relevantCreatorIds)}] for {numberOfMonths} months.");

            var startDate = DateTime.UtcNow.AddMonths(-(numberOfMonths - 1));

            var firstDayOfStartDateMonth = new DateTime(startDate.Year, startDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var monthlyRevenueData = await _context.Deals
                .Where(d => relevantCreatorIds.Contains(d.CreatedBy))
                .Where(d => d.dealStage.StageName == StageClosedWon && d.ClosingDate >= firstDayOfStartDateMonth)
                .Include(d => d.dealStage)
                .GroupBy(d => new { d.ClosingDate.Year, d.ClosingDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(d => d.DealAmount)
                })
                .OrderBy(r => r.Year).ThenBy(r => r.Month)
                .ToListAsync();

            return monthlyRevenueData.Select(r => new MonthlyRevenueDto
            {
                MonthYear = new DateTime(r.Year, r.Month, 1).ToString("MMMM yyyy", CultureInfo.InvariantCulture),
                TotalRevenue = r.TotalRevenue
            }).ToList();
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

            var salespersonIds = await _context.Users
                .Where(u => u.AssignedTo == managerUserId)
                .Select(u => u.Id)
                .ToListAsync();

            if (!salespersonIds.Any())
            {
                _logger.LogInformation("Repository: No salespersons found for stage counts, manager ID {ManagerUserId}", managerUserId);
                return Enumerable.Empty<ManagerStageCountDto>();
            }

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

        private async Task<List<long>> GetRelevantDealCreatorIdsAsync(long requestingUserId)
        {
            var privileges = await _userPrivilegeRepository.GetPrivilegesByUserIdAsync(requestingUserId);
            bool hasOverviewPrivilege = privileges?.Any(p => p.PrivilegeName == "Dashboard Overview") ?? false;

            if (hasOverviewPrivilege)
            {
                _logger.LogInformation($"User {requestingUserId} has 'Dashboard Overview' privilege. Fetching subordinate IDs.");
                var subordinateIds = await _context.Users
                    .Where(u => u.AssignedTo == requestingUserId)
                    .Select(u => u.Id)
                    .ToListAsync();

                if (!subordinateIds.Any())
                {
                    _logger.LogInformation($"Manager {requestingUserId} has no subordinates assigned directly via 'AssignedTo'.");
                }
                var relevantIds = new List<long>(subordinateIds);
                relevantIds.Add(requestingUserId);
                return relevantIds;
            }
            else
            {
                _logger.LogInformation($"User {requestingUserId} does not have 'Dashboard Overview'. Fetching their own deals.");
                return new List<long> { requestingUserId };
            }
        }

        public async Task<IEnumerable<TopCustomerDto>> GetTopCustomersByRevenueAsync(long requestingUserId, int count)
        {
            var relevantCreatorIds = await GetRelevantDealCreatorIdsAsync(requestingUserId);
            if (!relevantCreatorIds.Any()) return Enumerable.Empty<TopCustomerDto>();

            _logger.LogInformation($"Fetching top {count} customers by revenue for relevant creator IDs: [{string.Join(",", relevantCreatorIds)}].");

            var topCustomersData = await _context.Deals
                           .Where(d => relevantCreatorIds.Contains(d.CreatedBy))
                           .Where(d => d.dealStage.StageName == StageClosedWon && d.contact != null && d.contact.customer != null)
                           .Include(d => d.contact).ThenInclude(c => c.customer)
                           .Include(d => d.dealStage)
                           .GroupBy(d => new { d.contact.customer.Id, d.contact.customer.CustomerName })
                           .Select(g => new TopCustomerDto
                           {
                               CustomerName = g.Key.CustomerName,
                               TotalRevenueWon = g.Sum(d => d.DealAmount)
                           })
                           .OrderByDescending(c => c.TotalRevenueWon)
                           .Take(count)
                           .ToListAsync();

            return topCustomersData;
        }

        public async Task<DashboardMetricsDto> GetDashboardMetricsAsync(long requestingUserId)
        {
            var relevantCreatorIds = await GetRelevantDealCreatorIdsAsync(requestingUserId);

            if (!relevantCreatorIds.Any())
            {
                _logger.LogWarning($"No relevant creator IDs found for GetDashboardMetricsAsync (Requesting User: {requestingUserId}). Returning empty metrics.");
                return new DashboardMetricsDto
                {
                    OpenPipelines = new DashboardMetricItemDto { Value = "0", PercentageChange = 0, IsPositiveTrend = true },
                    PipelinesWon = new DashboardMetricItemDto { Value = "0", PercentageChange = 0, IsPositiveTrend = true },
                    PipelinesLost = new DashboardMetricItemDto { Value = "0", PercentageChange = 0, IsPositiveTrend = true },
                    RevenueWon = new DashboardMetricItemDto { Value = "$0.00", PercentageChange = 0, IsPositiveTrend = true }
                };
            }

            _logger.LogInformation($"Calculating dashboard metrics for relevant creator IDs: [{string.Join(",", relevantCreatorIds)}].");

            var metrics = new DashboardMetricsDto();
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var currentMonthEnd = currentMonthStart.AddMonths(1);
            var previousMonthStart = currentMonthStart.AddMonths(-1);

            var dealProjections = await _context.Deals
                .Where(d => relevantCreatorIds.Contains(d.CreatedBy))
                .Where(d => (d.StartingDate >= previousMonthStart && d.StartingDate < currentMonthEnd) ||
                             (d.ClosingDate >= previousMonthStart && d.ClosingDate < currentMonthEnd))
                .Include(d => d.dealStage)
                .Select(d => new
                {
                    d.Id,
                    d.StartingDate,
                    d.ClosingDate,
                    d.DealAmount,
                    DealStageName = d.dealStage != null ? d.dealStage.StageName : null
                })
                .ToListAsync();

            var currentMonthOpenDealsCount = dealProjections
                .Count(d => d.StartingDate >= currentMonthStart && d.StartingDate < currentMonthEnd &&
                                d.DealStageName != StageClosedWon && d.DealStageName != StageClosedLost);
            var previousMonthOpenDealsCount = dealProjections
                .Count(d => d.StartingDate >= previousMonthStart && d.StartingDate < currentMonthStart &&
                                d.DealStageName != StageClosedWon && d.DealStageName != StageClosedLost);
            metrics.OpenPipelines = new DashboardMetricItemDto
            {
                Value = currentMonthOpenDealsCount.ToString(),
                PercentageChange = CalculatePercentageChange((double)currentMonthOpenDealsCount, (double)previousMonthOpenDealsCount),
                IsPositiveTrend = currentMonthOpenDealsCount >= previousMonthOpenDealsCount
            };

            var currentMonthWonDealsCount = dealProjections
                .Count(d => d.ClosingDate >= currentMonthStart && d.ClosingDate < currentMonthEnd &&
                                d.DealStageName == StageClosedWon);
            var previousMonthWonDealsCount = dealProjections
                .Count(d => d.ClosingDate >= previousMonthStart && d.ClosingDate < currentMonthStart &&
                                d.DealStageName == StageClosedWon);
            metrics.PipelinesWon = new DashboardMetricItemDto
            {
                Value = currentMonthWonDealsCount.ToString(),
                PercentageChange = CalculatePercentageChange((double)currentMonthWonDealsCount, (double)previousMonthWonDealsCount),
                IsPositiveTrend = currentMonthWonDealsCount >= previousMonthWonDealsCount
            };

            var currentMonthLostDealsCount = dealProjections
                .Count(d => d.ClosingDate >= currentMonthStart && d.ClosingDate < currentMonthEnd &&
                                d.DealStageName == StageClosedLost);
            var previousMonthLostDealsCount = dealProjections
                .Count(d => d.ClosingDate >= previousMonthStart && d.ClosingDate < currentMonthStart &&
                                d.DealStageName == StageClosedLost);
            metrics.PipelinesLost = new DashboardMetricItemDto
            {
                Value = currentMonthLostDealsCount.ToString(),
                PercentageChange = CalculatePercentageChange((double)currentMonthLostDealsCount, (double)previousMonthLostDealsCount),
                IsPositiveTrend = currentMonthLostDealsCount <= previousMonthLostDealsCount
            };

            var currentMonthRevenueWonSum = dealProjections
                .Where(d => d.ClosingDate >= currentMonthStart && d.ClosingDate < currentMonthEnd &&
                                d.DealStageName == StageClosedWon)
                .Sum(d => d.DealAmount);
            var previousMonthRevenueWonSum = dealProjections
                .Where(d => d.ClosingDate >= previousMonthStart && d.ClosingDate < currentMonthStart &&
                                d.DealStageName == StageClosedWon)
                .Sum(d => d.DealAmount);
            metrics.RevenueWon = new DashboardMetricItemDto
            {
                Value = currentMonthRevenueWonSum.ToString("C", CultureInfo.GetCultureInfo("en-US")),
                PercentageChange = CalculatePercentageChange(currentMonthRevenueWonSum, previousMonthRevenueWonSum),
                IsPositiveTrend = currentMonthRevenueWonSum >= previousMonthRevenueWonSum
            };

            return metrics;
        }

        public async Task<IEnumerable<StageHistoryDto>> GetDealStageHistoryAsync(long dealId)
        {
            var history = await _context.StageHistories
                .Where(sh => sh.DealId == dealId)
                .Include(sh => sh.DealStage)
                .Include(sh => sh.Creator)
                .OrderByDescending(sh => sh.CreatedAt)
                .Select(sh => new StageHistoryDto
                {
                    Id = sh.Id,
                    DealId = sh.DealId,
                    StageName = sh.DealStage.StageName,
                    StageDisplayName = sh.DealStage.DisplayName,
                    CreatedBy = sh.Creator != null ? sh.Creator.Name : "Unknown User",
                    CreatedAt = sh.CreatedAt
                })
                .ToListAsync();

            return history;
        }

        public async Task<DealReadDto?> UpdateDealAsync(long id, DealEditDto dto)
        {
            var deal = await _context.Deals
                .Include(d => d.contact)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
            {
                return null;
            }

            if (!await _context.Regions.AnyAsync(r => r.Id == dto.RegionId))
                throw new InvalidOperationException($"Region with ID {dto.RegionId} does not exist.");
            if (!await _context.DealStages.AnyAsync(ds => ds.Id == dto.DealStageId))
                throw new InvalidOperationException($"Deal Stage with ID {dto.DealStageId} does not exist.");
            if (!await _context.RevenueTypes.AnyAsync(rt => rt.Id == dto.RevenueTypeId))
                throw new InvalidOperationException($"Revenue Type with ID {dto.RevenueTypeId} does not exist.");
            if (!await _context.DUs.AnyAsync(du => du.Id == dto.DuId))
                throw new InvalidOperationException($"DU with ID {dto.DuId} does not exist.");
            if (!await _context.Countries.AnyAsync(c => c.Id == dto.CountryId))
                throw new InvalidOperationException($"Country with ID {dto.CountryId} does not exist.");
            if (dto.ServiceId.HasValue && !await _context.ServiceLines.AnyAsync(sl => sl.Id == dto.ServiceId.Value))
                throw new InvalidOperationException($"Service Line with ID {dto.ServiceId.Value} does not exist.");
            if (dto.AccountId.HasValue && !await _context.Accounts.AnyAsync(a => a.Id == dto.AccountId.Value))
                throw new InvalidOperationException($"Account with ID {dto.AccountId.Value} does not exist.");

            Customer? customer;
            try
            {
                customer = await _customerRepository.GetByNameAsync(dto.CustomerName);
                if (customer == null)
                {
                    var customerCreateDto = new CustomerCreateDto
                    {
                        CustomerName = dto.CustomerName,
                        Website = null,
                        CustomerPhoneNumber = null,
                        CreatedBy = deal.CreatedBy
                    };
                    customer = await _customerRepository.AddCustomerAsync(customerCreateDto);
                    if (customer == null || customer.Id == 0)
                    {
                        throw new InvalidOperationException($"Failed to create customer: {dto.CustomerName}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating customer: {ex.Message}. Inner Exception: {ex.InnerException?.ToString()}", ex);
            }

            string firstName;
            string? lastName = null;
            var nameParts = dto.ContactFullName.Trim().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            firstName = nameParts.Length > 0 ? nameParts[0] : "Unknown";
            if (nameParts.Length > 1)
            {
                lastName = nameParts[1];
            }

            Contact? contact;
            try
            {
                contact = await _contactRepository.GetByFullNameAndCustomerIdAsync(firstName, lastName, customer.Id);

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
                        CreatedBy = deal.CreatedBy
                    };
                    contact = await _contactRepository.AddContactAsync(contactCreateDto);
                    if (contact == null || contact.Id == 0)
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
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating or updating contact: {ex.Message}. Inner Exception: {ex.InnerException?.ToString()}", ex);
            }

            if (contact.Id == 0)
            {
                throw new InvalidOperationException($"Contact '{dto.ContactFullName}' for customer '{customer.CustomerName}' has an invalid ID after creation/retrieval.");
            }

            deal.DealName = dto.Title;
            deal.DealAmount = dto.Amount;
            deal.AccountId = dto.AccountId ?? deal.AccountId;
            deal.RegionId = dto.RegionId;
            deal.DomainId = dto.DomainId ?? deal.DomainId;
            deal.DealStageId = dto.DealStageId;
            deal.RevenueTypeId = dto.RevenueTypeId;
            deal.DuId = dto.DuId;
            deal.CountryId = dto.CountryId;
            deal.ServiceLineId = dto.ServiceId ?? deal.ServiceLineId;
            deal.Description = dto.Description ?? deal.Description;
            deal.Probability = dto.Probability ?? deal.Probability;
            deal.StartingDate = dto.StartingDate ?? deal.StartingDate;
            deal.ClosingDate = dto.ClosingDate ?? deal.ClosingDate;
            deal.ContactId = contact.Id;
            deal.UpdatedAt = DateTime.UtcNow;

            _context.Deals.Update(deal);
            await _context.SaveChangesAsync();

            return await GetDealByIdAsync(deal.Id);
        }

        public async Task<int> GetStageIdByNameAsync(string stageName)
        {
            var stage = await _context.DealStages
                .FirstOrDefaultAsync(s => s.StageName == stageName);

            if (stage == null)
                throw new InvalidOperationException($"Unknown stage name: {stageName}");

            return (int)stage.Id;
        }

        public async Task<DealReadDto?> UpdateDealDescriptionAsync(long id, DealDescriptionUpdateDto dto)
        {
            var deal = await _context.Deals
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
            {
                return null;
            }


            deal.Description = dto.Description;
            deal.UpdatedAt = DateTime.UtcNow;
            deal.UpdatedBy = dto.UpdatedBy;


            await _context.SaveChangesAsync();


            return await GetDealByIdAsync(deal.Id);
        }
    }
}