using Microsoft.EntityFrameworkCore;
using ProductStorageAPI.Models;

namespace ProductStorageAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(x =>
            {
                x.HasKey(p => p.Id);
                x.Property(p => p.Name).IsRequired();
                x.Property(p => p.Price).IsRequired();
                x.Property(p => p.Amount).IsRequired();
                x.HasIndex(p => new { p.UserId, p.Name }).IsUnique();
                x.HasOne(p => p.User)
                    .WithMany(u => u.Products)
                    .HasForeignKey(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<User>(x =>
            {
                x.HasKey(u => u.Id);
                x.Property(u => u.FullName).IsRequired();
                x.Property(u => u.Email).IsRequired();
                x.Property(u => u.PasswordHash).IsRequired();
                x.HasIndex(u => u.Email).IsUnique();
            });
        }
    }
}
