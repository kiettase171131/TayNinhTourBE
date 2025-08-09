using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Configurations
{
    /// <summary>
    /// Entity Framework configuration for TourSlotTimelineProgress entity
    /// </summary>
    public class TourSlotTimelineProgressConfiguration : IEntityTypeConfiguration<TourSlotTimelineProgress>
    {
        public void Configure(EntityTypeBuilder<TourSlotTimelineProgress> builder)
        {
            // Table configuration
            builder.ToTable("TourSlotTimelineProgress");
            builder.HasComment("Tracks timeline completion progress for individual tour slots");

            // Primary key
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnType("char(36)")
                .IsRequired()
                .HasComment("Primary key identifier");

            // Required properties
            builder.Property(e => e.TourSlotId)
                .HasColumnType("char(36)")
                .IsRequired()
                .HasComment("Reference to the specific tour slot");

            builder.Property(e => e.TimelineItemId)
                .HasColumnType("char(36)")
                .IsRequired()
                .HasComment("Reference to the timeline item template");

            builder.Property(e => e.IsCompleted)
                .HasDefaultValue(false)
                .IsRequired()
                .HasComment("Whether this timeline item has been completed for this tour slot");

            // Optional properties
            builder.Property(e => e.CompletedAt)
                .HasColumnType("datetime")
                .HasComment("Timestamp when the timeline item was completed");

            builder.Property(e => e.CompletionNotes)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)")
                .HasComment("Optional notes added when completing the timeline item");

            // Base entity properties
            builder.Property(e => e.CreatedAt)
                .HasColumnType("datetime")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired()
                .HasComment("Timestamp when the record was created");

            builder.Property(e => e.CreatedById)
                .HasColumnType("char(36)")
                .IsRequired()
                .HasComment("ID of the user who created this record");

            builder.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasComment("Timestamp when the record was last updated");

            builder.Property(e => e.UpdatedById)
                .HasColumnType("char(36)")
                .HasComment("ID of the user who last updated this record");

            builder.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .IsRequired()
                .HasComment("Soft delete flag");

            // Unique constraint - one progress record per TourSlot-TimelineItem pair
            builder.HasIndex(e => new { e.TourSlotId, e.TimelineItemId })
                .IsUnique()
                .HasDatabaseName("UK_TourSlotTimeline");

            // Performance indexes
            builder.HasIndex(e => e.TourSlotId)
                .HasDatabaseName("IX_TourSlotTimelineProgress_TourSlotId");

            builder.HasIndex(e => e.TimelineItemId)
                .HasDatabaseName("IX_TourSlotTimelineProgress_TimelineItemId");

            builder.HasIndex(e => e.IsCompleted)
                .HasDatabaseName("IX_TourSlotTimelineProgress_IsCompleted");

            builder.HasIndex(e => e.CompletedAt)
                .HasDatabaseName("IX_TourSlotTimelineProgress_CompletedAt");

            builder.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_TourSlotTimelineProgress_CreatedAt");

            // Composite index for common queries
            builder.HasIndex(e => new { e.TourSlotId, e.IsCompleted, e.CompletedAt })
                .HasDatabaseName("IX_TourSlotTimelineProgress_TourSlot_Completed");

            // Foreign key relationships
            builder.HasOne(e => e.TourSlot)
                .WithMany(ts => ts.TimelineProgress)
                .HasForeignKey(e => e.TourSlotId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TourSlotTimelineProgress_TourSlot");

            builder.HasOne(e => e.TimelineItem)
                .WithMany(ti => ti.SlotProgress)
                .HasForeignKey(e => e.TimelineItemId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TourSlotTimelineProgress_TimelineItem");

            builder.HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TourSlotTimelineProgress_CreatedBy");

            builder.HasOne(e => e.UpdatedBy)
                .WithMany()
                .HasForeignKey(e => e.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_TourSlotTimelineProgress_UpdatedBy");

            // Query filters for soft delete
            builder.HasQueryFilter(e => e.IsActive);

            // Value conversions if needed
            builder.Property(e => e.CreatedAt)
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            builder.Property(e => e.UpdatedAt)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            builder.Property(e => e.CompletedAt)
                .HasConversion(
                    v => v.HasValue ? v.Value.ToUniversalTime() : (DateTime?)null,
                    v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            // Validation constraints
            builder.ToTable(t => t.HasCheckConstraint(
                "CK_TourSlotTimelineProgress_Completion_Logic",
                "(IsCompleted = FALSE AND CompletedAt IS NULL) OR (IsCompleted = TRUE AND CompletedAt IS NOT NULL)"));

            // Seed data or default values can be configured here if needed
            // builder.HasData(...);
        }
    }
}
