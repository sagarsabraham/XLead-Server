namespace XLead_Server.Models
{
    public class Contact
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Designation { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public long CompanyId { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Company company { get; set; }
        public User Creator { get; set; }

        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    }
}