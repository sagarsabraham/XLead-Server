using System.Net.Mail;
using System.Numerics;

namespace XLead_Server.Models
{
    public class Deal
    {
        public long Id { get; set; }
        public string DealName {  get; set; }
        public decimal DealAmount { get; set; }
        public int AccountId { get; set; }
        public int RegionId { get; set; }
        public int DomainId { get; set; }
        public int RevenueTypeId { get; set; }
        public int DuId {  get; set; }
        public int CountryId { get; set; }
        public string Description { get; set; }
        public decimal Probability { get; set; }
        public int StageId { get; set; }
        public long ContactId { get; set; }
        public long AttachmentId { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
        public Contact Contact { get; set; }
        public ICollection<StageHistory> DealStageHistory { get; set; }
    }
}
