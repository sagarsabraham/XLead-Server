namespace XLead_Server.DTOs
{
    public class AttachmentDto
    {
        public long Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string S3UploadName { get; set; } = string.Empty;
        public long DealId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAttachmentDto
    {
        public string FileName { get; set; } = string.Empty;
        public long DealId { get; set; }
        public long CreatedBy { get; set; }
    }
}
