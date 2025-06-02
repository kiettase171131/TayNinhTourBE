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

            builder.Property(t => t.Description)
                .HasMaxLength(2000);

            builder.Property(t => t.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.MaxGuests)
                .IsRequired();

            builder.Property(t => t.Duration)
                .IsRequired()
                .HasColumnType("decimal(5,2)");

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

            builder.Property(t => t.SpecialRequirements)
                .HasMaxLength(1000);

            builder.Property(t => t.MinGuests)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(t => t.ChildPrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(t => t.ChildMaxAge);

            builder.Property(t => t.Transportation)
                .HasMaxLength(200);

            builder.Property(t => t.MealsIncluded)
                .HasMaxLength(500);

            builder.Property(t => t.AccommodationInfo)
                .HasMaxLength(500);

            builder.Property(t => t.IncludedServices)
                .HasMaxLength(1000);

            builder.Property(t => t.ExcludedServices)
                .HasMaxLength(1000);

            builder.Property(t => t.CancellationPolicy)
                .HasMaxLength(1000);

            // Foreign Key Relationships
            
            // CreatedBy relationship
            builder.HasOne(t => t.CreatedBy)
                .WithMany(u => u.TourTemplatesCreated)
                .HasForeignKey(t => t.CreatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // UpdatedBy relationship
            builder.HasOne(t => t.UpdatedBy)
                .WithMany(u => u.TourTemplatesUpdated)
                .HasForeignKey(t => t.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict)
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

            builder.HasIndex(t => new { t.Price, t.IsActive })
                .HasDatabaseName("IX_TourTemplate_Price_IsActive");

            // Table Configuration
            builder.ToTable("TourTemplates");
        }
    }
}
