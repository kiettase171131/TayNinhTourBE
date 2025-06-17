using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Configurations
{
    /// <summary>
    /// Configuration cho TimelineItem entity
    /// </summary>
    public class TimelineItemConfiguration : IEntityTypeConfiguration<TimelineItem>
    {
        public void Configure(EntityTypeBuilder<TimelineItem> builder)
        {
            // Table name
            builder.ToTable("TimelineItem");

            // Primary key
            builder.HasKey(t => t.Id);

            // Properties configuration
            builder.Property(t => t.Id)
                .HasColumnType("CHAR(36)")
                .IsRequired();

            builder.Property(t => t.TourDetailsId)
                .HasColumnType("CHAR(36)")
                .IsRequired();

            builder.Property(t => t.CheckInTime)
                .HasColumnType("TIME")
                .IsRequired();

            builder.Property(t => t.Activity)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(t => t.SpecialtyShopId)
                .HasColumnType("CHAR(36)")
                .IsRequired(false);

            builder.Property(t => t.SortOrder)
                .IsRequired();

            // Audit fields from BaseEntity
            builder.Property(t => t.CreatedAt)
                .HasColumnType("DATETIME")
                .IsRequired();

            builder.Property(t => t.UpdatedAt)
                .HasColumnType("DATETIME")
                .IsRequired(false);

            builder.Property(t => t.CreatedById)
                .HasColumnType("CHAR(36)")
                .IsRequired();

            builder.Property(t => t.UpdatedById)
                .HasColumnType("CHAR(36)")
                .IsRequired(false);

            builder.Property(t => t.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            // Relationships
            builder.HasOne(t => t.TourDetails)
                .WithMany(td => td.Timeline)
                .HasForeignKey(t => t.TourDetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(t => t.SpecialtyShop)
                .WithMany(s => s.TimelineItems)
                .HasForeignKey(t => t.SpecialtyShopId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(t => t.CreatedBy)
                .WithMany()
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.UpdatedBy)
                .WithMany()
                .HasForeignKey(t => t.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(t => t.TourDetailsId)
                .HasDatabaseName("IX_TimelineItem_TourDetailsId");

            builder.HasIndex(t => new { t.TourDetailsId, t.SortOrder })
                .HasDatabaseName("IX_TimelineItem_TourDetailsId_SortOrder");

            builder.HasIndex(t => t.SpecialtyShopId)
                .HasDatabaseName("IX_TimelineItem_SpecialtyShopId");
        }
    }
}
