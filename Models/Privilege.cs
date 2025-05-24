using System.Numerics;

namespace XLead_Server.Models
{
    public class Privilege
    {
        public long Id { get; set; }
        public string PrivilegeName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<UserPrivilege> UserPrivileges { get; set; }
    }
}
