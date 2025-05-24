namespace XLead_Server.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string CountryName { get; set; }
        public ICollection<Deal> Deals { get; set; }
    }
}
