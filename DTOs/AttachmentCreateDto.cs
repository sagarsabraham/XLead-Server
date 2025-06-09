namespace XLead_Server.DTOs
{
    public class AttachmentCreateDto
    {
        public string FileName { get; set; }
        public string S3UploadName { get; set; }
        public long DealId { get; set; }
        public long CreatedBy { get; set; }
    }
}