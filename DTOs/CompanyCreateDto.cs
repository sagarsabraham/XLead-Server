namespace XLead_Server.DTOs
{
    public class CompanyCreateDto
    {
        public string CompanyName { get; set; }
        //public string IndustryVerticalId { get; set; }
        public string Website { get; set; }
        public string CompanyPhoneNumber { get; set; }
        public long CreatedBy { get; set; }
    }
}