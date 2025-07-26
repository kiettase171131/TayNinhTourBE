using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot
{
    /// <summary>
    /// DTO cho TourSlot response
    /// </summary>
    public class TourSlotDto
    {
        /// <summary>
        /// ID của TourSlot
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của TourTemplate
        /// </summary>
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// ID của TourDetails (nếu slot được assign cho details cụ thể)
        /// </summary>
        public Guid? TourDetailsId { get; set; }

        /// <summary>
        /// Ngày diễn ra tour
        /// </summary>
        public DateOnly TourDate { get; set; }

        /// <summary>
        /// Ngày trong tuần
        /// </summary>
        public ScheduleDay ScheduleDay { get; set; }

        /// <summary>
        /// Tên ngày trong tuần (tiếng Việt)
        /// </summary>
        public string ScheduleDayName { get; set; } = string.Empty;

        /// <summary>
        /// Trạng thái của slot
        /// </summary>
        public TourSlotStatus Status { get; set; }

        /// <summary>
        /// Tên trạng thái (tiếng Việt)
        /// </summary>
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// Số lượng khách tối đa cho slot này
        /// </summary>
        public int MaxGuests { get; set; }

        /// <summary>
        /// Số lượng khách hiện tại đã booking
        /// </summary>
        public int CurrentBookings { get; set; }

        /// <summary>
        /// Số ghế còn lại
        /// </summary>
        public int AvailableSpots { get; set; }

        /// <summary>
        /// Slot có đang active không
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Slot có thể được booking không (dựa trên available spots và trạng thái)
        /// </summary>
        public bool IsBookable => IsActive && Status == TourSlotStatus.Available && AvailableSpots > 0;

        /// <summary>
        /// Thông tin TourTemplate (nếu include)
        /// </summary>
        public TourTemplateInfo? TourTemplate { get; set; }

        /// <summary>
        /// Thông tin TourDetails (nếu include)
        /// </summary>
        public TourDetailsInfo? TourDetails { get; set; }

        /// <summary>
        /// Thông tin TourOperation (nếu có)
        /// </summary>
        public TourOperationInfo? TourOperation { get; set; }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Ngày cập nhật
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Formatted date string cho display
        /// </summary>
        public string FormattedDate { get; set; } = string.Empty;

        /// <summary>
        /// Formatted date with day name (e.g., "Thứ 7 - 15/8/2025")
        /// </summary>
        public string FormattedDateWithDay { get; set; } = string.Empty;
    }

    /// <summary>
    /// Thông tin cơ bản của TourTemplate
    /// </summary>
    public class TourTemplateInfo
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public TourTemplateType TemplateType { get; set; }
    }

    /// <summary>
    /// Thông tin cơ bản của TourDetails
    /// </summary>
    public class TourDetailsInfo
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public TourDetailsStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Thông tin cơ bản của TourOperation
    /// </summary>
    public class TourOperationInfo
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public int MaxGuests { get; set; }
        public int CurrentBookings { get; set; }
        public int AvailableSpots { get; set; }
        public TourOperationStatus Status { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Chi tiết slot với thông tin tour và danh sách user đã book
    /// </summary>
    public class TourSlotWithBookingsDto
    {
        /// <summary>
        /// Thông tin cơ bản của slot
        /// </summary>
        public TourSlotDto Slot { get; set; } = null!;

        /// <summary>
        /// Thông tin chi tiết tour
        /// </summary>
        public TourDetailsSummary? TourDetails { get; set; }

        /// <summary>
        /// Danh sách user đã book tour này
        /// </summary>
        public List<BookedUserInfo> BookedUsers { get; set; } = new();

        /// <summary>
        /// Thống kê booking
        /// </summary>
        public BookingStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// Thông tin tóm tắt của tour details
    /// </summary>
    public class TourDetailsSummary
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new();
        public List<string> SkillsRequired { get; set; } = new();
        public TourDetailsStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public TourTemplateInfo? TourTemplate { get; set; }
        public TourOperationInfo? TourOperation { get; set; }
    }

    /// <summary>
    /// Thông tin user đã book tour
    /// </summary>
    public class BookedUserInfo
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserEmail { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public int NumberOfGuests { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public BookingStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public string BookingCode { get; set; } = string.Empty;
        public string? CustomerNotes { get; set; }
    }

    /// <summary>
    /// Thống kê booking của slot
    /// </summary>
    public class BookingStatistics
    {
        public int TotalBookings { get; set; }
        public int TotalGuests { get; set; }
        public int ConfirmedBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CancelledBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal ConfirmedRevenue { get; set; }
        public double OccupancyRate { get; set; } // Tỷ lệ lấp đầy (%)
    }
}
