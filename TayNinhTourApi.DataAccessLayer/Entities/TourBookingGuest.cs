using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Entity lưu trữ thông tin từng khách hàng trong tour booking
    /// Mỗi booking có thể có nhiều guests, mỗi guest có QR code riêng
    /// </summary>
    [Table("TourBookingGuests")]
    public class TourBookingGuest : BaseEntity
    {
        /// <summary>
        /// ID của TourBooking chứa guest này
        /// </summary>
        [Required]
        public Guid TourBookingId { get; set; }

        /// <summary>
        /// Họ và tên của khách hàng (bắt buộc)
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Tên khách hàng không được vượt quá 100 ký tự")]
        public string GuestName { get; set; } = null!;

        /// <summary>
        /// Email của khách hàng (bắt buộc, unique trong cùng booking)
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string GuestEmail { get; set; } = null!;

        /// <summary>
        /// Số điện thoại của khách hàng (tùy chọn)
        /// </summary>
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        public string? GuestPhone { get; set; }

        /// <summary>
        /// QR code data riêng cho khách hàng này
        /// Chứa thông tin cá nhân và thông tin booking để tour guide check-in
        /// </summary>
        public string? QRCodeData { get; set; }

        /// <summary>
        /// Trạng thái check-in của khách hàng
        /// </summary>
        public bool IsCheckedIn { get; set; } = false;

        /// <summary>
        /// Thời gian check-in thực tế của khách hàng
        /// Được set khi tour guide quét QR code thành công
        /// </summary>
        public DateTime? CheckInTime { get; set; }

        /// <summary>
        /// Ghi chú bổ sung khi check-in (optional)
        /// Ví dụ: ghi chú đặc biệt từ tour guide
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú check-in không được vượt quá 500 ký tự")]
        public string? CheckInNotes { get; set; }

        // Navigation Properties

        /// <summary>
        /// TourBooking chứa guest này
        /// </summary>
        public virtual TourBooking TourBooking { get; set; } = null!;
    }
}
