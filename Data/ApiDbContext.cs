using System.Diagnostics.Metrics;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using XLead_Server.Interfaces;
using XLead_Server.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using Attachment = XLead_Server.Models.Attachment;

namespace XLead_Server.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<Privilege> Privileges { get; set; }
        public DbSet<UserPrivilege> UsersPrivileges { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Domain> Domains { get; set; }
        public DbSet<RevenueType> RevenueTypes { get; set; }
        public DbSet<DU> DUs { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<DealStage> DealStages { get; set; }
        public DbSet<StageHistory> StageHistories { get; set; }
        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<IndustrialVertical> IndustrialVerticals { get; set; }
        public DbSet<ServiceLine> ServiceLines { get; set; }
        public DbSet<Note> Notes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User Relationships
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasOne(u => u.Creator)
                .WithMany(u => u.CreatedUsers)
                .HasForeignKey(u => u.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.AssignedManager)
                .WithMany(u => u.AssignedSubordinates)
                .HasForeignKey(u => u.AssignedTo)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // Account Relationships
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasMany(a => a.Deals)
                .WithOne(d => d.account)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Creator)
                .WithMany(u => u.CreatedAccounts)
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            //Note Relationships
            modelBuilder.Entity<Note>(entity =>
            {
                entity.HasOne(n => n.Deal)
                .WithMany(d => d.Notes)
                .HasForeignKey(n => n.DealId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(n => n.Creator)
                    .WithMany(u => u.CreatedNotes)
                    .HasForeignKey(n => n.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Updater)
                    .WithMany(u => u.UpdatedNotes)
                    .HasForeignKey(n => n.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Attachment Relationships
            modelBuilder.Entity<Models.Attachment>(entity =>
            {
                entity.HasOne(a => a.Deal)
                .WithMany(d => d.Attachments)
                .HasForeignKey(a => a.DealId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Creator)
                .WithMany(u => u.Attachments)
                .HasForeignKey(a => a.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // Company Relationships
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasMany(c => c.Contacts)
                .WithOne(c => c.customer)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Creator)
                .WithMany(u => u.Customers)
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Updater)
                .WithMany(u => u.UpdatedCustomers)
                .HasForeignKey(c => c.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);

                entity.Property(c => c.IndustryVerticalId);
                entity.HasOne(c => c.IndustrialVertical)
                .WithMany(iv => iv.Customers)
                .HasForeignKey(c => c.IndustryVerticalId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // Contact Relationships
            modelBuilder.Entity<Contact>(entity =>
            {
                entity.HasMany(c => c.Deals)
                .WithOne(d => d.contact)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.customer)
                .WithMany(c => c.Contacts)
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Creator)
                .WithMany(u => u.Contacts)
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // Deal Relationships
            modelBuilder.Entity<Deal>(entity =>
            {
                entity.HasOne(d => d.account)
                .WithMany(a => a.Deals)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.region)
                .WithMany(r => r.Deals)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.domain)
                .WithMany(dm => dm.Deals)
                .HasForeignKey(d => d.DomainId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.dealStage)
                .WithMany(ds => ds.Deals)
                .HasForeignKey(d => d.DealStageId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.revenueType)
                .WithMany(rt => rt.Deals)
                .HasForeignKey(d => d.RevenueTypeId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.du)
                .WithMany(du => du.Deals)
                .HasForeignKey(d => d.DuId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.country)
                .WithMany(c => c.Deals)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.contact)
                .WithMany(c => c.Deals)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.serviceLine)
                .WithMany(sl => sl.Deals)
                .HasForeignKey(d => d.ServiceLineId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Creator)
                .WithMany(u => u.Deals)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.DealStageHistory)
                .WithOne(sh => sh.Deal)
                .HasForeignKey(sh => sh.DealId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.Property(d => d.DealAmount).HasPrecision(18, 2);
                entity.Property(d => d.Probability).HasPrecision(5, 2);
            });

            // DealStage Relationships
            modelBuilder.Entity<DealStage>(entity =>
            {
                entity.HasMany(ds => ds.DealStageHistories)
                .WithOne(sh => sh.DealStage)
                .HasForeignKey(sh => sh.DealStageId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                .WithMany(u => u.CreatedDealStages)
                .HasForeignKey(ds => ds.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                .WithMany(u => u.UpdatedDealStages)
                .HasForeignKey(ds => ds.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // Domain Relationships
            modelBuilder.Entity<Domain>(entity =>
            {
                entity.HasOne(d => d.Creator)
                .WithMany(u => u.CreatedDomains)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // DU Relationships
            modelBuilder.Entity<DU>(entity =>
            {
                entity.HasOne(d => d.Creator)
                .WithMany(u => u.CreatedDUs)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // Country Relationships
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasOne(c => c.Creator)
                .WithMany(u => u.CreatedCountries)
                .HasForeignKey(c => c.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // IndustrialVertical Relationships
            modelBuilder.Entity<IndustrialVertical>(entity =>
            {
                entity.HasOne(iv => iv.Creator)
                .WithMany(u => u.CreatedIndustrialVerticals)
                .HasForeignKey(iv => iv.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // Privilege Relationships
            modelBuilder.Entity<Privilege>(entity =>
            {
                entity.HasOne(p => p.Creator)
                .WithMany(u => u.Privileges)
                .HasForeignKey(p => p.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // UserPrivilege Relationships
            modelBuilder.Entity<UserPrivilege>(entity =>
            {
                entity.HasKey(up => new { up.UserId, up.PrivilegeId });

                entity.HasOne(up => up.User)
                .WithMany(u => u.UserPrivileges)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(up => up.Privilege)
                .WithMany(p => p.UserPrivileges)
                .HasForeignKey(up => up.PrivilegeId)
                .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(up => up.Creator)
                .WithMany(u => u.CreatedUserPrivileges)
                .HasForeignKey(up => up.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(up => up.Updater)
                .WithMany(u => u.UpdatedUserPrivileges)
                .HasForeignKey(up => up.UpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // StageHistory Relationships
            modelBuilder.Entity<StageHistory>(entity =>
            {
                entity.HasOne(sh => sh.DealStage)
                .WithMany(ds => ds.DealStageHistories)
                .HasForeignKey(sh => sh.DealStageId)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sh => sh.Creator)
                .WithMany(u => u.DealStageHistory)
                .HasForeignKey(sh => sh.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // ServiceLine Relationships
            modelBuilder.Entity<ServiceLine>(entity =>
            {
                entity.HasOne(sl => sl.Creator)
                .WithMany(u => u.CreatedServiceLines)
                .HasForeignKey(sl => sl.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(sl => sl.Deals)
                .WithOne(d => d.serviceLine)
                .HasForeignKey(d => d.ServiceLineId)
                .OnDelete(DeleteBehavior.Restrict);
            });

            // RevenueType Relationships
            modelBuilder.Entity<RevenueType>(entity =>
            {
                entity.HasOne(rt => rt.Creator)
                .WithMany(u => u.CreatedRevenueTypes)
                .HasForeignKey(rt => rt.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(rt => rt.Deals)
                .WithOne(d => d.revenueType)
                .HasForeignKey(d => d.RevenueTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            });
            //  Configure 
             // Ensure DealStage does not infer additional relationships

        }
    }
}