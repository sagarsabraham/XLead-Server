using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Diagnostics.Metrics;
using System.Net.Mail;
using XLead_Server.Interfaces;

namespace XLead_Server.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public long? AssignedTo { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User Creator { get; set; } 
        public User AssignedManager { get; set; }
        public ICollection<User> CreatedUsers { get; set; } = new List<User>();
        public ICollection<User> AssignedSubordinates { get; set; } = new List<User>();
        public ICollection<Account> CreatedAccounts { get; set; } = new List<Account>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>(); 
        public ICollection<Customer> Customers { get; set; } = new List<Customer>(); 
        public ICollection<Customer> UpdatedCustomers { get; set; } = new List<Customer>();
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>(); 
        public ICollection<Deal> Deals { get; set; } = new List<Deal>(); 
        
        public ICollection<DealStage> UpdatedDealStages { get; set; } = new List<DealStage>();
        public ICollection<Privilege> Privileges { get; set; } = new List<Privilege>(); 
        public ICollection<StageHistory> DealStageHistory { get; set; } = new List<StageHistory>(); 
        public ICollection<UserPrivilege> UserPrivileges { get; set; } = new List<UserPrivilege>();
        public ICollection<UserPrivilege> CreatedUserPrivileges { get; set; } = new List<UserPrivilege>();
        public ICollection<UserPrivilege> UpdatedUserPrivileges { get; set; } = new List<UserPrivilege>();
        public ICollection<Domain> CreatedDomains { get; set; } = new List<Domain>();
        public ICollection<DU> CreatedDUs { get; set; } = new List<DU>();
        public ICollection<Country> CreatedCountries { get; set; } = new List<Country>();
        public ICollection<IndustrialVertical> CreatedIndustrialVerticals { get; set; } = new List<IndustrialVertical>();
        public ICollection<ServiceLine> CreatedServiceLines { get; set; } = new List<ServiceLine>();
        public ICollection<RevenueType> CreatedRevenueTypes { get; set; } = new List<RevenueType>();
    }
}