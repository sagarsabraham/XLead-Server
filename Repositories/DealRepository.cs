using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Models;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.DTOs;

namespace XLead_Server.Repositories
{
    public class DealRepository : IDealRepository
    {
        private readonly ApiDbContext _context;

        public DealRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<Deal> AddDealAsync(Deal deal)
        {
            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();
            return deal;
        }



        public async Task<DealReadDto?> GetDealByIdAsync(long id)
        {
            var deal = await _context.Deals
                .Include(d => d.Account)
                .Include(d => d.Region)
                .Include(d => d.Domain)
                .Include(d => d.RevenueType)
                .Include(d => d.DU)
                .Include(d => d.Country)
                .Include(d => d.Contact)
                .Include(d => d.DealStages)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (deal == null)
                return null;

            // Get the current stage (latest by CreatedAt)
            var currentStage = deal.DealStages
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefault();

            return new DealReadDto
            {
                Id = deal.Id,
                DealName = deal.DealName,
                DealAmount = deal.DealAmount,
                AccountId = deal.AccountId,
                AccountName = deal.Account?.AccountName,
                RegionId = deal.RegionId,
                RegionName = deal.Region?.RegionName,
                DomainId = deal.DomainId,
                DomainName = deal.Domain?.DomainName,
                RevenueTypeId = deal.RevenueTypeId,
                RevenueTypeName = deal.RevenueType?.RevenueTypeName,
                DuId = deal.DuId,
                DUName = deal.DU?.DUName,
                CountryId = deal.CountryId,
                CountryName = deal.Country?.CountryName,
                Description = deal.Description,
                Probability = deal.Probability,
                StageId = currentStage?.Id,
                StageName = currentStage?.StageName,
                ContactId = deal.ContactId,
                ContactName = deal.Contact != null ? $"{deal.Contact.FirstName} {deal.Contact.LastName}" : null,
                StartingDate = deal.StartingDate,
                ClosingDate = deal.ClosingDate,
                CreatedBy = deal.CreatedBy,
                CreatedAt = deal.CreatedAt
            };
        }



        public async Task<IEnumerable<Deal>> GetAllDealsAsync()
        {
            return await _context.Deals.ToListAsync();
        }
    }
}
