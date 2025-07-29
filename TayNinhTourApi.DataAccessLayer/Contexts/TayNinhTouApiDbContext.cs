using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.DataAccessLayer.Contexts
{
    public class TayNinhTouApiDbContext(DbContextOptions<TayNinhTouApiDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<SupportTicket> SupportTickets { get; set; } = null!;
        public DbSet<SupportTicketComment> SupportTicketComments { get; set; } = null!;
        public DbSet<TourGuideApplication> TourGuideApplications { get; set; } = null!;
        public DbSet<TourGuide> TourGuides { get; set; } = null!;
        public DbSet<Image> Images { get; set; } = null!;
        public DbSet<Tour> Tours { get; set; } = null!;
        public DbSet<TourTemplate> TourTemplates { get; set; } = null!;
        public DbSet<TourSlot> TourSlots { get; set; } = null!;
        public DbSet<TourDetails> TourDetails { get; set; } = null!;
        public DbSet<TourDetailsSpecialtyShop> TourDetailsSpecialtyShops { get; set; } = null!;
        public DbSet<TourOperation> TourOperations { get; set; } = null!;
        public DbSet<TourGuideInvitation> TourGuideInvitations { get; set; } = null!;
        public DbSet<TimelineItem> TimelineItems { get; set; } = null!;

        public DbSet<SupportTicketImage> SupportTicketImages { get; set; } = null!;
        public DbSet<Blog> Blogs { get; set; } = null!;
        public DbSet<BlogImage> BlogImages { get; set; } = null!;
        public DbSet<BlogReaction> BlogReactions { get; set; }
        public DbSet<BlogComment> BlogComments { get; set; }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;

        public DbSet<SpecialtyShopApplication> SpecialtyShopApplications { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;

        public DbSet<SpecialtyShop> SpecialtyShops { get; set; } = null!;

        public DbSet<TourBooking> TourBookings { get; set; } = null!;
        public DbSet<TourCompany> TourCompanies { get; set; } = null!;

        public DbSet<ProductRating> ProductRatings { get; set; } = null!;
        public DbSet<ProductReview> ProductReviews { get; set; } = null!;
        public DbSet<Voucher> Vouchers { get; set; } = null!;
        public DbSet<VoucherCode> VoucherCodes { get; set; } = null!;

        // AI Chat entities
        public DbSet<AIChatSession> AIChatSessions { get; set; } = null!;
        public DbSet<AIChatMessage> AIChatMessages { get; set; } = null!;

        // Notification entities
        public DbSet<Notification> Notifications { get; set; } = null!;

        // Withdrawal system entities
        public DbSet<BankAccount> BankAccounts { get; set; } = null!;
        public DbSet<WithdrawalRequest> WithdrawalRequests { get; set; } = null!;

        // Tour booking refund system entities
        public DbSet<TourBookingRefund> TourBookingRefunds { get; set; } = null!;
        public DbSet<RefundPolicy> RefundPolicies { get; set; } = null!;

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
                var now = VietnamTimeZoneUtility.GetVietnamNow(); // Use Vietnam timezone

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
