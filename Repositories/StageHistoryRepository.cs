using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class StageHistoryRepository : IStageHistoryRepository
    {
        private readonly ApiDbContext _context;
        private readonly IMapper _mapper;
        public StageHistoryRepository(ApiDbContext context,IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<StageHistoryReadDto> CreateStageHistoryAsync(StageHistoryCreateDto stageHistoryDto)
        {
            if (stageHistoryDto.DealId == 0)
            {
                throw new ArgumentException("DealId cannot be zero.", nameof(stageHistoryDto.DealId));
            }
            if (string.IsNullOrWhiteSpace(stageHistoryDto.StageName))
            {
                throw new ArgumentException("StageName cannot be null or empty.", nameof(stageHistoryDto.StageName));
            }
            if (stageHistoryDto.CreatedBy == 0)
            {
                throw new ArgumentException("CreatedBy cannot be zero.", nameof(stageHistoryDto.CreatedBy));
            }

            var stage = await _context.DealStages
                .FirstOrDefaultAsync(s => s.StageName == stageHistoryDto.StageName);

            if (stage == null)
            {
                throw new InvalidOperationException($"Stage '{stageHistoryDto.StageName}' not found.");
            }

            var stageHistory = new StageHistory
            {
                DealId = stageHistoryDto.DealId,
                DealStageId = stage.Id,
                CreatedBy = stageHistoryDto.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.StageHistories.Add(stageHistory);
            await _context.SaveChangesAsync();

            return new StageHistoryReadDto
            {
                Id = stageHistory.Id,
                DealId = stageHistory.DealId,
                StageName = stage.StageName,
                CreatedBy = stageHistory.CreatedBy,
                CreatedAt = stageHistory.CreatedAt,
                UpdatedBy = stageHistory.UpdatedBy,
                UpdatedAt = stageHistory.UpdatedAt
            };
        }
        public async Task<IEnumerable<StageHistoryReadDto>> GetStageHistoryByDealIdAsync(long dealId)
        {
            var historyEntities = await _context.StageHistories
                .Where(sh => sh.DealId == dealId)
                .Include(sh => sh.DealStage) 
                .OrderByDescending(sh => sh.CreatedAt) 
                .AsNoTracking()
                .ToListAsync();

            return _mapper.Map<IEnumerable<StageHistoryReadDto>>(historyEntities);
        }
    }
}
