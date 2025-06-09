namespace XLead_Server.DTOs
{
    public class CustomerContactMapDto
    {
        public bool IsActive { get; set; }
        public bool? IsHidden { get; set; }
        public List<string> Contacts { get; set; } = new List<string>();
    }
}