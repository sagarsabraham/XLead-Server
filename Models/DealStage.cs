namespace XLead_Server.Models
{
    public class DealStage
    {
        public long Id { get; set; }
        public string StageName { get; set; }
        public string DisplayName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public long DealId { get; set; } // Foreign key to Deal
        public Deal Deal { get; set; } // Navigation property for one Deal
        public ICollection<Deal> Deals { get; set; } = new List<Deal>(); // Navigation property for many Deals

        public ICollection<StageHistory> DealStageHistory { get; set; } = new List<StageHistory>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
Required Deal Class
To complet