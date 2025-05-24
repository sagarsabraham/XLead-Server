using System.Net.Mail;
using System.Numerics;

namespace XLead_Server.Models
{
    public class Deal
    {
        public long Id { get; set; }
        public string DealName { get; set; }
        public decimal DealAmount { get; set; }
        public int AccountId { get; set; }
        public Account Account { get; set; }
        public int RegionId { get; set; }
        public Region Region { get; set; }
        public int DomainId { get; set; }
        public Domain Domain { get; set; }
        public int RevenueTypeId { get; set; }
        public RevenueType RevenueType { get; set; }
        public int DuId { get; set; }
        public DU DU { get; set; }
        public int CountryId { get; set; }
        public Country Country { get; set; }
        public string Description { get; set; }
        public decimal Probability { get; set; }
        public long ContactId { get; set; }
        public Contact Contact { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
        public ICollection<DealStage> DealStages { get; set; } = new List<DealStage>();
        public ICollection<StageHistory> DealStageHistory { get; set; } = new List<StageHistory>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
