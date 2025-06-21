using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity configuration cho TourGuideApplication
    /// Định nghĩa relationships, constraints, và indexes
    /// </summary>
    public class TourGuideApplicationConfiguration : IEntityTypeConfiguration<TourGuideApplication>
    {
        public void Configure(EntityTypeBuilder<TourGuideApplication> builder)
        {
            // Table name
            builder.ToTable("TourGuideApplications");

            // Primary key
            builder.HasKey(x => x.Id);

            // Properties configuration
            builder.Property(x => x.UserId)
                .IsRequired();

            builder.Property(x => x.FullName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Experience)
                .IsRequired();

            builder.Property(x => x.Languages)
                .HasMaxLength(200);

            builder.Property(x => x.CurriculumVitae)
                .HasMaxLength(500);

            builder.Property(x => x.CvOriginalFileName)
                .HasMaxLength(255);

            builder.Property(x => x.CvFileSize);

            builder.Property(x => x.CvContentType)
                .HasMaxLength(100);

            builder.Property(x => x.CvFilePath)
                .HasMaxLength(500);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasDefaultValue(TourGuideApplicationStatus.Pending);

            builder.Property(x => x.RejectionReason)
                .HasMaxLength(500);

            builder.Property(x => x.SubmittedAt)
                .IsRequired();

            builder.Property(x => x.ProcessedAt);

            builder.Property(x => x.ProcessedById);

            // Relationships
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.ProcessedBy)
                .WithMany()
                .HasForeignKey(x => x.ProcessedById)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_TourGuideApplications_UserId");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_TourGuideApplications_Status");

            builder.HasIndex(x => x.SubmittedAt)
                .HasDatabaseName("IX_TourGuideApplications_SubmittedAt");

            builder.HasIndex(x => x.Email)
                .HasDatabaseName("IX_TourGuideApplications_Email");

            // Composite index for common queries
            builder.HasIndex(x => new { x.UserId, x.Status })
                .HasDatabaseName("IX_TourGuideApplications_UserId_Status");
        }
    }
}
