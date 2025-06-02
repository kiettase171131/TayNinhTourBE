using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho Shop entity
    /// Định nghĩa relationships, constraints và indexes
    /// </summary>
    public class ShopConfiguration : IEntityTypeConfiguration<Shop>
    {
        public void Configure(EntityTypeBuilder<Shop> builder)
        {
            // Primary Key
            builder.HasKey(s => s.Id);

            // Properties Configuration
            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Description)
                .HasMaxLength(1000);

            builder.Property(s => s.Location)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(s => s.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(s => s.Email)
                .HasMaxLength(100);

            builder.Property(s => s.Website)
                .HasMaxLength(200);

            builder.Property(s => s.OpeningHours)
                .HasMaxLength(100);

            builder.Property(s => s.ShopType)
                .HasMaxLength(50);

            builder.Property(s => s.Rating)
                .HasColumnType("decimal(3,2)");

            builder.Property(s => s.Notes)
                .HasMaxLength(500);

            // Foreign Key Relationships
            
            // CreatedBy relationship
            builder.HasOne(s => s.CreatedBy)
                .WithMany(u => u.ShopsCreated)
                .HasForeignKey(s => s.CreatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // UpdatedBy relationship
            builder.HasOne(s => s.UpdatedBy)
                .WithMany(u => u.ShopsUpdated)
                .HasForeignKey(s => s.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes for Performance
            builder.HasIndex(s => s.Name)
                .HasDatabaseName("IX_Shop_Name");

            builder.HasIndex(s => s.Location)
                .HasDatabaseName("IX_Shop_Location");

            builder.HasIndex(s => s.ShopType)
                .HasDatabaseName("IX_Shop_ShopType");

            builder.HasIndex(s => s.IsActive)
                .HasDatabaseName("IX_Shop_IsActive");

            builder.HasIndex(s => s.CreatedById)
                .HasDatabaseName("IX_Shop_CreatedById");

            builder.HasIndex(s => new { s.ShopType, s.IsActive })
                .HasDatabaseName("IX_Shop_ShopType_IsActive");

            builder.HasIndex(s => new { s.Rating, s.IsActive })
                .HasDatabaseName("IX_Shop_Rating_IsActive");

            // Table Configuration
            builder.ToTable("Shops");
        }
    }
}
