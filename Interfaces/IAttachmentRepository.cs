using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IAttachmentRepository
    {
        Task AddAsync(Attachment attachment);
        void Update(Attachment attachment);
        Task SaveAsync();
        Task<IEnumerable<Attachment>> GetByDealIdAsync(long dealId);
    }
}
