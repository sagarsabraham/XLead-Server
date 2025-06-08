using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly ApiDbContext _context;

        public AttachmentRepository(ApiDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Attachment attachment)
        {
            await _context.Attachments.AddAsync(attachment);
        }

        public void Update(Attachment attachment)
        {
            _context.Attachments.Update(attachment);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Attachment>> GetByDealIdAsync(long dealId)
        {
            return await _context.Attachments
                .Where(a => a.DealId == dealId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
    }
}