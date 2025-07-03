using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Booking
{
    /// <summary>
    /// DTO cho response danh sách bookings với pagination
    /// </summary>
    public class ResponseGetBookingsDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách bookings
        /// </summary>
        public List<ResponseBookingDto> Bookings { get; set; } = new List<ResponseBookingDto>();

        /// <summary>
        /// Thông tin pagination
        /// </summary>
        public PaginationInfoDto Pagination { get; set; } = new PaginationInfoDto();

        /// <summary>
        /// Thống kê tổng quan
        /// </summary>
        public BookingSummaryDto? Summary { get; set; }
    }

    /// <summary>
    /// DTO thông tin pagination
    /// </summary>
    public class PaginationInfoDto
    {
        /// <summary>
        /// Trang hiện tại
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Kích thước trang
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Tổng số items
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Có trang trước không
        /// </summary>
        public bool HasPrevious { get; set; }

        /// <summary>
        /// Có trang sau không
        /// </summary>
        public bool HasNext { get; set; }
    }

    /// <summary>
    /// DTO thống kê booking
    /// </summary>
    public class BookingSummaryDto
    {
        /// <summary>
        /// Tổng số bookings
        /// </summary>
        public int TotalBookings { get; set; }

        /// <summary>
        /// Số booking đang chờ xác nhận
        /// </summary>
        public int PendingBookings { get; set; }

        /// <summary>
        /// Số booking đã xác nhận
        /// </summary>
        public int ConfirmedBookings { get; set; }

        /// <summary>
        /// Số booking đã hủy
        /// </summary>
        public int CancelledBookings { get; set; }

        /// <summary>
        /// Tổng doanh thu (từ booking đã xác nhận)
        /// </summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>
        /// Tổng số khách
        /// </summary>
        public int TotalGuests { get; set; }
    }
}
