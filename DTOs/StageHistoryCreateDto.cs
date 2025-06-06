namespace XLead_Server.DTOs
{
    public class StageHistoryCreateDto
    {
        public long DealId { get; set; }
        public string StageName { get; set; } = null!;
        public long CreatedBy { get; set; }
    }
}
