using System.ComponentModel.DataAnnotations;
using XLead_Server.Models;

public class Attachment
{
    [Key]
    public long AttachmentId { get; set; }
    public string FileName { get; set; }
    public string S3UploadName { get; set; } //=id.extension
    public long CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ICollection<Deal> Deals { get; set; }

}
