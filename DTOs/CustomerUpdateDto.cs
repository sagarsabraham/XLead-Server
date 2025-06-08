
namespace XLead_Server.DTOs
{
    public class CustomerUpdateDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public string PhoneNo { get; set; } = string.Empty;
        public string? Website { get; set; }
        public int? IndustryVerticalId { get; set; }
        public bool IsActive { get; set; }
      
    }
}