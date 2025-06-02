namespace XLead_Server.Models
{
    public class DealStage
    {
        public long Id { get; set; }
        public string StageName { get; set; }
        public string DisplayName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User Creator { get; set; }
        public ICollection<Deal> Deals { get; set; }
        public ICollection<StageHistory> DealStageHistories { get; set; }
    }
}
