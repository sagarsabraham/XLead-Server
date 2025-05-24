namespace XLead_Server.DTOs
{
    public class DealReadDto
    {
        public long Id { get; set; }
        public string DealName { get; set; }
        public decimal DealAmount { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public int RegionId { get; set; }
        public string RegionName { get; set; }
        public int DomainId { get; set; }
        public string DomainName { get; set; }
        public int RevenueTypeId { get; set; }
        public string RevenueTypeName { get; set; }
        public int DuId { get; set; }
        public string DUName { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string Description { get; set; }
        public decimal Probability { get; set; }
      
        public long? StageId { get; set; }
        public string StageName { get; set; }
       

        public long ContactId { get; set; }
        public string ContactName { get; set; }
        public DateTime StartingDate { get; set; }
        public DateTime ClosingDate { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
