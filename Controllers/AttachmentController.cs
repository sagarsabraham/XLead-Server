using Microsoft.AspNetCore.Mvc;
using XLead_Server.Data;
using XLead_Server.Models;
using XLead_Server.Interfaces;
using XLead_Server.DTOs;

using AutoMapper;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;

        public AttachmentsController(
            IAttachmentRepository attachmentRepository,
            IWebHostEnvironment env,
            IMapper mapper)
        {
            _attachmentRepository = attachmentRepository;
            _env = env;
            _mapper = mapper;
        }

        [HttpGet("deal/{dealId}")]
        public async Task<ActionResult<IEnumerable<AttachmentReadDto>>> GetAttachmentsForDeal(long dealId)
        {
            var attachments = await _attachmentRepository.GetByDealIdAsync(dealId);

            return Ok(_mapper.Map<IEnumerable<AttachmentReadDto>>(attachments));
        }

        [HttpPost("upload")]
        public async Task<ActionResult<AttachmentReadDto>> UploadAttachment([FromForm] IFormFile file, [FromForm] long dealId)
        {
            if (dealId <= 0)
            {
                return BadRequest(new { message = "A valid DealId is required." });
            }
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded or file is empty." });
            }

            Console.WriteLine($"Received upload request for Deal ID: {dealId} with file: {file.FileName} ({file.Length} bytes)");

            var attachment = new Attachment
            {
                FileName = file.FileName,
                S3UploadName = "",
                DealId = dealId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            };

            await _attachmentRepository.AddAsync(attachment);
            await _attachmentRepository.SaveAsync();

            string fileExtension = Path.GetExtension(file.FileName);
            string uniqueFileName = $"{attachment.Id}{fileExtension}";

            attachment.S3UploadName = uniqueFileName;
            _attachmentRepository.Update(attachment);
            await _attachmentRepository.SaveAsync();

            var uploadsFolderPath = Path.Combine(_env.ContentRootPath, "UploadedFiles");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }
            var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file {uniqueFileName}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error saving the uploaded file.");
            }

            var attachmentDto = _mapper.Map<AttachmentReadDto>(attachment);

            return CreatedAtAction(nameof(GetAttachmentsForDeal), new { dealId = attachmentDto.DealId }, attachmentDto);
        }
    }
}