namespace XLead_Server.DTOs
{
    public class StageHistoryDto
    {
        public long Id { get; set; }
        public long DealId { get; set; }
        public string StageName { get; set; }
        public string StageDisplayName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}