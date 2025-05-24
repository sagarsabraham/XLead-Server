namespace XLead_Server.Models
{
    public class DU
    {
        public int Id { get; set; }
        public string DUName { get; set; }
        public ICollection<Deal> Deals { get; set; }
    }
}
