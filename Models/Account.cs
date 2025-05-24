namespace XLead_Server.Models
{
    public class Account
    {
        public int Id { get; set; }
        public string AccountName { get; set; }
        public ICollection<Deal> Deals { get; set; }
    }
}
