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
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public User Creator { get; set; } // Navigation for CreatedBy
        public User AssignedManager { get; set; } // Navigation for AssignedTo
        public ICollection<User> CreatedUsers { get; set; } = new List<User>();
        public ICollection<User> AssignedSubordinates { get; set; } = new List<User>();
        public ICollection<Account> CreatedAccounts { get; set; } = new List<Account>();
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>(); // Renamed to match model
        public ICollection<Company> Companies { get; set; } = new List<Company>(); // CreatedCompanies
        public ICollection<Company> UpdatedCompanies { get; set; } = new List<Company>();
        public ICollection<Contact> Contacts { get; set; } = new List<Contact>(); // CreatedContacts
        public ICollection<Deal> Deals { get; set; } = new List<Deal>(); // CreatedDeals
        public ICollection<DealStage> DealStages { get; set; } = new List<DealStage>(); // CreatedDealStages
        public ICollection<DealStage> UpdatedDealStages { get; set; } = new List<DealStage>();
        public ICollection<Privilege> Privileges { get; set; } = new List<Privilege>(); // CreatedPrivileges
        public ICollection<StageHistory> DealStageHistory { get; set; } = new List<StageHistory>(); // CreatedStageHistories
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