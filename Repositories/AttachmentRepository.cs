using System;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Repositories
{
    public class AttachmentRepository : IAttachmentRepository
    {
        private readonly ApiDbContext _context;
        private readonly string _uploadPath;

        public AttachmentRepository(ApiDbContext context, IConfiguration configuration)
        {
            _context = context;
            _uploadPath = configuration.GetValue<string>("UploadPath") ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<Attachment> AddAsync(Attachment attachment)
        {
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task<Attachment> GetByIdAsync(int id)
        {
            return await _context.Attachments.FindAsync(id);
        }

        public async Task DeleteAsync(int id)
        {
            var attachment = await _context.Attachments.FindAsync(id);
            if (attachment != null)
            {
                // Delete the file from the local folder
                var filePath = Path.Combine(_uploadPath, attachment.S3UploadName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }
    }
}