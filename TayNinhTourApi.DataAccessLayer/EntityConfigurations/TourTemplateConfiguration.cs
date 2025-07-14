using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho TourTemplate entity
    /// Định nghĩa relationships, constraints và indexes
    /// </summary>
    public class TourTemplateConfiguration : IEntityTypeConfiguration<TourTemplate>
    {
        public void Configure(EntityTypeBuilder<TourTemplate> builder)
        {
            // Primary Key
            builder.HasKey(t => t.Id);

            // Properties Configuration
            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(200);



            builder.Property(t => t.TemplateType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.ScheduleDays)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.StartLocation)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(t => t.EndLocation)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(t => t.Month)
                .IsRequired();

            builder.Property(t => t.Year)
                .IsRequired();

            // Foreign Key Relationships

            // CreatedBy relationship (Required)
            builder.HasOne(t => t.CreatedBy)
                .WithMany(u => u.TourTemplatesCreated)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // UpdatedBy relationship (Optional)
            builder.HasOne(t => t.UpdatedBy)
                .WithMany(u => u.TourTemplatesUpdated)
                .HasForeignKey(t => t.UpdatedById)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Many-to-Many relationship với Images
            builder.HasMany(t => t.Images)
                .WithMany(i => i.TourTemplates)
                .UsingEntity<Dictionary<string, object>>(
                    "ImageTourTemplate",
                    j => j.HasOne<Image>().WithMany().HasForeignKey("ImagesId"),
                    j => j.HasOne<TourTemplate>().WithMany().HasForeignKey("TourTemplateId"),
                    j =>
                    {
                        j.HasKey("ImagesId", "TourTemplateId");
                        j.ToTable("ImageTourTemplate");
                    });

            // Indexes for Performance
            builder.HasIndex(t => t.TemplateType)
                .HasDatabaseName("IX_TourTemplate_TemplateType");

            builder.HasIndex(t => t.IsActive)
                .HasDatabaseName("IX_TourTemplate_IsActive");

            builder.HasIndex(t => t.CreatedById)
                .HasDatabaseName("IX_TourTemplate_CreatedById");

            builder.HasIndex(t => t.StartLocation)
                .HasDatabaseName("IX_TourTemplate_StartLocation");

            builder.HasIndex(t => t.EndLocation)
                .HasDatabaseName("IX_TourTemplate_EndLocation");

            builder.HasIndex(t => new { t.TemplateType, t.IsActive })
                .HasDatabaseName("IX_TourTemplate_TemplateType_IsActive");

            builder.HasIndex(t => new { t.Month, t.Year })
                .HasDatabaseName("IX_TourTemplate_Month_Year");

            builder.HasIndex(t => new { t.Year, t.Month, t.IsActive })
                .HasDatabaseName("IX_TourTemplate_Year_Month_IsActive");

            // Table Configuration
            builder.ToTable("TourTemplates");
        }
    }
}
