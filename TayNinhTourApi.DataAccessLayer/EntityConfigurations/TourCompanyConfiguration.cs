using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho TourCompany entity
    /// </summary>
    public class TourCompanyConfiguration : IEntityTypeConfiguration<TourCompany>
    {
        public void Configure(EntityTypeBuilder<TourCompany> builder)
        {
            // Table name
            builder.ToTable("TourCompanies");

            // Primary Key
            builder.HasKey(tc => tc.Id);

            // Properties Configuration
            builder.Property(tc => tc.UserId)
                .IsRequired()
                .HasComment("ID của User có role Tour Company");

            builder.Property(tc => tc.CompanyName)
                .IsRequired()
                .HasMaxLength(200)
                .HasComment("Tên công ty tour");

            builder.Property(tc => tc.Wallet)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasDefaultValue(0)
                .HasComment("Số tiền có thể rút");

            builder.Property(tc => tc.RevenueHold)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasDefaultValue(0)
                .HasComment("Số tiền đang hold chờ chuyển");

            builder.Property(tc => tc.Description)
                .HasMaxLength(1000)
                .IsRequired(false)
                .HasComment("Mô tả về công ty");

            builder.Property(tc => tc.Address)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("Địa chỉ công ty");

            builder.Property(tc => tc.Website)
                .HasMaxLength(200)
                .IsRequired(false)
                .HasComment("Website công ty");

            builder.Property(tc => tc.BusinessLicense)
                .HasMaxLength(50)
                .IsRequired(false)
                .HasComment("Số giấy phép kinh doanh");

            builder.Property(tc => tc.IsActive)
                .IsRequired()
                .HasDefaultValue(true)
                .HasComment("Trạng thái hoạt động của công ty");

            // Foreign Key Relationships

            // User relationship (One-to-One)
            builder.HasOne(tc => tc.User)
                .WithOne(u => u.TourCompany) // User có navigation property TourCompany
                .HasForeignKey<TourCompany>(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // TourTemplate relationship (One-to-Many)
            builder.HasMany(tc => tc.TourTemplates)
                .WithOne(tt => tt.CreatedBy) // TourTemplate có navigation property CreatedBy
                .HasForeignKey(tt => tt.CreatedById)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa company nếu còn templates

            // TourDetails relationship (One-to-Many)
            builder.HasMany(tc => tc.TourDetailsCreated)
                .WithOne(td => td.CreatedBy) // TourDetails có navigation property CreatedBy
                .HasForeignKey(td => td.CreatedById)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa company nếu còn details

            // TourOperation relationship (One-to-Many)
            builder.HasMany(tc => tc.TourOperationsCreated)
                .WithOne(to => to.CreatedBy) // TourOperation có navigation property CreatedBy
                .HasForeignKey(to => to.CreatedById)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa company nếu còn operations

            // Indexes
            builder.HasIndex(tc => tc.UserId)
                .IsUnique()
                .HasDatabaseName("IX_TourCompanies_UserId");

            builder.HasIndex(tc => tc.CompanyName)
                .HasDatabaseName("IX_TourCompanies_CompanyName");

            builder.HasIndex(tc => tc.IsActive)
                .HasDatabaseName("IX_TourCompanies_IsActive");

            // Constraints
            builder.HasCheckConstraint("CK_TourCompanies_Wallet_NonNegative", "`Wallet` >= 0");
            builder.HasCheckConstraint("CK_TourCompanies_RevenueHold_NonNegative", "`RevenueHold` >= 0");
        }
    }
}
