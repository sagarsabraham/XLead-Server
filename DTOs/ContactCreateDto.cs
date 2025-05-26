namespace XLead_Server.DTOs
{
    public class ContactCreateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public long CompanyId { get; set; }
        public long CreatedBy { get; set; }

        public string CompanyName { get; set; } 

    }
}
