using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Contexts
{
    public class TayNinhTouApiDbContext(DbContextOptions<TayNinhTouApiDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<SupportTicket> SupportTickets { get; set; } = null!;
        public DbSet<SupportTicketComment> SupportTicketComments { get; set; } = null!;
        public DbSet<TourGuideApplication> TourGuideApplications { get; set; } = null!;
        public DbSet<Image> Images { get; set; } = null!;
        public DbSet<Tour> Tours { get; set; } = null!;
        public DbSet<TourTemplate> TourTemplates { get; set; } = null!;
        public DbSet<TourSlot> TourSlots { get; set; } = null!;
        public DbSet<TourDetails> TourDetails { get; set; } = null!;
        public DbSet<TourOperation> TourOperations { get; set; } = null!;
        public DbSet<Shop> Shops { get; set; } = null!;
        public DbSet<SupportTicketImage> SupportTicketImages { get; set; } = null!;
        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<BlogImage> BlogImages { get; set; } = null!;
        public DbSet<BlogReaction> BlogReactions { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TayNinhTouApiDbContext).Assembly);
            // Đảm bảo một user không thể reaction nhiều lần cho cùng 1 blog
            modelBuilder.Entity<BlogReaction>()
                .HasIndex(br => new { br.BlogId, br.UserId })
                .IsUnique();
        }

        public override int SaveChanges()
        {
            UpdateTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                var now = DateTime.UtcNow; // Use UTC for consistency

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                    entity.IsDeleted = false;
                    entity.IsActive = true;
                }
                entity.UpdatedAt = now;
            }
        }
    }
}
