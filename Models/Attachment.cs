namespace XLead_Server.Models
{
    public class Attachment
    {
        public long Id { get; set; }
        public string FileName { get; set; }
        public string S3UploadName { get; set; }
        public long DealId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }


        // Navigation properties
        public User Creator { get; set; }
        public Deal Deal { get; set; }
    }
}