namespace XLead_Server.DTOs
{
    public class CustomerReadDto
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public string Status { get; set; } 
        public string? Website { get; set; }
        public string? CustomerPhoneNumber { get; set; }
        public long? IndustryVerticalId { get; set; }

    }
}
