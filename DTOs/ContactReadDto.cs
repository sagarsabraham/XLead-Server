namespace XLead_Server.DTOs
{
    public class ContactReadDto
    {
       
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CompanyName { get; set; }
        public bool IsActive { get; set; }
    }
}
