namespace XLead_Server.Models
{
    public class Region
    {
        public int Id { get; set; }
        public string RegionName { get; set; }
        public ICollection<Deal> Deals { get; set; }
    }
}
