using System.Numerics;

namespace XLead_Server.Models
{
    public class Company
    {
        public long Id { get; set; }
        public string CompanyName { get; set; }
        //public string IndustryVerticalId { get; set; }
        public string Website { get; set; }
        public string CompanyPhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<Contact> Contacts { get; set; }
    }
}