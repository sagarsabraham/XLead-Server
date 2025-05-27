//using Microsoft.EntityFrameworkCore;
//using XLead_Server.Data;
//using XLead_Server.Interfaces;

//namespace XLead_Server.Repositories
//{
//    public class AttachmentRepository : IAttachmentRepository
//    {
//        private ApiDbContext _context;

//        public AttachmentRepository(ApiDbContext context)
//        {
//            _context = context;
//        }
//        public async Task<IEnumerable<Attachment>> GetAllAttachments()
//        {
//            return await _context.Attachments.ToListAsync();
//        }
//    }
//}
