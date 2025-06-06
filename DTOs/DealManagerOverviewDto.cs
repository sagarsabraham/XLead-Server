namespace XLead_Server.DTOs
{
    public class DealManagerOverviewDto
    {
        public long Id { get; set; }
        public string DealName { get; set; }
        public decimal DealAmount { get; set; }
        public string? StageName { get; set; }
        public DateTime? ClosingDate { get; set; }
        public long SalespersonId { get; set; }      // This is Deal.CreatedBy
        public string SalespersonName { get; set; } = "Unknown"; // User.Name of the creator

        // Include other fields visible in your table:
        public string? AccountName { get; set; }
        public string? RegionName { get; set; }
        public string? DUName { get; set; } // Department
        public string? ContactName { get; set; }
        public DateTime? StartingDate { get; set; }
        // Add any other fields from DealReadDto that you show in the table.
    }
}

