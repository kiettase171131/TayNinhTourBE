using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

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

            builder.Property(td => td.TimeSlot)
                .IsRequired()
                .HasComment("Thời gian trong ngày cho hoạt động này");

            builder.Property(td => td.Location)
                .HasMaxLength(500)
                .HasComment("Địa điểm hoặc tên hoạt động");

            builder.Property(td => td.Description)
                .HasMaxLength(1000)
                .HasComment("Mô tả chi tiết về hoạt động");

            builder.Property(td => td.ShopId)
                .IsRequired(false)
                .HasComment("ID của shop liên quan (nếu có)");

            builder.Property(td => td.SortOrder)
                .IsRequired()
                .HasComment("Thứ tự sắp xếp trong timeline");

            // Foreign Key Relationships
            
            // TourTemplate relationship (Required)
            builder.HasOne(td => td.TourTemplate)
                .WithMany(tt => tt.TourDetails)
                .HasForeignKey(td => td.TourTemplateId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Shop relationship (Optional)
            builder.HasOne(td => td.Shop)
                .WithMany(s => s.TourDetails)
                .HasForeignKey(td => td.ShopId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

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

            // Composite index for TourTemplateId + SortOrder (for timeline ordering)
            builder.HasIndex(td => new { td.TourTemplateId, td.SortOrder })
                .HasDatabaseName("IX_TourDetails_TourTemplateId_SortOrder")
                .IsUnique(); // Ensure unique sort order within each tour template

            // Index for ShopId (for shop-related queries)
            builder.HasIndex(td => td.ShopId)
                .HasDatabaseName("IX_TourDetails_ShopId");

            // Index for TimeSlot (for time-based queries)
            builder.HasIndex(td => td.TimeSlot)
                .HasDatabaseName("IX_TourDetails_TimeSlot");

            // Composite index for TourTemplateId + TimeSlot (for timeline queries)
            builder.HasIndex(td => new { td.TourTemplateId, td.TimeSlot })
                .HasDatabaseName("IX_TourDetails_TourTemplateId_TimeSlot");

            // Check Constraints
            
            // Ensure SortOrder is positive
            builder.HasCheckConstraint("CK_TourDetails_SortOrder_Positive", 
                "SortOrder > 0");
        }
    }
}
