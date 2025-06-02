namespace XLead_Server.Models
{
    public class Domain
    {
        public long Id { get; set; }
        public string DomainName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User Creator { get; set; }
        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    }
}
