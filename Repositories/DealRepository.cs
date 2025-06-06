using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
        //private readonly ILogger<DealRepository> _logger;

        public DealRepository(
            ApiDbContext context,
            ICustomerRepository customerRepository,
            IContactRepository contactRepository,
            IMapper mapper
             //ILogger<DealRepository> logger
            )


        {
            _context = context;
            _customerRepository = customerRepository;
            _contactRepository = contactRepository;
            _mapper = mapper;
            //_logger = logger;
        }

        public async Task<DealReadDto?> AddDealAsync(DealCreateDto dto)
        {
            try
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
            catch (DbUpdateException dbEx)
            {
                var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                Console.WriteLine($"Database error: {innerMessage}"); // Use Console.WriteLine instead
                throw new InvalidOperationException($"Database error: {innerMessage}", dbEx);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in AddDealAsync: {ex.Message}"); // Use Console.WriteLine instead
                throw;
            }
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


        public async Task<DealReadDto?> UpdateDealStageAsync(long id, DealUpdateDTO dto)
        {
            // 1. Find the deal by ID
            var deal = await _context.Deals
                .Include(d => d.dealStage)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
            {
                return null; // Deal not found
            }

            // 2. Find the stage by StageName
            var newStage = await _context.DealStages
                .FirstOrDefaultAsync(s => s.StageName == dto.StageName);

            if (newStage == null)
            {
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
    }
    }
