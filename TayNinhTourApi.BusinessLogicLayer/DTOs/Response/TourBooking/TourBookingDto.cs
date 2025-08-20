using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking
{
    /// <summary>
    /// DTO cho thông tin tour booking
    /// </summary>
    public class TourBookingDto
    {
        public Guid Id { get; set; }
        public Guid TourOperationId { get; set; }
        public Guid UserId { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string BookingCode { get; set; } = string.Empty;
        public string? PayOsOrderCode { get; set; }
        [Obsolete("Use Guests collection instead. Will be removed in future version.")]
        public string? QRCodeData { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancellationReason { get; set; }
        public string? CustomerNotes { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? SpecialRequests { get; set; }

        /// <summary>
        /// Loại booking: Individual hoặc GroupRepresentative
        /// </summary>
        public string BookingType { get; set; } = "Individual";

        /// <summary>
        /// Tên nhóm (chỉ áp dụng cho booking loại GroupRepresentative)
        /// </summary>
        public string? GroupName { get; set; }

        /// <summary>
        /// Mô tả nhóm (chỉ áp dụng cho booking loại GroupRepresentative)
        /// </summary>
        public string? GroupDescription { get; set; }

        /// <summary>
        /// QR code data cho cả nhóm (chỉ áp dụng cho booking loại GroupRepresentative)
        /// </summary>
        public string? GroupQRCodeData { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Tên tour (computed từ TourOperation.TourDetails.Title)
        /// </summary>
        public string TourTitle { get; set; } = string.Empty;

        /// <summary>
        /// Ngày tour (computed từ TourSlot.TourDate)
        /// </summary>
        public DateTime? TourDate { get; set; }

        /// <summary>
        /// Danh sách khách hàng trong booking này
        /// Mỗi guest có thông tin riêng và QR code riêng
        /// </summary>
        public List<TourBookingGuestDto> Guests { get; set; } = new();

        // Navigation properties
        public TourOperationSummaryDto? TourOperation { get; set; }
        public UserSummaryDto? User { get; set; }
    }

    /// <summary>
    /// DTO tóm tắt thông tin TourOperation cho booking
    /// </summary>
    public class TourOperationSummaryDto
    {
        public Guid Id { get; set; }
        public Guid TourDetailsId { get; set; }
        public string TourTitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxGuests { get; set; }
        public int CurrentBookings { get; set; }
        public int AvailableSpots => MaxGuests - CurrentBookings;
        public DateTime? TourStartDate { get; set; }
        public string? GuideId { get; set; }
        public string? GuideName { get; set; }
        public string? GuidePhone { get; set; }
    }

    /// <summary>
    /// DTO tóm tắt thông tin User cho booking
    /// </summary>
    public class UserSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
