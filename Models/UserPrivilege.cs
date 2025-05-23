using System.Numerics;

namespace XLead_Server.Models
{
    public class UserPrivilege
    {
        public long Id { get; set; }
        public long PrivilegeId { get; set; }
        public long UserId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long UpdatedBy { get; set;}
        public DateTime UpdatedAt { get; set; }
        public User User { get; set; }
        public Privilege Privilege { get; set; }
    }
}
