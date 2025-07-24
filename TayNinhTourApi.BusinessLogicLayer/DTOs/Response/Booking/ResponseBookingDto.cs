using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Booking
{
    /// <summary>
    /// DTO cho response booking details
    /// </summary>
    public class ResponseBookingDto
    {
        /// <summary>
        /// ID của booking
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của TourOperation
        /// </summary>
        public Guid TourOperationId { get; set; }

        /// <summary>
        /// ID của User
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Tên user đã booking
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Email user đã booking
        /// </summary>
        public string? UserEmail { get; set; }

        /// <summary>
        /// Số lượng khách
        /// </summary>
        public int NumberOfGuests { get; set; }

        /// <summary>
        /// Tổng giá tiền
        /// </summary>
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Trạng thái booking
        /// </summary>
        public BookingStatus Status { get; set; }

        /// <summary>
        /// Tên trạng thái bằng tiếng Việt
        /// </summary>
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// Mã booking
        /// </summary>
        public string BookingCode { get; set; } = string.Empty;

        /// <summary>
        /// Ngày tạo booking
        /// </summary>
        public DateTime BookingDate { get; set; }

        /// <summary>
        /// Ngày xác nhận booking
        /// </summary>
        public DateTime? ConfirmedDate { get; set; }

        /// <summary>
        /// Ngày hủy booking
        /// </summary>
        public DateTime? CancelledDate { get; set; }

        /// <summary>
        /// Lý do hủy booking
        /// </summary>
        public string? CancellationReason { get; set; }

        /// <summary>
        /// Ghi chú từ khách hàng
        /// </summary>
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Tên người liên hệ
        /// </summary>
        public string? ContactName { get; set; }

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        public string? ContactPhone { get; set; }

        /// <summary>
        /// Email liên hệ
        /// </summary>
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Thông tin tour operation
        /// </summary>
        public TourOperationSummaryDto? TourOperation { get; set; }

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian cập nhật cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// DTO tóm tắt thông tin TourOperation cho booking
    /// </summary>
    public class TourOperationSummaryDto
    {
        /// <summary>
        /// ID của TourOperation
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Giá tour
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Số ghế tối đa
        /// </summary>
        public int MaxGuests { get; set; }

        /// <summary>
        /// Tên tour template
        /// </summary>
        public string TourTitle { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả tour
        /// </summary>
        public string? TourDescription { get; set; }

        /// <summary>
        /// Ngày tour (từ TourSlot)
        /// </summary>
        public DateTime? TourDate { get; set; }

        /// <summary>
        /// Tên hướng dẫn viên
        /// </summary>
        public string? GuideName { get; set; }

        /// <summary>
        /// Số điện thoại hướng dẫn viên
        /// </summary>
        public string? GuidePhone { get; set; }
    }
}
