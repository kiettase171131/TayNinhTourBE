using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Cấu hình Entity Framework cho TourDetails entity
    /// </summary>
    public class TourDetailsConfiguration : IEntityTypeConfiguration<TourDetails>
    {
        public void Configure(EntityTypeBuilder<TourDetails> builder)
        {
            // Table Configuration
            builder.ToTable("TourDetails");

            // Primary Key
            builder.HasKey(td => td.Id);

            // Property Configurations

            builder.Property(td => td.TourTemplateId)
                .IsRequired()
                .HasComment("ID của tour template mà chi tiết này thuộc về");

            builder.Property(td => td.Title)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("Tiêu đề của lịch trình");

            builder.Property(td => td.Description)
                .HasMaxLength(1000)
                .HasComment("Mô tả về lịch trình này");

            builder.Property(td => td.Status)
                .IsRequired()
                .HasDefaultValue(TourDetailsStatus.Pending)
                .HasComment("Trạng thái duyệt của tour details");

            builder.Property(td => td.CommentApproved)
                .HasMaxLength(1000)
                .IsRequired(false)
                .HasComment("Bình luận từ admin khi duyệt/từ chối tour details");

            builder.Property(td => td.SkillsRequired)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("Kỹ năng yêu cầu cho hướng dẫn viên (comma-separated)");

            // Foreign Key Relationships

            // TourTemplate relationship (Required)
            builder.HasOne(td => td.TourTemplate)
                .WithMany(tt => tt.TourDetails)
                .HasForeignKey(td => td.TourTemplateId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // TourOperation relationship (One-to-One)
            builder.HasOne(td => td.TourOperation)
                .WithOne(to => to.TourDetails)
                .HasForeignKey<TourOperation>(to => to.TourDetailsId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false);

            // Timeline relationship (One-to-Many)
            builder.HasMany(td => td.Timeline)
                .WithOne(ti => ti.TourDetails)
                .HasForeignKey(ti => ti.TourDetailsId)
                .OnDelete(DeleteBehavior.Cascade);

            // AssignedSlots relationship (One-to-Many)
            builder.HasMany(td => td.AssignedSlots)
                .WithOne(ts => ts.TourDetails)
                .HasForeignKey(ts => ts.TourDetailsId)
                .OnDelete(DeleteBehavior.SetNull);

            // CreatedBy relationship
            builder.HasOne(td => td.CreatedBy)
                .WithMany(u => u.TourDetailsCreated)
                .HasForeignKey(td => td.CreatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // UpdatedBy relationship
            builder.HasOne(td => td.UpdatedBy)
                .WithMany(u => u.TourDetailsUpdated)
                .HasForeignKey(td => td.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes for Performance

            // Index for TourTemplateId (most common query)
            builder.HasIndex(td => td.TourTemplateId)
                .HasDatabaseName("IX_TourDetails_TourTemplateId");

            // Index for Title (for searching by title)
            builder.HasIndex(td => td.Title)
                .HasDatabaseName("IX_TourDetails_Title");

            // Index for Status (for filtering by approval status)
            builder.HasIndex(td => td.Status)
                .HasDatabaseName("IX_TourDetails_Status");
        }
    }
}
