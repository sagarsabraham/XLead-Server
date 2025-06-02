namespace XLead_Server.Models
{
    public class IndustrialVertical
    {
        public long Id { get; set; }
        public string IndustryName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User Creator { get; set; }
        public ICollection<Company> Companies { get; set; }
    }
}