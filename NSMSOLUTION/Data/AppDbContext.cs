using NSMSOLUTION.Models;
using System.Data.Entity;

namespace NSMSOLUTION.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor for EF6
        public AppDbContext() : base("name=AuthContextNSM")
        {
            // Optional: you can disable lazy loading or proxy creation here
            // this.Configuration.LazyLoadingEnabled = false;
            // this.Configuration.ProxyCreationEnabled = false;
        }

        public DbSet<AspNetUser> Users { get; set; }

        // EF6 uses DbModelBuilder instead of ModelBuilder
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Map User entity to AspNetUsers table
            modelBuilder.Entity<AspNetUser>()
                .ToTable("AspNetUsers") // Table name in DB
                .HasKey(u => u.Id);     // Primary key

            // Example: if you want to map property names to DB columns
            // modelBuilder.Entity<User>()
            //     .Property(u => u.UserName)
            //     .HasColumnName("UserNameColumnInDb");
        }
    }
}
