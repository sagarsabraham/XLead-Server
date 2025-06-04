namespace XLead_Server.Models
{
    public class Privilege
    {
        public long Id { get; set; }
        public string PrivilegeName { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public User Creator { get; set; }
        public ICollection<UserPrivilege> UserPrivileges { get; set; } = new List<UserPrivilege>();
    }
}