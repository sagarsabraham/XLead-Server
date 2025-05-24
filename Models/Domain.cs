namespace XLead_Server.Models
{
    public class Domain
    {
        public int Id { get; set; }
        public string DomainName { get; set; }
        public ICollection<Deal> Deals { get; set; }
    }
}
