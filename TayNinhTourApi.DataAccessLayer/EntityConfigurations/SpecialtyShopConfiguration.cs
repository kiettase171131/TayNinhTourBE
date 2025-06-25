using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho SpecialtyShop entity
    /// Định nghĩa relationships, constraints và indexes
    /// </summary>
    public class SpecialtyShopConfiguration : IEntityTypeConfiguration<SpecialtyShop>
    {
        public void Configure(EntityTypeBuilder<SpecialtyShop> builder)
        {
            // Primary Key
            builder.HasKey(s => s.Id);

            // Unique constraint on UserId (1:1 relationship)
            builder.HasIndex(s => s.UserId)
                .IsUnique()
                .HasDatabaseName("IX_SpecialtyShop_UserId_Unique");

            // Required Properties Configuration
            builder.Property(s => s.UserId)
                .IsRequired();

            builder.Property(s => s.ShopName)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Location)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(s => s.RepresentativeName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(100);

            // Optional Properties Configuration
            builder.Property(s => s.Description)
                .HasMaxLength(1000);

            builder.Property(s => s.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(s => s.Address)
                .HasMaxLength(500);

            builder.Property(s => s.Website)
                .HasMaxLength(200);

            builder.Property(s => s.BusinessLicense)
                .HasMaxLength(100);

            builder.Property(s => s.BusinessLicenseUrl)
                .HasMaxLength(500);

            builder.Property(s => s.LogoUrl)
                .HasMaxLength(500);

            builder.Property(s => s.ShopType)
                .HasMaxLength(50);

            builder.Property(s => s.OpeningHours)
                .HasMaxLength(100);

            builder.Property(s => s.Rating)
                .HasColumnType("decimal(3,2)");

            builder.Property(s => s.IsShopActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(s => s.Notes)
                .HasMaxLength(500);

            // Foreign Key Relationships

            // 1:1 Relationship with User
            builder.HasOne(s => s.User)
                .WithOne(u => u.SpecialtyShop)
                .HasForeignKey<SpecialtyShop>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // 1:N Relationship with TimelineItems
            builder.HasMany(s => s.TimelineItems)
                .WithOne(t => t.SpecialtyShop)
                .HasForeignKey(t => t.SpecialtyShopId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // Indexes for Performance
            builder.HasIndex(s => s.ShopName)
                .HasDatabaseName("IX_SpecialtyShop_ShopName");

            builder.HasIndex(s => s.Email)
                .HasDatabaseName("IX_SpecialtyShop_Email");

            builder.HasIndex(s => s.IsShopActive)
                .HasDatabaseName("IX_SpecialtyShop_IsShopActive");

            builder.HasIndex(s => s.ShopType)
                .HasDatabaseName("IX_SpecialtyShop_ShopType");

            builder.HasIndex(s => s.Location)
                .HasDatabaseName("IX_SpecialtyShop_Location");

            // Composite index for active shops by type
            builder.HasIndex(s => new { s.IsShopActive, s.ShopType })
                .HasDatabaseName("IX_SpecialtyShop_IsShopActive_ShopType");

            // Table name
            builder.ToTable("SpecialtyShops");
        }
    }
}
