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
                t.HasCheckConstraint("CK_TourOperations_CurrentBookings_NonNegative", "CurrentBookings >= 0");
                t.HasCheckConstraint("CK_TourOperations_CurrentBookings_LessOrEqualMaxGuests", "CurrentBookings <= MaxGuests");
            });

            // Primary Key
            builder.HasKey(to => to.Id);

            // Property Configurations

            builder.Property(to => to.TourDetailsId)
                .IsRequired()
                .HasComment("ID của TourDetails mà operation này thuộc về");

            builder.Property(to => to.GuideId)
                .IsRequired(false)
                .HasComment("ID của User làm hướng dẫn viên cho tour này (optional)");

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

            builder.Property(to => to.Notes)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("Ghi chú bổ sung cho tour operation");

            builder.Property(to => to.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Trạng thái hoạt động của tour operation");

            builder.Property(to => to.CurrentBookings)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Số lượng khách đã booking hiện tại");

            builder.Property(to => to.RowVersion)
                .IsRowVersion()
                .HasComment("Row version cho optimistic concurrency control");

            // Foreign Key Relationships

            // TourDetails relationship (One-to-One)
            builder.HasOne(to => to.TourDetails)
                .WithOne(td => td.TourOperation) // One-to-One relationship with navigation property
                .HasForeignKey<TourOperation>(to => to.TourDetailsId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Guide/User relationship (Many-to-One)
            builder.HasOne(to => to.Guide)
                .WithMany(u => u.TourOperationsAsGuide) // Many TourOperations can have the same Guide
                .HasForeignKey(to => to.GuideId)
                .OnDelete(DeleteBehavior.Restrict) // Prevent deleting User if they have TourOperations
                .IsRequired(false); // GuideId is optional - TourOperation can be created without a Guide

            // CreatedBy relationship
            builder.HasOne(to => to.CreatedBy)
                .WithMany(u => u.TourOperationsCreated)
                .HasForeignKey(to => to.CreatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // UpdatedBy relationship
            builder.HasOne(to => to.UpdatedBy)
                .WithMany(u => u.TourOperationsUpdated)
                .HasForeignKey(to => to.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // TourBookings relationship (One-to-Many)
            builder.HasMany(to => to.TourBookings)
                .WithOne(tb => tb.TourOperation)
                .HasForeignKey(tb => tb.TourOperationId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Indexes for Performance

            // Unique index for TourDetailsId (ensures one-to-one relationship)
            builder.HasIndex(to => to.TourDetailsId)
                .IsUnique()
                .HasDatabaseName("IX_TourOperations_TourDetailsId_Unique");

            // Index for GuideId (for guide-related queries)
            builder.HasIndex(to => to.GuideId)
                .HasDatabaseName("IX_TourOperations_GuideId");

            // Index for IsActive (for filtering active operations)
            builder.HasIndex(to => to.IsActive)
                .HasDatabaseName("IX_TourOperations_IsActive");

            // Composite index for GuideId + IsActive (for active operations by guide)
            builder.HasIndex(to => new { to.GuideId, to.IsActive })
                .HasDatabaseName("IX_TourOperations_GuideId_IsActive");

            // Index for CurrentBookings (for capacity queries)
            builder.HasIndex(to => to.CurrentBookings)
                .HasDatabaseName("IX_TourOperations_CurrentBookings");

            // Composite index for CurrentBookings + MaxGuests (for availability queries)
            builder.HasIndex(to => new { to.CurrentBookings, to.MaxGuests })
                .HasDatabaseName("IX_TourOperations_CurrentBookings_MaxGuests");

        }
    }
}
