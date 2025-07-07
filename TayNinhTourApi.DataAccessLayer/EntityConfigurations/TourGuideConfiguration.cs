using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration for TourGuide entity
    /// Defines table structure, relationships, constraints, and indexes
    /// </summary>
    public class TourGuideConfiguration : IEntityTypeConfiguration<TourGuide>
    {
        public void Configure(EntityTypeBuilder<TourGuide> builder)
        {
            // Table name
            builder.ToTable("TourGuides");

            // Primary key
            builder.HasKey(tg => tg.Id);

            // Unique constraints
            builder.HasIndex(tg => tg.UserId)
                .IsUnique()
                .HasDatabaseName("IX_TourGuides_UserId_Unique");

            builder.HasIndex(tg => tg.ApplicationId)
                .IsUnique()
                .HasDatabaseName("IX_TourGuides_ApplicationId_Unique");

            // Property configurations
            builder.Property(tg => tg.UserId)
                .IsRequired()
                .HasComment("Foreign Key to User table - One-to-One relationship");

            builder.Property(tg => tg.ApplicationId)
                .IsRequired()
                .HasComment("Foreign Key to approved TourGuideApplication");

            builder.Property(tg => tg.FullName)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Full name of the tour guide");

            builder.Property(tg => tg.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20)
                .HasComment("Contact phone number");

            builder.Property(tg => tg.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Contact email address");

            builder.Property(tg => tg.Experience)
                .IsRequired()
                .HasMaxLength(1000)
                .HasComment("Tour guide experience description");

            builder.Property(tg => tg.Skills)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("Tour guide skills (comma-separated TourGuideSkill enum values)");

            builder.Property(tg => tg.Rating)
                .HasPrecision(3, 2)
                .HasDefaultValue(0.00m)
                .HasComment("Average rating from tour participants");

            builder.Property(tg => tg.TotalToursGuided)
                .HasDefaultValue(0)
                .HasComment("Total number of tours guided");

            builder.Property(tg => tg.IsAvailable)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Whether the tour guide is currently available for new tours");

            builder.Property(tg => tg.Notes)
                .HasMaxLength(1000)
                .IsRequired(false)
                .HasComment("Additional notes about the tour guide");

            builder.Property(tg => tg.ProfileImageUrl)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("Tour guide's profile image URL");

            builder.Property(tg => tg.ApprovedAt)
                .IsRequired()
                .HasComment("Date when the tour guide was approved and became active");

            builder.Property(tg => tg.ApprovedById)
                .IsRequired()
                .HasComment("ID of the admin who approved this tour guide");

            // Foreign Key Relationships

            // One-to-One relationship with User
            builder.HasOne(tg => tg.User)
                .WithOne(u => u.TourGuide)
                .HasForeignKey<TourGuide>(tg => tg.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Many-to-One relationship with TourGuideApplication
            builder.HasOne(tg => tg.Application)
                .WithOne(app => app.ApprovedTourGuide)
                .HasForeignKey<TourGuide>(tg => tg.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // Many-to-One relationship with User (ApprovedBy)
            builder.HasOne(tg => tg.ApprovedBy)
                .WithMany(u => u.ApprovedTourGuides)
                .HasForeignKey(tg => tg.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // One-to-Many relationship with TourOperations
            builder.HasMany(tg => tg.TourOperations)
                .WithOne(to => to.TourGuide)
                .HasForeignKey(to => to.TourGuideId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // One-to-Many relationship with TourGuideInvitations
            builder.HasMany(tg => tg.Invitations)
                .WithOne(inv => inv.TourGuide)
                .HasForeignKey(inv => inv.GuideId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // Performance indexes
            builder.HasIndex(tg => tg.IsAvailable)
                .HasDatabaseName("IX_TourGuides_IsAvailable");

            builder.HasIndex(tg => tg.Rating)
                .HasDatabaseName("IX_TourGuides_Rating");

            builder.HasIndex(tg => tg.ApprovedAt)
                .HasDatabaseName("IX_TourGuides_ApprovedAt");

            builder.HasIndex(tg => tg.ApprovedById)
                .HasDatabaseName("IX_TourGuides_ApprovedById");

            // Composite index for common queries
            builder.HasIndex(tg => new { tg.IsAvailable, tg.Rating })
                .HasDatabaseName("IX_TourGuides_Available_Rating");
        }
    }
}
