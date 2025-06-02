// XLead_Server/Repositories/DealRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly ICompanyRepository _companyRepository;
        private readonly IContactRepository _contactRepository;

        public DealRepository(
            ApiDbContext context,
            ICompanyRepository companyRepository,
            IContactRepository contactRepository)
        {
            _context = context;
            _companyRepository = companyRepository;
            _contactRepository = contactRepository;
        }

        public async Task<DealReadDto?> AddDealAsync(DealCreateDto dto)
        {
            // 1. Find or Create Company
            Company? company = await _companyRepository.GetByNameAsync(dto.CompanyName);
            if (company == null)
            {
                var companyCreateDto = new CompanyCreateDto
                {
                    CompanyName = dto.CompanyName,
                    // Provide sensible defaults or make these part of DealCreateDto if needed
                    // If these fields are required for a company, this logic needs adjustment
                    Website = null,
                    CompanyPhoneNumber = null,
                    CreatedBy = dto.CreatedBy
                };
                company = await _companyRepository.AddCompanyAsync(companyCreateDto);
                if (company == null)
                {
                    // Consider throwing a more specific exception or returning a result object
                    throw new InvalidOperationException($"Failed to create company: {dto.CompanyName}");
                }
            }

            // 2. Find or Create Contact
            string firstName;
            string? lastName = null; // Initialize lastName as nullable
            var nameParts = dto.ContactFullName.Trim().Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

            firstName = nameParts.Length > 0 ? nameParts[0] : "Unknown"; // Default if empty string somehow passed
            if (nameParts.Length > 1)
            {
                lastName = nameParts[1];
            }

            Contact? contact = await _contactRepository.GetByFullNameAndCompanyIdAsync(firstName, lastName, company.Id);

            if (contact == null)
            {
                var contactCreateDto = new ContactCreateDto
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = null, // Default or make part of DealCreateDto
                    PhoneNumber = null, // Default or make part of DealCreateDto
                    CompanyId = company.Id,
                    CompanyName = company.CompanyName, // For DTO consistency, though CompanyId is primary
                    CreatedBy = dto.CreatedBy
                };
                contact = await _contactRepository.AddContactAsync(contactCreateDto);
                if (contact == null)
                {
                    throw new InvalidOperationException($"Failed to create contact: {dto.ContactFullName}");
                }
            }

            // Defensive check, though AddContactAsync should ensure ID is set by EF Core
            if (contact.Id == 0)
            {
                throw new InvalidOperationException($"Contact '{dto.ContactFullName}' for company '{company.CompanyName}' has an invalid ID after creation/retrieval.");
            }

            // 3. Create Deal entity
            var deal = new Deal
            {
                DealName = dto.Title,
                DealAmount = dto.Amount,
              
                Description = dto.Description,
                Probability = dto.Probability??= 0,
                //StartingDate = dto.StartingDate,
                // Fix for CS0266 and CS8629: Ensure nullable DateTime? is handled properly by providing a default value or throwing an exception if null.
                StartingDate = dto.StartingDate ?? throw new InvalidOperationException("StartingDate cannot be null."),
                ClosingDate = dto.ClosingDate ?? throw new InvalidOperationException("ClosingDate cannot be null."),
                //ClosingDate = dto.ClosingDate,
                AccountId = dto.AccountId ??= 0,
                RegionId = dto.RegionId ??= 0,
                DomainId = dto.DomainId ??= 0,
                RevenueTypeId = dto.RevenueTypeId??=0,
                DuId = dto.DuId??=0,
                CountryId = dto.CountryId??=0,
                DealStageId = dto.DealStageId??=0,
                ContactId = contact.Id,
                CreatedBy = dto.CreatedBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();

            return await GetDealByIdAsync(deal.Id); // Return the full DTO
        }

        public async Task<DealReadDto?> GetDealByIdAsync(long id)
        {
            var deal = await _context.Deals
                .AsNoTracking() // Good for read operations
                .Include(d => d.AccountId)
                .Include(d => d.RegionId)
                .Include(d => d.DomainId)
                .Include(d => d.RevenueTypeId)
                .Include(d => d.DuId)
                .Include(d => d.CountryId)
                .Include(d => d.ContactId)
                .Include(d => d.DealStageId)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null) return null;

            return new DealReadDto
            {
                Id = deal.Id,
                DealName = deal.DealName,
                DealAmount = deal.DealAmount,
                AccountId = deal.AccountId, // Use AccountId directly
                AccountName = null, // Set to null since 'Account' is not part of the 'Deal' entity
                RegionId = deal.RegionId,
                RegionName = deal.region?.RegionName,
                DomainId = deal.DomainId,
                DomainName = deal.domain?.DomainName,
                RevenueTypeId = deal.RevenueTypeId,
                RevenueTypeName = deal.revenueType?.RevenueTypeName,
                DuId = deal.DuId,
                DUName = deal.du?.DUName,
                CountryId = deal.CountryId,
                CountryName = deal.country?.CountryName,
                Description = deal.Description,
                Probability = deal.Probability,
                DealStageId = deal.DealStageId,
                StageName = deal.dealStage?.StageName,
                ContactId = deal.ContactId,
                ContactName = deal.contact != null ? $"{deal.contact.FirstName} {deal.contact.LastName}".Trim() : null,
                StartingDate = deal.StartingDate,
                ClosingDate = deal.ClosingDate,
                CreatedBy = deal.CreatedBy,
                CreatedAt = deal.CreatedAt
            };
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
                .Select(deal => new DealReadDto // Project to DTO
                {
                    Id = deal.Id,
                    DealName = deal.DealName,
                    DealAmount = deal.DealAmount,
                   
                    AccountId = deal.AccountId,
                    AccountName = deal.account != null ? deal.account.AccountName : null,
                    RegionId = deal.RegionId,
                    RegionName = deal.region != null ? deal.region.RegionName : null,
                    DomainId = deal.DomainId,
                    DomainName = deal.domain != null ? deal.domain.DomainName : null,
                    RevenueTypeId = deal.RevenueTypeId,
                    RevenueTypeName = deal.revenueType != null ? deal.revenueType.RevenueTypeName : null,
                    DuId = deal.DuId,
                    DUName = deal.du != null ? deal.du.DUName : null,
                    CountryId = deal.CountryId,
                    CountryName = deal.country != null ? deal.country.CountryName : null,
                    Description = deal.Description,
                    Probability = deal.Probability,
                    DealStageId = deal.DealStageId,
                    StageName = deal.dealStage != null ? deal.dealStage.StageName : null,
                    ContactId = deal.ContactId,
                    ContactName = deal.contact != null ? $"{deal.contact.FirstName} {deal.contact.LastName}".Trim() : null,
                    StartingDate = deal.StartingDate,
                    ClosingDate = deal.ClosingDate,
                    CreatedBy = deal.CreatedBy,
                    CreatedAt = deal.CreatedAt
                })
                .ToListAsync();
        }
    }
}