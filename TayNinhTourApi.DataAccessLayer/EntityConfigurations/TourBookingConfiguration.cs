using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.EntityConfigurations
{
    /// <summary>
    /// Cấu hình Entity Framework cho TourBooking entity
    /// </summary>
    public class TourBookingConfiguration : IEntityTypeConfiguration<TourBooking>
    {
        public void Configure(EntityTypeBuilder<TourBooking> builder)
        {
            // Table Configuration
            builder.ToTable("TourBookings", t =>
            {
                t.HasCheckConstraint("CK_TourBookings_NumberOfGuests_Positive", "NumberOfGuests > 0");
                t.HasCheckConstraint("CK_TourBookings_AdultCount_NonNegative", "AdultCount >= 0");
                t.HasCheckConstraint("CK_TourBookings_ChildCount_NonNegative", "ChildCount >= 0");
                t.HasCheckConstraint("CK_TourBookings_TotalPrice_NonNegative", "TotalPrice >= 0");
                t.HasCheckConstraint("CK_TourBookings_GuestCount_Match", "NumberOfGuests = AdultCount + ChildCount");
            });

            // Primary Key
            builder.HasKey(tb => tb.Id);

            // Property Configurations
            builder.Property(tb => tb.TourOperationId)
                .IsRequired()
                .HasComment("ID của TourOperation được booking");

            builder.Property(tb => tb.TourSlotId)
                .IsRequired(false)
                .HasComment("ID của TourSlot cụ thể được booking (optional)");

            builder.Property(tb => tb.UserId)
                .IsRequired()
                .HasComment("ID của User thực hiện booking");

            builder.Property(tb => tb.NumberOfGuests)
                .IsRequired()
                .HasComment("Tổng số lượng khách trong booking");

            builder.Property(tb => tb.AdultCount)
                .IsRequired()
                .HasComment("Số lượng khách người lớn");

            builder.Property(tb => tb.ChildCount)
                .IsRequired()
                .HasDefaultValue(0)
                .HasComment("Số lượng trẻ em");

            builder.Property(tb => tb.OriginalPrice)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasComment("Giá gốc trước khi áp dụng discount");

            builder.Property(tb => tb.DiscountPercent)
                .IsRequired()
                .HasPrecision(5, 2)
                .HasDefaultValue(0)
                .HasComment("Phần trăm giảm giá được áp dụng");

            builder.Property(tb => tb.TotalPrice)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasComment("Tổng giá tiền của booking sau discount");

            builder.Property(tb => tb.Status)
                .IsRequired()
                .HasDefaultValue(BookingStatus.Pending)
                .HasComment("Trạng thái của booking");

            builder.Property(tb => tb.BookingDate)
                .IsRequired()
                .HasComment("Ngày tạo booking");

            builder.Property(tb => tb.ConfirmedDate)
                .IsRequired(false)
                .HasComment("Ngày xác nhận booking");

            builder.Property(tb => tb.CancelledDate)
                .IsRequired(false)
                .HasComment("Ngày hủy booking");

            builder.Property(tb => tb.CancellationReason)
                .HasMaxLength(500)
                .IsRequired(false)
                .HasComment("Lý do hủy booking");

            builder.Property(tb => tb.CustomerNotes)
                .HasMaxLength(1000)
                .IsRequired(false)
                .HasComment("Ghi chú từ khách hàng");

            builder.Property(tb => tb.ContactName)
                .HasMaxLength(100)
                .IsRequired(false)
                .HasComment("Tên người liên hệ");

            builder.Property(tb => tb.ContactPhone)
                .HasMaxLength(20)
                .IsRequired(false)
                .HasComment("Số điện thoại liên hệ");

            builder.Property(tb => tb.ContactEmail)
                .HasMaxLength(100)
                .IsRequired(false)
                .HasComment("Email liên hệ");

            builder.Property(tb => tb.BookingCode)
                .IsRequired()
                .HasMaxLength(20)
                .HasComment("Mã booking duy nhất");

            builder.Property(tb => tb.PayOsOrderCode)
                .HasMaxLength(20)
                .IsRequired(false)
                .HasComment("PayOS order code cho thanh toán");

            builder.Property(tb => tb.QRCodeData)
                .IsRequired(false)
                .HasComment("QR code data cho khách hàng");

            builder.Property(tb => tb.ReservedUntil)
                .IsRequired(false)
                .HasComment("Thời gian hết hạn reservation để tự động release slot nếu không thanh toán");

            builder.Property(tb => tb.RowVersion)
                .IsRequired()
                .IsRowVersion()
                .HasComment("Row version cho optimistic concurrency control");

            // Foreign Key Relationships
            builder.HasOne(tb => tb.TourOperation)
                .WithMany() // TourOperation có nhiều bookings
                .HasForeignKey(tb => tb.TourOperationId)
                .OnDelete(DeleteBehavior.Restrict) // Không cho phép xóa TourOperation nếu có bookings
                .IsRequired();

            builder.HasOne(tb => tb.User)
                .WithMany() // User có nhiều bookings
                .HasForeignKey(tb => tb.UserId)
                .OnDelete(DeleteBehavior.Restrict) // Không cho phép xóa User nếu có bookings
                .IsRequired();

            builder.HasOne(tb => tb.TourSlot)
                .WithMany() // TourSlot có nhiều bookings
                .HasForeignKey(tb => tb.TourSlotId)
                .OnDelete(DeleteBehavior.Restrict) // Không cho phép xóa TourSlot nếu có bookings
                .IsRequired(false);

            // Indexes for Performance
            builder.HasIndex(tb => tb.TourOperationId)
                .HasDatabaseName("IX_TourBookings_TourOperationId");

            builder.HasIndex(tb => tb.UserId)
                .HasDatabaseName("IX_TourBookings_UserId");

            builder.HasIndex(tb => tb.TourSlotId)
                .HasDatabaseName("IX_TourBookings_TourSlotId");

            builder.HasIndex(tb => tb.BookingCode)
                .IsUnique()
                .HasDatabaseName("IX_TourBookings_BookingCode_Unique");

            builder.HasIndex(tb => tb.PayOsOrderCode)
                .IsUnique()
                .HasDatabaseName("IX_TourBookings_PayOsOrderCode_Unique");

            builder.HasIndex(tb => tb.Status)
                .HasDatabaseName("IX_TourBookings_Status");

            builder.HasIndex(tb => tb.BookingDate)
                .HasDatabaseName("IX_TourBookings_BookingDate");

            // Composite index for capacity checking
            builder.HasIndex(tb => new { tb.TourOperationId, tb.Status })
                .HasDatabaseName("IX_TourBookings_TourOperationId_Status");
        }
    }
}
