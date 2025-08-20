using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBooking;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service xử lý tour booking cho user với early bird pricing
    /// </summary>
    public interface IUserTourBookingService
    {
        /// <summary>
        /// Lấy danh sách tours có thể booking (status = Public, còn slot)
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số lượng items per page</param>
        /// <param name="fromDate">Lọc từ ngày</param>
        /// <param name="toDate">Lọc đến ngày</param>
        /// <param name="searchKeyword">Từ khóa tìm kiếm</param>
        /// <returns>Danh sách tours có thể booking</returns>
        Task<Common.PagedResult<AvailableTourDto>> GetAvailableToursAsync(
            int pageIndex = 1,
            int pageSize = 10,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string? searchKeyword = null);

        /// <summary>
        /// Lấy chi tiết tour để booking
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Chi tiết tour để booking</returns>
        Task<TourDetailsForBookingDto?> GetTourDetailsForBookingAsync(Guid tourDetailsId);

        /// <summary>
        /// Tính giá tour trước khi booking
        /// </summary>
        /// <param name="request">Thông tin tính giá</param>
        /// <returns>Kết quả tính giá</returns>
        Task<PriceCalculationDto?> CalculateBookingPriceAsync(CalculatePriceRequest request);

        /// <summary>
        /// Tạo booking tour mới
        /// </summary>
        /// <param name="request">Thông tin booking</param>
        /// <param name="userId">ID của user thực hiện booking</param>
        /// <returns>Kết quả tạo booking</returns>
        Task<CreateBookingResultDto> CreateBookingAsync(CreateTourBookingRequest request, Guid userId);

        /// <summary>
        /// Lấy danh sách bookings của user với filter
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số lượng items per page</param>
        /// <param name="status">Lọc theo trạng thái booking (confirmed, cancel, pending)</param>
        /// <param name="startDate">Lọc từ ngày (booking date)</param>
        /// <param name="endDate">Lọc đến ngày (booking date)</param>
        /// <param name="searchTerm">Tìm kiếm theo tên công ty tổ chức tour</param>
        /// <param name="bookingCode">Mã PayOsOrderCode để tìm kiếm booking cụ thể</param>
        /// <returns>Danh sách bookings của user</returns>
        Task<Common.PagedResult<TourBookingDto>> GetUserBookingsAsync(
            Guid userId,
            int pageIndex = 1,
            int pageSize = 10,
            BookingStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            string? bookingCode = null);

        /// <summary>
        /// Hủy booking
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <param name="userId">ID của user</param>
        /// <param name="reason">Lý do hủy</param>
        /// <returns>Kết quả hủy booking</returns>
        Task<BaseResposeDto> CancelBookingAsync(Guid bookingId, Guid userId, string? reason = null);

        /// <summary>
        /// Lấy chi tiết booking theo ID
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <param name="userId">ID của user (để kiểm tra quyền)</param>
        /// <returns>Chi tiết booking</returns>
        Task<TourBookingDto?> GetBookingDetailsAsync(Guid bookingId, Guid userId);

        /// <summary>
        /// Xử lý callback thanh toán thành công
        /// </summary>
        /// <param name="payOsOrderCode">PayOS order code</param>
        /// <returns>Kết quả xử lý</returns>
        Task<BaseResposeDto> HandlePaymentSuccessAsync(string payOsOrderCode);

        /// <summary>
        /// Xử lý callback thanh toán hủy
        /// </summary>
        /// <param name="payOsOrderCode">PayOS order code</param>
        /// <returns>Kết quả xử lý</returns>
        Task<BaseResposeDto> HandlePaymentCancelAsync(string payOsOrderCode);

        /// <summary>
        /// Manually resend QR ticket email for confirmed booking
        /// This can be called if the original email failed to send
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <param name="userId">ID của user (để kiểm tra quyền)</param>
        /// <returns>Kết quả gửi lại email</returns>
        Task<BaseResposeDto> ResendQRTicketEmailAsync(Guid bookingId, Guid userId);

        /// <summary>
        /// Lấy tiến độ tour đang diễn ra cho user
        /// </summary>
        /// <param name="tourOperationId">ID của tour operation</param>
        /// <param name="userId">ID của user</param>
        /// <returns>Tiến độ tour với timeline và thống kê</returns>
        Task<UserTourProgressDto?> GetTourProgressAsync(Guid tourOperationId, Guid userId);

        /// <summary>
        /// Kiểm tra user có booking cho tour này không
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="tourOperationId">ID của tour operation</param>
        /// <returns>True nếu user có booking</returns>
        Task<bool> UserHasBookingForTourAsync(Guid userId, Guid tourOperationId);

        /// <summary>
        /// Lấy tổng quan dashboard cho user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Thống kê tổng quan về tours của user</returns>
        Task<UserDashboardSummaryDto> GetUserDashboardSummaryAsync(Guid userId);

        /// <summary>
        /// Gửi lại QR ticket cho booking
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <param name="userId">ID của user</param>
        /// <returns>Kết quả gửi lại QR ticket</returns>
        Task<ResendQRTicketResultDto> ResendQRTicketAsync(Guid bookingId, Guid userId);
    }

    /// <summary>
    /// DTO cho tour có thể booking với thông tin early bird chi tiết
    /// </summary>
    public class AvailableTourDto
    {
        public Guid TourDetailsId { get; set; }
        public Guid TourOperationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public string? ImageUrl => ImageUrls.FirstOrDefault();
        public decimal Price { get; set; }
        public int MaxGuests { get; set; }
        public int CurrentBookings { get; set; }
        public int AvailableSpots => MaxGuests - CurrentBookings;
        public DateTime? TourStartDate { get; set; }
        public string? GuideId { get; set; }
        public string? GuideName { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // ✅ NEW: Slot-specific capacity information
        /// <summary>
        /// Số lượng slot có sẵn (có thể booking)
        /// </summary>
        public int AvailableSlots { get; set; }

        /// <summary>
        /// Tổng capacity của tất cả slots còn available
        /// </summary>
        public int TotalSlotsCapacity { get; set; }

        /// <summary>
        /// Tổng số chỗ trống trong tất cả slots còn available
        /// </summary>
        public int TotalAvailableSpots { get; set; }

        // Early Bird Information
        public bool IsEarlyBirdActive { get; set; }
        public decimal EarlyBirdPrice { get; set; }
        public decimal EarlyBirdDiscountPercent { get; set; }
        public decimal EarlyBirdDiscountAmount { get; set; }
        public DateTime? EarlyBirdEndDate { get; set; }
        public int DaysRemainingForEarlyBird { get; set; }
        public string PricingType { get; set; } = "Standard"; // "Early Bird" hoặc "Standard"

        // Computed properties for FE convenience
        public decimal FinalPrice => IsEarlyBirdActive ? EarlyBirdPrice : Price;
        public bool HasEarlyBirdDiscount => IsEarlyBirdActive && EarlyBirdDiscountPercent > 0;

        /// <summary>
        /// ✅ NEW: Tour có thể book được không (dựa trên slot availability)
        /// </summary>
        public bool IsBookable => AvailableSlots > 0 && TotalAvailableSpots > 0;

        /// <summary>
        /// ✅ NEW: Message hiển thị cho user về availability
        /// </summary>
        public string AvailabilityMessage => AvailableSlots switch
        {
            0 => "Tất cả slot đã đầy",
            1 => $"Còn 1 slot với {TotalAvailableSpots} chỗ trống",
            _ => $"Còn {AvailableSlots} slots với tổng {TotalAvailableSpots} chỗ trống"
        };

        /// <summary>
        /// Đã bỏ IsEarlyBirdEligible để tránh confusion - dùng IsEarlyBirdActive thay thế
        /// </summary>
        [Obsolete("Use IsEarlyBirdActive instead")]
        public bool IsEarlyBirdEligible => IsEarlyBirdActive;
    }

    /// <summary>
    /// DTO chi tiết tour để booking với thông tin early bird
    /// </summary>
    public class TourDetailsForBookingDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public string? ImageUrl => ImageUrls.FirstOrDefault();
        public string? SkillsRequired { get; set; }
        public DateTime CreatedAt { get; set; }

        // Tour Operation info
        public TourOperationSummaryDto TourOperation { get; set; } = new();

        // Timeline
        public List<TimelineItemDto> Timeline { get; set; } = new();

        // Tour dates
        public List<TourDateDto> TourDates { get; set; } = new();

        // Template info
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;

        // Early Bird Information for this tour
        public EarlyBirdInfoDto EarlyBirdInfo { get; set; } = new();
    }

    /// <summary>
    /// DTO thông tin early bird chi tiết
    /// </summary>
    public class EarlyBirdInfoDto
    {
        public bool IsActive { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime? EndDate { get; set; }
        public int DaysRemaining { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal SavingsAmount { get; set; }

        /// <summary>
        /// Kiểm tra early bird có sắp hết hạn không (còn <= 3 ngày)
        /// </summary>
        public bool IsExpiringSoon => IsActive && DaysRemaining <= 3 && DaysRemaining > 0;

        /// <summary>
        /// Message hiển thị cho user
        /// </summary>
        public string DisplayMessage => IsActive
            ? $"Giảm {DiscountPercent}% - Còn {DaysRemaining} ngày!"
            : "Không có giảm giá";
    }

    /// <summary>
    /// DTO cho ngày tour với thông tin slot và early bird
    /// </summary>
    public class TourDateDto
    {
        public Guid TourSlotId { get; set; }
        public DateTime TourDate { get; set; }
        public string ScheduleDay { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int MaxGuests { get; set; }
        public int CurrentBookings { get; set; }
        public int AvailableSpots { get; set; }
        public bool IsBookable { get; set; }
        public string StatusName { get; set; } = string.Empty;

        // Pricing information cho từng slot cụ thể
        public decimal OriginalPrice { get; set; }
        public decimal FinalPrice { get; set; }
        public bool IsEarlyBirdApplicable { get; set; }
        public decimal EarlyBirdDiscountPercent { get; set; }
    }
}
