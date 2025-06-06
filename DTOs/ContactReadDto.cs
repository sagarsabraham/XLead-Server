// XLead_Server/DTOs/ContactReadDto.cs
namespace XLead_Server.DTOs
{
    public class ContactReadDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string? LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Designation { get; set; } // Added to match Contact model
        public long CustomerId { get; set; }
        public bool IsActive { get; set; } // Added to reflect Contact model
    }
}