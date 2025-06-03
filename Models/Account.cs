namespace XLead_Server.Models
{
    public class Account
    {
        public long Id { get; set; }
        public string AccountName { get; set; }
        public long CreatedBy { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation properties
        public User Creator { get; set; }
        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    }
}