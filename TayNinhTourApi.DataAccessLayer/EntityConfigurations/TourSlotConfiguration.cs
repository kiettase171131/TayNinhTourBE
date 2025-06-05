using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho TourSlot entity
    /// Định nghĩa relationships, constraints và indexes
    /// </summary>
    public class TourSlotConfiguration : IEntityTypeConfiguration<TourSlot>
    {
        public void Configure(EntityTypeBuilder<TourSlot> builder)
        {
            // Table Configuration
            builder.ToTable("TourSlots");
            // Note: Date validation will be handled in application logic instead of DB constraint

            // Primary Key
            builder.HasKey(ts => ts.Id);

            // Property Configurations

            builder.Property(ts => ts.TourTemplateId)
                .IsRequired()
                .HasComment("ID của TourTemplate mà slot này được tạo từ");

            builder.Property(ts => ts.TourDate)
                .IsRequired()
                .HasColumnType("date")
                .HasComment("Ngày tour cụ thể sẽ diễn ra");

            builder.Property(ts => ts.ScheduleDay)
                .IsRequired()
                .HasConversion<int>()
                .HasComment("Ngày trong tuần của tour (Saturday hoặc Sunday)");

            builder.Property(ts => ts.Status)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(TourSlotStatus.Available)
                .HasComment("Trạng thái của tour slot");

            builder.Property(ts => ts.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Trạng thái slot có sẵn sàng để booking không");

            // Foreign Key Relationships

            // TourTemplate relationship (Many-to-One)
            builder.HasOne(ts => ts.TourTemplate)
                .WithMany(tt => tt.TourSlots)
                .HasForeignKey(ts => ts.TourTemplateId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // TourOperation relationship (One-to-One, Optional)
            // Configured from TourOperation side to avoid circular dependency

            // CreatedBy relationship
            builder.HasOne(ts => ts.CreatedBy)
                .WithMany(u => u.TourSlotsCreated)
                .HasForeignKey(ts => ts.CreatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // UpdatedBy relationship
            builder.HasOne(ts => ts.UpdatedBy)
                .WithMany(u => u.TourSlotsUpdated)
                .HasForeignKey(ts => ts.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes for Performance

            // Index for TourTemplateId (most common query)
            builder.HasIndex(ts => ts.TourTemplateId)
                .HasDatabaseName("IX_TourSlots_TourTemplateId");

            // Index for TourDate (for date-based queries)
            builder.HasIndex(ts => ts.TourDate)
                .HasDatabaseName("IX_TourSlots_TourDate");

            // Index for ScheduleDay (for weekend filtering)
            builder.HasIndex(ts => ts.ScheduleDay)
                .HasDatabaseName("IX_TourSlots_ScheduleDay");

            // Index for IsActive (for filtering active slots)
            builder.HasIndex(ts => ts.IsActive)
                .HasDatabaseName("IX_TourSlots_IsActive");

            // Composite index for TourTemplateId + TourDate (for template-specific date queries)
            builder.HasIndex(ts => new { ts.TourTemplateId, ts.TourDate })
                .HasDatabaseName("IX_TourSlots_TourTemplateId_TourDate")
                .IsUnique(); // Ensure one slot per template per date

            // Composite index for TourDate + IsActive (for available slots by date)
            builder.HasIndex(ts => new { ts.TourDate, ts.IsActive })
                .HasDatabaseName("IX_TourSlots_TourDate_IsActive");

            // Composite index for ScheduleDay + IsActive (for weekend availability)
            builder.HasIndex(ts => new { ts.ScheduleDay, ts.IsActive })
                .HasDatabaseName("IX_TourSlots_ScheduleDay_IsActive");


        }
    }
}
