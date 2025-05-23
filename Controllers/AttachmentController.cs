using Microsoft.AspNetCore.Mvc;
using XLead_Server.Interfaces;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentController : ControllerBase
    {
        private IAttachmentRepository _attachmentRepository;
        public AttachmentController(IAttachmentRepository attachmentRepository)
        {
            _attachmentRepository = attachmentRepository;
        }

        [HttpGet]

        public async Task<IActionResult> Get()
        {
            var attachments = await _attachmentRepository.GetAllAttachments();
            if (attachments == null)
            {
                return NotFound();
            }
            return Ok(attachments);
        }
    }
}
