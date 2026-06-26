using Microsoft.EntityFrameworkCore;
using UserService.Models;

namespace UserService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(x => x.UserId).IsUnique();
                entity.HasIndex(x => x.Email).IsUnique();

                entity.Property(x => x.UserId).HasMaxLength(50);
                entity.Property(x => x.FirstName).HasMaxLength(100);
                entity.Property(x => x.LastName).HasMaxLength(100);
                entity.Property(x => x.FullName).HasMaxLength(220);
                entity.Property(x => x.Email).HasMaxLength(256);
                entity.Property(x => x.PasswordHash).HasMaxLength(200);
                entity.Property(x => x.PasswordSalt).HasMaxLength(200);
                entity.Property(x => x.Role).HasMaxLength(20).HasDefaultValue("Patient");
                entity.Property(x => x.Specialization).HasMaxLength(100);
                entity.Property(x => x.Gender).HasMaxLength(10);
            });
        }
    }
}
