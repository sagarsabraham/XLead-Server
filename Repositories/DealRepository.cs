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
        private readonly ILogger<DealRepository> _logger;

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
            if (dto.ServiceId == null || dto.ServiceId == 0)
                throw new InvalidOperationException("Service Line ID is required.");
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
            if (!await _context.ServiceLines.AnyAsync(sl => sl.Id == dto.ServiceId.Value))
                throw new InvalidOperationException($"Service Line with ID {dto.ServiceId.Value} does not exist.");

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
                .Include(d => d.serviceLine)
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
                .Include(d => d.serviceLine)
                .Select(deal => _mapper.Map<DealReadDto>(deal))
                .ToListAsync();
        }

        public async Task<DealReadDto?> UpdateDealStageAsync(long id, DealUpdateDTO dto)
        {
            var deal = await _context.Deals
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
            {
                _logger.LogWarning("Deal with ID {DealId} not found in UpdateDealStageAsync.", id);
                return null;
            }

            var stage = await _context.DealStages
                .FirstOrDefaultAsync(s => s.StageName == dto.StageName);

            if (stage == null)
            {
                _logger.LogWarning("Stage '{StageName}' not found for deal ID {DealId}.", dto.StageName, id);
                throw new InvalidOperationException($"Stage '{dto.StageName}' not found.");
            }
            _logger.LogInformation("Found stage '{StageName}' with ID {StageId} for deal ID {DealId}.", stage.StageName, stage.Id, id);
            deal.DealStageId = stage.Id;
            deal.UpdatedAt = DateTime.UtcNow;
            deal.UpdatedBy = dto.PerformedByUserId;

            switch (stage.Id)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                    deal.IsHidden = false;
                    break;
                case 6:
                    deal.IsHidden = true;
                    break;
                default:
                    _logger.LogWarning("Unexpected stage ID {StageId} for stage '{StageName}' while updating deal ID {DealId}.", stage.Id, stage.StageName, id);
                    throw new InvalidOperationException($"Invalid stage ID: {stage.Id}. Expected stage ID between 1 and 6.");
            }

            _logger.LogInformation("Set IsHidden to {IsHidden} for deal ID {DealId} based on stage ID {StageId}.", deal.IsHidden, id, stage.Id);

            await _context.SaveChangesAsync();
            _logger.LogInformation("Changes saved for deal ID {DealId}. IsHidden is now {IsHidden} in the database.", id, deal.IsHidden);

            var updatedDeal = await GetDealByIdAsync(deal.Id);
            _logger.LogInformation("Retrieved updated deal ID {DealId} with IsHidden: {IsHidden}.", id, updatedDeal?.IsHidden);

            return updatedDeal;

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

    }
}