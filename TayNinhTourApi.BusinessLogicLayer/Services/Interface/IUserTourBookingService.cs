using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBooking;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking;

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
        /// Lấy danh sách bookings của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số lượng items per page</param>
        /// <returns>Danh sách bookings của user</returns>
        Task<Common.PagedResult<TourBookingDto>> GetUserBookingsAsync(Guid userId, int pageIndex = 1, int pageSize = 10);

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
    }

    /// <summary>
    /// DTO cho tour có thể booking
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
        public bool IsEarlyBirdEligible { get; set; }
        public decimal EarlyBirdPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO chi tiết tour để booking
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
    }



    /// <summary>
    /// DTO cho ngày tour với thông tin slot
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
    }


}
