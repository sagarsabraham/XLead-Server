namespace XLead_Server.Models
{
    public class Region
    {
        public long Id { get; set; }
        public string RegionName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Deal> Deals { get; set; } = new List<Deal>();
    }
}
