using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

namespace XLead_Server.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }

        // Self-referencing relationship
        public long? CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public User CreatedByUser { get; set; }

        // Reverse navigation property
        public ICollection<User> CreatedUsers { get; set; }

        public long? AssignedTo { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<Attachment> Attachments { get; set; }
        public ICollection<Company> Companies { get; set; }
        public ICollection<Contact> Contacts { get; set; }
        public ICollection<Deal> Deals { get; set; }
        public ICollection<DealStage> DealStages { get; set; }
        public ICollection<Privilege> Privileges { get; set; }
        public ICollection<StageHistory> DealStageHistory { get; set; }
    }
}
