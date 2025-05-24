namespace XLead_Server.Models
{
    public class RevenueType
    {
        public int Id { get; set; }
        public string RevenueTypeName { get; set; }
        public ICollection<Deal> Deals { get; set; }
    }
}
