using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using XLead_Server.DTOs;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentRepository _attachmentRepository;
        private readonly IMapper _mapper;

        public AttachmentController(IAttachmentRepository attachmentRepository, IMapper mapper)
        {
            _attachmentRepository = attachmentRepository;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<IActionResult> AddAttachment([FromBody] AttachmentCreateDto dto)
        {
            try
            {
                // Step 1: Map to Entity & save to generate Id
                var attachment = _mapper.Map<Attachment>(dto);
                attachment.CreatedAt = DateTime.UtcNow;

                await _attachmentRepository.AddAsync(attachment);
                await _attachmentRepository.SaveAsync(); // Commit to get Id

                // Step 2: Generate S3UploadName
                var extension = Path.GetExtension(attachment.FileName);
                attachment.S3UploadName = $"{attachment.Id}{extension}";

                _attachmentRepository.Update(attachment);
                await _attachmentRepository.SaveAsync();

                // Step 3: Map to read DTO and return
                var resultDto = _mapper.Map<AttachmentReadDto>(attachment);
                return Ok(resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error saving attachment: {ex.Message}");
            }
        }

        // Optional: GET endpoint for attachments
        [HttpGet("deal/{dealId}")]
        public async Task<IActionResult> GetAttachmentsByDeal(long dealId)
        {
            var attachments = await _attachmentRepository.GetByDealIdAsync(dealId);
            var resultDtos = _mapper.Map<List<AttachmentReadDto>>(attachments);
            return Ok(resultDtos);
        }
    }
}