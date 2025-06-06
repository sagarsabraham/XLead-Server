
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using XLead_Server.Interfaces;
using XLead_Server.Models;

namespace XLead_Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentRepository _attachmentRepository;

        public AttachmentController(IAttachmentRepository attachmentRepository)
        {
            _attachmentRepository = attachmentRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Attachment>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<Attachment>>> GetAllAttachmentsAsync()
        {
            var attachments = await _attachmentRepository.GetAllAttachments();
            if (attachments == null)
            {
                return NotFound("Attachment data is currently unavailable or no attachments found.");
            }
            return Ok(attachments);
        }
    }
}
