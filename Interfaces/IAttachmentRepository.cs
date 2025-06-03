using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IAttachmentRepository
    {
        Task<IEnumerable<Attachment>> GetAllAttachments();
    }
}
