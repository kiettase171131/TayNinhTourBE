using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Booking
{
    /// <summary>
    /// DTO cho request lấy danh sách bookings với filter
    /// </summary>
    public class RequestGetBookingsDto
    {
        /// <summary>
        /// Trang hiện tại (bắt đầu từ 0)
        /// </summary>
        public int PageIndex { get; set; } = 0;

        /// <summary>
        /// Kích thước trang
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Filter theo TourOperation ID (optional)
        /// </summary>
        public Guid? TourOperationId { get; set; }

        /// <summary>
        /// Filter theo User ID (optional)
        /// </summary>
        public Guid? UserId { get; set; }

        /// <summary>
        /// Filter theo trạng thái booking (optional)
        /// </summary>
        public BookingStatus? Status { get; set; }

        /// <summary>
        /// Từ ngày (optional)
        /// </summary>
        public DateTime? FromDate { get; set; }

        /// <summary>
        /// Đến ngày (optional)
        /// </summary>
        public DateTime? ToDate { get; set; }

        /// <summary>
        /// Tìm kiếm theo booking code hoặc tên khách hàng (optional)
        /// </summary>
        public string? SearchText { get; set; }
    }
}
