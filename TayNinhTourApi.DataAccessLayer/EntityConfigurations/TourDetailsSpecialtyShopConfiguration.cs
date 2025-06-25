using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity configuration cho TourDetailsSpecialtyShop
    /// </summary>
    public class TourDetailsSpecialtyShopConfiguration : IEntityTypeConfiguration<TourDetailsSpecialtyShop>
    {
        public void Configure(EntityTypeBuilder<TourDetailsSpecialtyShop> builder)
        {
            // Table name
            builder.ToTable("TourDetailsSpecialtyShops");

            // Primary Key
            builder.HasKey(tdss => tdss.Id);

            // Properties
            builder.Property(tdss => tdss.TourDetailsId)
                .IsRequired()
                .HasComment("ID của TourDetails");

            builder.Property(tdss => tdss.SpecialtyShopId)
                .IsRequired()
                .HasComment("ID của SpecialtyShop được mời");

            builder.Property(tdss => tdss.InvitedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()")
                .HasComment("Thời gian được mời tham gia tour");

            builder.Property(tdss => tdss.Status)
                .IsRequired()
                .HasDefaultValue(ShopInvitationStatus.Pending)
                .HasComment("Trạng thái phản hồi của shop");

            builder.Property(tdss => tdss.RespondedAt)
                .IsRequired(false)
                .HasComment("Thời gian shop phản hồi");

            builder.Property(tdss => tdss.ResponseNote)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("Ghi chú từ shop khi phản hồi");

            builder.Property(tdss => tdss.ExpiresAt)
                .IsRequired()
                .HasComment("Thời gian hết hạn invitation");

            builder.Property(tdss => tdss.Priority)
                .IsRequired(false)
                .HasComment("Ưu tiên hiển thị trong timeline");

            // Foreign Key Relationships

            // TourDetails relationship (Required)
            builder.HasOne(tdss => tdss.TourDetails)
                .WithMany(td => td.InvitedSpecialtyShops)
                .HasForeignKey(tdss => tdss.TourDetailsId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // SpecialtyShop relationship (Required)
            builder.HasOne(tdss => tdss.SpecialtyShop)
                .WithMany(ss => ss.TourInvitations)
                .HasForeignKey(tdss => tdss.SpecialtyShopId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            // CreatedBy relationship (Required)
            builder.HasOne(tdss => tdss.CreatedBy)
                .WithMany()
                .HasForeignKey(tdss => tdss.CreatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            // UpdatedBy relationship (Optional)
            builder.HasOne(tdss => tdss.UpdatedBy)
                .WithMany()
                .HasForeignKey(tdss => tdss.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);

            // Indexes for performance
            builder.HasIndex(tdss => tdss.TourDetailsId)
                .HasDatabaseName("IX_TourDetailsSpecialtyShops_TourDetailsId");

            builder.HasIndex(tdss => tdss.SpecialtyShopId)
                .HasDatabaseName("IX_TourDetailsSpecialtyShops_SpecialtyShopId");

            builder.HasIndex(tdss => tdss.Status)
                .HasDatabaseName("IX_TourDetailsSpecialtyShops_Status");

            builder.HasIndex(tdss => new { tdss.TourDetailsId, tdss.SpecialtyShopId })
                .IsUnique()
                .HasDatabaseName("IX_TourDetailsSpecialtyShops_TourDetails_Shop_Unique");

            // Constraints
            builder.HasCheckConstraint("CK_TourDetailsSpecialtyShops_ExpiresAt", 
                "ExpiresAt > InvitedAt");

            builder.HasCheckConstraint("CK_TourDetailsSpecialtyShops_RespondedAt", 
                "RespondedAt IS NULL OR RespondedAt >= InvitedAt");
        }
    }
}
