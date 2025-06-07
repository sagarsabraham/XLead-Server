namespace XLead_Server.DTOs
{
    public class AttachmentReadDto
    {
        public string FileName { get; set; } = string.Empty;
        public string S3UploadName { get; set; } = string.Empty;
        public long DealId { get; set; }
        public long CreatedBy { get; set; }
    }
}
