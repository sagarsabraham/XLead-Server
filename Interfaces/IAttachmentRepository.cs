using XLead_Server.DTOs;
using XLead_Server.Models;

namespace XLead_Server.Interfaces
{
    public interface IAttachmentRepository
    {
        Task<Attachment> AddAsync(Attachment attachment);
        Task<Attachment> GetByIdAsync(int id);
        Task DeleteAsync(int id);
    }
}