using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XLead_Server.Data;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly string _uploadPath;

        public AttachmentsController(IAttachmentRepository attachmentRepository, IConfiguration configuration)
        {
            _attachmentRepository = attachmentRepository;
            _uploadPath = configuration.GetValue<string>("UploadPath") ?? Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        }

        [HttpPost("deal/{dealId}")]
        [ProducesResponseType(typeof(Attachment), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<Attachment>> UploadFile(long dealId, long CreatedBy,  IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            try
            {
                // Create a new attachment entry
                var attachment = new Attachment
                {
                    FileName = file.FileName,
                    DealId = dealId,
                    CreatedBy = CreatedBy, // Replace with actual user
                    CreatedAt = DateTime.UtcNow
                };

                // Save to database to get the ID
                attachment = await _attachmentRepository.AddAsync(attachment);

                // Generate S3UploadName as id.extension
                var extension = Path.GetExtension(file.FileName);
                attachment.S3UploadName = $"{attachment.Id}{extension}";
                attachment.UpdatedBy = CreatedBy;
                attachment.UpdatedAt = DateTime.UtcNow;

                // Update the attachment with S3UploadName
                // Since we're directly using the repository, we'll call AddAsync again to update
                attachment = await _attachmentRepository.AddAsync(attachment);

                // Save the file to the local folder
                var filePath = Path.Combine(_uploadPath, attachment.S3UploadName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return the created attachment with location header
                return CreatedAtAction(nameof(GetAttachmentById), new { id = attachment.Id }, attachment);
            }
            catch (InvalidOperationException ex)
            {
                // Log ex (for debugging)
                return Problem(ex.Message, statusCode: 400);
            }
            catch (Exception ex)
            {
                // Log ex (for debugging)
                return Problem($"An unexpected error occurred: {ex.Message}", statusCode: 500);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Attachment), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<Attachment>> GetAttachmentById(int id)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(id);
            if (attachment == null)
            {
                return NotFound($"Attachment with ID {id} not found.");
            }
            return Ok(attachment);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult> DeleteFile(int id)
        {
            var attachment = await _attachmentRepository.GetByIdAsync(id);
            if (attachment == null)
            {
                return NotFound($"Attachment with ID {id} not found.");
            }

            try
            {
                await _attachmentRepository.DeleteAsync(id);
                return Ok("Attachment deleted successfully.");
            }
            catch (Exception ex)
            {
                // Log ex (for debugging)
                return Problem($"An unexpected error occurred: {ex.Message}", statusCode: 500);
            }
        }
    }
}