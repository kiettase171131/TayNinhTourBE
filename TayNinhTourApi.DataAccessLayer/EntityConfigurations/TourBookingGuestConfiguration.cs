using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Entity Framework configuration cho TourBookingGuest entity
    /// Định nghĩa constraints, indexes và relationships
    /// </summary>
    public class TourBookingGuestConfiguration : IEntityTypeConfiguration<TourBookingGuest>
    {
        public void Configure(EntityTypeBuilder<TourBookingGuest> builder)
        {
            // Table Configuration
            builder.ToTable("TourBookingGuests", t =>
            {
                t.HasComment("Bảng lưu trữ thông tin từng khách hàng trong tour booking với QR code riêng");
            });

            // Primary Key
            builder.HasKey(g => g.Id)
                .HasName("PK_TourBookingGuests");

            // Properties Configuration
            builder.Property(g => g.TourBookingId)
                .IsRequired()
                .HasComment("ID của TourBooking chứa guest này");

            builder.Property(g => g.GuestName)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Họ và tên của khách hàng");

            builder.Property(g => g.GuestEmail)
                .IsRequired()
                .HasMaxLength(100)
                .HasComment("Email của khách hàng (unique trong cùng booking)");

            builder.Property(g => g.GuestPhone)
                .HasMaxLength(20)
                .IsRequired(false)
                .HasComment("Số điện thoại của khách hàng (tùy chọn)");

            builder.Property(g => g.QRCodeData)
                .IsRequired(false)
                .HasComment("QR code data riêng cho khách hàng này");

            builder.Property(g => g.IsGroupRepresentative)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Đánh dấu khách hàng này là người đại diện nhóm");

            builder.Property(g => g.IsCheckedIn)
                .IsRequired()
                .HasDefaultValue(false)
                .HasComment("Trạng thái check-in của khách hàng");

            builder.Property(g => g.CheckInTime)
                .IsRequired(false)
                .HasComment("Thời gian check-in thực tế");

            builder.Property(g => g.CheckInNotes)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("Ghi chú bổ sung khi check-in");

            // Unique Constraints
            builder.HasIndex(g => new { g.TourBookingId, g.GuestEmail })
                .IsUnique()
                .HasDatabaseName("UQ_TourBookingGuests_Email_Booking")
                .HasFilter("IsDeleted = 0") // Chỉ apply cho records chưa bị xóa
                .HasAnnotation("SqlServer:IncludeProperties", "GuestName");

            // Performance Indexes
            builder.HasIndex(g => g.TourBookingId)
                .HasDatabaseName("IX_TourBookingGuests_TourBookingId")
                .HasFilter("IsDeleted = 0");

            builder.HasIndex(g => g.GuestEmail)
                .HasDatabaseName("IX_TourBookingGuests_GuestEmail")
                .HasFilter("IsDeleted = 0");

            builder.HasIndex(g => g.QRCodeData)
                .HasDatabaseName("IX_TourBookingGuests_QRCodeData")
                .HasFilter("QRCodeData IS NOT NULL AND IsDeleted = 0");

            builder.HasIndex(g => g.IsCheckedIn)
                .HasDatabaseName("IX_TourBookingGuests_IsCheckedIn")
                .HasFilter("IsDeleted = 0");

            // Foreign Key Relationships
            builder.HasOne(g => g.TourBooking)
                .WithMany(b => b.Guests)
                .HasForeignKey(g => g.TourBookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_TourBookingGuests_TourBooking");

            // Check Constraints (MySQL compatible) - Simplified
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_TourBookingGuests_GuestName_NotEmpty",
                    "LENGTH(TRIM(GuestName)) > 0");
                t.HasCheckConstraint("CK_TourBookingGuests_GuestEmail_NotEmpty",
                    "LENGTH(TRIM(GuestEmail)) > 0");
                // Note: CheckInTime validation will be handled in application logic
            });

            // Soft Delete Filter
            builder.HasQueryFilter(g => !g.IsDeleted);
        }
    }
}
