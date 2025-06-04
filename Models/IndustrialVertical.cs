namespace XLead_Server.Models
{
    public class IndustrialVertical
    {
        public long Id { get; set; }
        public string IndustryName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public User Creator { get; set; }
        public ICollection<Customer> Customers { get; set; }
    }
}