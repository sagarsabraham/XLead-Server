namespace XLead_Server.DTOs
{
    public class CompanyReadDto
    {
        public long Id { get; set; }
        public string CompanyName { get; set; }
        public string Website { get; set; }
        public string CompanyPhoneNumber { get; set; }
        public string Status { get; set; }
        public string OwnerName { get; set; } // Add this property
    }
}