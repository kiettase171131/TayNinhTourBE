using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Cấu hình Entity Framework cho TourOperation entity
    /// </summary>
    public class TourOperationConfiguration : IEntityTypeConfiguration<TourOperation>
    {
        public void Configure(EntityTypeBuilder<TourOperation> builder)
        {
            // Table Configuration
            builder.ToTable("TourOperations", t =>
            {
                t.HasCheckConstraint("CK_TourOperations_Price_Positive", "Price > 0");
                t.HasCheckConstraint("CK_TourOperations_MaxGuests_Positive", "MaxGuests > 0");
            });

            // Primary Key
            builder.HasKey(to => to.Id);

            // Property Configurations
            
            builder.Property(to => to.TourSlotId)
                .IsRequired()
                .HasComment("ID của TourSlot mà operation này thuộc về");

            builder.Property(to => to.GuideId)
                .IsRequired()
                .HasComment("ID của User làm hướng dẫn viên cho tour này");

            builder.Property(to => to.Price)
                .IsRequired()
                .HasPrecision(18, 2) // Precision for decimal
                .HasComment("Giá tour cho operation này");

            builder.Property(to => to.MaxGuests)
                .IsRequired()
                .HasComment("Số lượng khách tối đa cho tour operation này");

            builder.Property(to => to.Description)
                .HasMaxLength(1000)
                .IsRequired(false)
                .HasComment("Mô tả bổ sung cho tour operation");

            builder.Property(to => to.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Trạng thái hoạt động của tour operation");

            // Foreign Key Relationships
            
            // TourSlot relationship (One-to-One)
            builder.HasOne(to => to.TourSlot)
                .WithOne(ts => ts.TourOperation) // One-to-One relationship with navigation property
                .HasForeignKey<TourOperation>(to => to.TourSlotId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Guide/User relationship (Many-to-One)
            builder.HasOne(to => to.Guide)
                .WithMany(u => u.TourOperationsAsGuide) // Many TourOperations can have the same Guide
                .HasForeignKey(to => to.GuideId)
                .OnDelete(DeleteBehavior.Restrict) // Prevent deleting User if they have TourOperations
                .IsRequired();

            // Indexes for Performance
            
            // Unique index for TourSlotId (ensures one-to-one relationship)
            builder.HasIndex(to => to.TourSlotId)
                .IsUnique()
                .HasDatabaseName("IX_TourOperations_TourSlotId_Unique");

            // Index for GuideId (for guide-related queries)
            builder.HasIndex(to => to.GuideId)
                .HasDatabaseName("IX_TourOperations_GuideId");

            // Index for IsActive (for filtering active operations)
            builder.HasIndex(to => to.IsActive)
                .HasDatabaseName("IX_TourOperations_IsActive");

            // Composite index for GuideId + IsActive (for active operations by guide)
            builder.HasIndex(to => new { to.GuideId, to.IsActive })
                .HasDatabaseName("IX_TourOperations_GuideId_IsActive");


        }
    }
}
