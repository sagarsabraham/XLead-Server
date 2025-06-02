namespace XLead_Server.Models
{
    public class DU
    {
        public long Id { get; set; }
        public string DUName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User Creator { get; set; }
        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    }
}