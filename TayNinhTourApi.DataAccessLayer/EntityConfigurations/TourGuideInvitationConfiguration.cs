using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Cấu hình Entity Framework cho TourGuideInvitation entity
    /// </summary>
    public class TourGuideInvitationConfiguration : IEntityTypeConfiguration<TourGuideInvitation>
    {
        public void Configure(EntityTypeBuilder<TourGuideInvitation> builder)
        {
            // Table Configuration
            builder.ToTable("TourGuideInvitations");

            // Primary Key
            builder.HasKey(i => i.Id);

            // Property Configurations

            builder.Property(i => i.TourDetailsId)
                .IsRequired()
                .HasComment("ID của TourDetails mà lời mời này thuộc về");

            builder.Property(i => i.GuideId)
                .IsRequired()
                .HasComment("ID của TourGuide được mời");

            builder.Property(i => i.InvitationType)
                .IsRequired()
                .HasComment("Loại lời mời (Automatic hoặc Manual)");

            builder.Property(i => i.Status)
                .IsRequired()
                .HasDefaultValue(InvitationStatus.Pending)
                .HasComment("Trạng thái lời mời");

            builder.Property(i => i.InvitedAt)
                .IsRequired()
                .HasComment("Thời gian gửi lời mời");

            builder.Property(i => i.RespondedAt)
                .IsRequired(false)
                .HasComment("Thời gian TourGuide phản hồi");

            builder.Property(i => i.ExpiresAt)
                .IsRequired()
                .HasComment("Thời gian hết hạn lời mời");

            builder.Property(i => i.RejectionReason)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("Ghi chú từ TourGuide khi từ chối lời mời");

            // Foreign Key Relationships

            // TourDetails relationship (Required)
            builder.HasOne(i => i.TourDetails)
                .WithMany()
                .HasForeignKey(i => i.TourDetailsId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // TourGuide relationship (Required)
            builder.HasOne(i => i.TourGuide)
                .WithMany(tg => tg.Invitations)
                .HasForeignKey(i => i.GuideId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // CreatedBy relationship
            builder.HasOne(i => i.CreatedBy)
                .WithMany()
                .HasForeignKey(i => i.CreatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // UpdatedBy relationship
            builder.HasOne(i => i.UpdatedBy)
                .WithMany()
                .HasForeignKey(i => i.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes for Performance

            // Index for TourDetailsId (most common query)
            builder.HasIndex(i => i.TourDetailsId)
                .HasDatabaseName("IX_TourGuideInvitations_TourDetailsId");

            // Index for GuideId (for guide's invitation list)
            builder.HasIndex(i => i.GuideId)
                .HasDatabaseName("IX_TourGuideInvitations_GuideId");

            // Index for Status (for filtering by status)
            builder.HasIndex(i => i.Status)
                .HasDatabaseName("IX_TourGuideInvitations_Status");

            // Index for ExpiresAt (for background job processing)
            builder.HasIndex(i => i.ExpiresAt)
                .HasDatabaseName("IX_TourGuideInvitations_ExpiresAt");

            // Composite index for TourDetailsId + GuideId (unique constraint)
            builder.HasIndex(i => new { i.TourDetailsId, i.GuideId })
                .HasDatabaseName("IX_TourGuideInvitations_TourDetailsId_GuideId")
                .IsUnique();

            // Composite index for Status + ExpiresAt (for expired invitations query)
            builder.HasIndex(i => new { i.Status, i.ExpiresAt })
                .HasDatabaseName("IX_TourGuideInvitations_Status_ExpiresAt");
        }
    }
}
