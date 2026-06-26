using Microsoft.EntityFrameworkCore;
using NotificationService.Models;

namespace NotificationService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProcessedMessage>(entity =>
            {
                entity.Property(x => x.MessageType).HasMaxLength(200);
                entity.Property(x => x.Status).HasMaxLength(50);
                entity.Property(x => x.Error).HasMaxLength(1000);
            });
        }
    }
}
