namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking
{
    /// <summary>
    /// DTO response cho thông tin từng khách hàng trong tour booking
    /// </summary>
    public class TourBookingGuestDto
    {
        /// <summary>
        /// ID của guest record
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của TourBooking chứa guest này
        /// </summary>
        public Guid TourBookingId { get; set; }

        /// <summary>
        /// Họ và tên của khách hàng
        /// </summary>
        public string GuestName { get; set; } = null!;

        /// <summary>
        /// Email của khách hàng
        /// </summary>
        public string GuestEmail { get; set; } = null!;

        /// <summary>
        /// Số điện thoại của khách hàng (có thể null)
        /// </summary>
        public string? GuestPhone { get; set; }

        /// <summary>
        /// Đánh dấu khách hàng này là người đại diện nhóm
        /// </summary>
        public bool IsGroupRepresentative { get; set; }

        /// <summary>
        /// QR code data riêng cho khách hàng này
        /// Chỉ hiển thị cho owner của booking hoặc admin
        /// </summary>
        public string? QRCodeData { get; set; }

        /// <summary>
        /// Trạng thái check-in của khách hàng
        /// </summary>
        public bool IsCheckedIn { get; set; }

        /// <summary>
        /// Thời gian check-in thực tế (nếu đã check-in)
        /// </summary>
        public DateTime? CheckInTime { get; set; }

        /// <summary>
        /// Ghi chú từ tour guide khi check-in
        /// </summary>
        public string? CheckInNotes { get; set; }

        /// <summary>
        /// Thời gian tạo guest record
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
