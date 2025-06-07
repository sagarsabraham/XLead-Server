namespace XLead_Server.Models
{
    public class Customer
    {
        public long Id { get; set; }
        public string CustomerName { get; set; }
        public long IndustryVerticalId { get; set; }
        public string Website { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Boolean ? IsHidden { get; set; }

        // Navigation properties
        public User Creator { get; set; }
        public User Updater { get; set; }
        public IndustrialVertical IndustrialVertical { get; set; }
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    }
}