using System.Collections.Generic;

namespace XLead_Server.Models 
{ 
public class RevenueType
    {
        public long Id { get; set; }
        public string RevenueTypeName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public User Creator { get; set; } // Added property to fix CS1061  
        public ICollection<Deal> Deals { get; set; }
    }
}