namespace XLead_Server.DTOs
{
    public class StageHistoryReadDto
    {
        public long Id { get; set; }
        public long DealId { get; set; }
        public string StageName { get; set; } = null!;
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}