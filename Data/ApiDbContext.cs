// XLead_Server/Data/ApiDbContext.cs
using Microsoft.EntityFrameworkCore;
using XLead_Server.Models;

namespace XLead_Server.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<Deal> Deals { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Domain> Domains { get; set; }
        public DbSet<RevenueType> RevenueTypes { get; set; }
        public DbSet<DU> DUs { get; set; } // Delivery Units
        public DbSet<Country> Countries { get; set; }
        public DbSet<DealStage> DealStages { get; set; }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure potential unique constraints or cascade delete behaviors if needed
            // Example: CompanyName should ideally be unique
            modelBuilder.Entity<Company>()
                .HasIndex(c => c.CompanyName)
                .IsUnique();

            // Example: Contact (FirstName, LastName, CompanyId) combination should be unique
            modelBuilder.Entity<Contact>()
                .HasIndex(c => new { c.FirstName, c.LastName, c.CompanyId })
                .IsUnique();

            // Configure relationships (EF Core conventions handle many, but explicit is clearer)
            modelBuilder.Entity<Deal>()
                .HasOne(d => d.Contact)
                .WithMany(c => c.Deals)
                .HasForeignKey(d => d.ContactId)
                .OnDelete(DeleteBehavior.Restrict); // Or Cascade if appropriate

            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Company)
                .WithMany(co => co.Contacts)
                .HasForeignKey(c => c.CompanyId)
                .OnDelete(DeleteBehavior.Restrict); // Or Cascade

            // Soft delete query filters (if you implement IsDeleted flag)
            // modelBuilder.Entity<Company>().HasQueryFilter(c => !c.IsDeleted);
        }
    }
}