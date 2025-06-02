using System.Collections.Generic;

namespace XLead_Server.Models 
{
    public class ServiceLine
    {
        public long Id { get; set; }
        public string ServiceName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public User Creator { get; set; }
        public ICollection<Deal> Deals { get; set; }
    }
}