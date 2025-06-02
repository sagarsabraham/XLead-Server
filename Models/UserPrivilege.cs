namespace XLead_Server.Models
{
    public class UserPrivilege
    {
        public long UserId { get; set; }
        public long PrivilegeId { get; set; }
        public long CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Privilege Privilege { get; set; }
        public User Creator { get; set; }
        public User Updater { get; set; }
    }
}