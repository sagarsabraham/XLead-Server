namespace XLead_Server.DTOs
{
    public class ContactUpdateDto
    {

        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? Designation { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int UpdatedBy { get; set; }


    }
}