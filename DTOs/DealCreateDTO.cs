namespace XLead_Server.DTOs
{
    public class DealCreateDto
    {
        public string DealName { get; set; }
        public decimal DealAmount { get; set; }
        public int AccountId { get; set; }
        public int RegionId { get; set; }
        public int DomainId { get; set; }
        public int RevenueTypeId { get; set; }
        public int DuId { get; set; }
        public int CountryId { get; set; }
        public string Description { get; set; }
        public decimal Probability { get; set; }
        public int StageId { get; set; }
        public long ContactId { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public long CreatedBy { get; set; }
    }
}
