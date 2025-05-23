using Microsoft.EntityFrameworkCore;
using XLead_Server.Models;

namespace XLead_Server.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {

        }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<DealStage> DealStages { get; set; }
        public DbSet<Privilege> Privileges { get; set; }
        public DbSet<StageHistory> DealStageHistory { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserPrivilege> UserPrivileges { get; set; }
    }
}
