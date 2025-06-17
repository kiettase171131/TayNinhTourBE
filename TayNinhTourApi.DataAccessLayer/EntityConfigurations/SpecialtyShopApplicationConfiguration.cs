using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho SpecialtyShopApplication entity
    /// Định nghĩa relationships, constraints và indexes
    /// </summary>
    public class SpecialtyShopApplicationConfiguration : IEntityTypeConfiguration<SpecialtyShopApplication>
    {
        public void Configure(EntityTypeBuilder<SpecialtyShopApplication> builder)
        {
            // Primary Key
            builder.HasKey(s => s.Id);

            // Required Properties Configuration
            builder.Property(s => s.UserId)
                .IsRequired();

            builder.Property(s => s.ShopName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.BusinessLicense)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Location)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(s => s.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.RepresentativeName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Status)
                .IsRequired()
                .HasDefaultValue(SpecialtyShopApplicationStatus.Pending);

            builder.Property(s => s.SubmittedAt)
                .IsRequired();

            // Optional Properties Configuration
            builder.Property(s => s.ShopDescription)
                .HasMaxLength(1000);

            builder.Property(s => s.Website)
                .HasMaxLength(200);

            builder.Property(s => s.ShopType)
                .HasMaxLength(50);

            builder.Property(s => s.OpeningHours)
                .HasMaxLength(100);

            builder.Property(s => s.BusinessLicenseUrl)
                .HasMaxLength(500);

            builder.Property(s => s.LogoUrl)
                .HasMaxLength(500);

            builder.Property(s => s.RejectionReason)
                .HasMaxLength(500);

            // Foreign Key Relationships

            // N:1 Relationship with User (Applicant)
            builder.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // N:1 Relationship with User (ProcessedBy Admin)
            builder.HasOne(s => s.ProcessedBy)
                .WithMany()
                .HasForeignKey(s => s.ProcessedById)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indexes for Performance
            builder.HasIndex(s => s.UserId)
                .HasDatabaseName("IX_SpecialtyShopApplication_UserId");

            builder.HasIndex(s => s.Status)
                .HasDatabaseName("IX_SpecialtyShopApplication_Status");

            builder.HasIndex(s => s.Email)
                .HasDatabaseName("IX_SpecialtyShopApplication_Email");

            builder.HasIndex(s => s.SubmittedAt)
                .HasDatabaseName("IX_SpecialtyShopApplication_SubmittedAt");

            builder.HasIndex(s => s.ProcessedAt)
                .HasDatabaseName("IX_SpecialtyShopApplication_ProcessedAt");

            // Composite index for admin queries
            builder.HasIndex(s => new { s.Status, s.SubmittedAt })
                .HasDatabaseName("IX_SpecialtyShopApplication_Status_SubmittedAt");

            // Table name
            builder.ToTable("SpecialtyShopApplications");
        }
    }
}
