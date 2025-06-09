using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Data; // Your DbContext namespace
using XLead_Server.Models;

[Route("api/[controller]")]
[ApiController]
public class AttachmentsController : ControllerBase
{
    private readonly ApiDbContext _context;
    private readonly IWebHostEnvironment _env;

    public AttachmentsController(ApiDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // GET: api/attachments/deal/{dealId}
    [HttpGet("deal/{dealId}")]
    public async Task<ActionResult<IEnumerable<Attachment>>> GetAttachmentsForDeal(long dealId)
    {
        // Returns a list of attachment metadata for a given deal
        return await _context.Attachments
            .Where(a => a.DealId == dealId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
    }

    // POST: api/attachments/upload
    [HttpPost("upload")]
    public async Task<ActionResult<Attachment>> UploadAttachment([FromForm] IFormFile file, [FromForm] long dealId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // --- Create DB record first to get the ID ---
        var attachment = new Attachment
        {
            FileName = file.FileName,
            S3UploadName = "", // We'll fill this in after getting the ID
            DealId = dealId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = 1 // TODO: Get current user ID from HttpContext.User claims
        };

        // 1. Add to context and save to generate the ID
        _context.Attachments.Add(attachment);
        await _context.SaveChangesAsync();

        // --- Now, use the generated ID to create the unique filename ---
        string fileExtension = Path.GetExtension(file.FileName);
        string uniqueFileName = $"{attachment.Id}{fileExtension}";

        // 2. Update the entity with the unique name
        attachment.S3UploadName = uniqueFileName;
        await _context.SaveChangesAsync();

        // --- Save the physical file to the server ---
        var uploadsFolderPath = Path.Combine(_env.ContentRootPath, "UploadedFiles");
        if (!Directory.Exists(uploadsFolderPath))
        {
            Directory.CreateDirectory(uploadsFolderPath);
        }

        var filePath = Path.Combine(uploadsFolderPath, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return the created attachment metadata
        return CreatedAtAction(nameof(GetAttachmentsForDeal), new { dealId = attachment.DealId }, attachment);
    }
}