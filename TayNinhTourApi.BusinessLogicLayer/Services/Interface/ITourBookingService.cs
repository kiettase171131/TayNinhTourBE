using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Booking;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Booking;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho quản lý TourBooking
    /// </summary>
    public interface ITourBookingService
    {
        /// <summary>
        /// Tạo booking mới với capacity validation
        /// Business Rules:
        /// - Kiểm tra TourOperation tồn tại và active
        /// - Kiểm tra capacity còn đủ không
        /// - Tính toán giá tiền chính xác
        /// - Tạo booking code unique
        /// - Handle concurrency để tránh overbooking
        /// </summary>
        /// <param name="request">Thông tin booking</param>
        /// <param name="currentUser">User hiện tại</param>
        /// <returns>Response với thông tin booking đã tạo</returns>
        Task<ResponseCreateBookingDto> CreateBookingAsync(RequestCreateBookingDto request, CurrentUserObject currentUser);

        /// <summary>
        /// Hủy booking
        /// Business Rules:
        /// - Chỉ user tạo booking hoặc admin mới có thể hủy
        /// - Booking phải ở trạng thái có thể hủy
        /// - Cập nhật trạng thái và lý do hủy
        /// - Release capacity đã reserve
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <param name="request">Thông tin hủy booking</param>
        /// <param name="currentUser">User hiện tại</param>
        /// <returns>Response kết quả</returns>
        Task<ResponseCreateBookingDto> CancelBookingAsync(Guid bookingId, RequestUpdateBookingStatusDto request, CurrentUserObject currentUser);

        /// <summary>
        /// Confirm booking (Admin/TourCompany only)
        /// Business Rules:
        /// - Chỉ admin hoặc tour company mới có thể confirm
        /// - Booking phải ở trạng thái Pending
        /// - Capacity đã được reserve từ khi tạo booking
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <param name="currentUser">User hiện tại</param>
        /// <returns>Response kết quả</returns>
        Task<ResponseCreateBookingDto> ConfirmBookingAsync(Guid bookingId, CurrentUserObject currentUser);

        /// <summary>
        /// Lấy danh sách bookings của user hiện tại
        /// </summary>
        /// <param name="currentUser">User hiện tại</param>
        /// <param name="includeInactive">Có bao gồm booking đã hủy không</param>
        /// <returns>Danh sách bookings</returns>
        Task<ResponseGetBookingsDto> GetMyBookingsAsync(CurrentUserObject currentUser, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách bookings với filter (Admin/TourCompany)
        /// </summary>
        /// <param name="request">Filter parameters</param>
        /// <param name="currentUser">User hiện tại</param>
        /// <returns>Danh sách bookings với pagination</returns>
        Task<ResponseGetBookingsDto> GetBookingsAsync(RequestGetBookingsDto request, CurrentUserObject currentUser);

        /// <summary>
        /// Lấy chi tiết booking theo ID
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <param name="currentUser">User hiện tại</param>
        /// <returns>Chi tiết booking</returns>
        Task<ResponseBookingDto?> GetBookingByIdAsync(Guid bookingId, CurrentUserObject currentUser);

        /// <summary>
        /// Lấy booking theo booking code
        /// </summary>
        /// <param name="bookingCode">Mã booking</param>
        /// <param name="currentUser">User hiện tại (optional cho public lookup)</param>
        /// <returns>Chi tiết booking</returns>
        Task<ResponseBookingDto?> GetBookingByCodeAsync(string bookingCode, CurrentUserObject? currentUser = null);

        /// <summary>
        /// Kiểm tra capacity còn lại cho TourOperation
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <param name="requestedGuests">Số khách muốn booking (optional)</param>
        /// <returns>Thông tin capacity</returns>
        Task<ResponseCapacityCheckDto> CheckCapacityAsync(Guid tourOperationId, int? requestedGuests = null);

        /// <summary>
        /// Cập nhật trạng thái booking (Admin/TourCompany only)
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <param name="currentUser">User hiện tại</param>
        /// <returns>Response kết quả</returns>
        Task<ResponseCreateBookingDto> UpdateBookingStatusAsync(Guid bookingId, RequestUpdateBookingStatusDto request, CurrentUserObject currentUser);

        /// <summary>
        /// Lấy thống kê bookings cho TourOperation
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <param name="currentUser">User hiện tại</param>
        /// <returns>Thống kê booking</returns>
        Task<BookingSummaryDto> GetBookingStatisticsAsync(Guid tourOperationId, CurrentUserObject currentUser);

        /// <summary>
        /// Validate business rules cho booking
        /// </summary>
        /// <param name="request">Request booking</param>
        /// <param name="currentUser">User hiện tại</param>
        /// <returns>Kết quả validation</returns>
        Task<(bool IsValid, string ErrorMessage)> ValidateBookingAsync(RequestCreateBookingDto request, CurrentUserObject currentUser);

        /// <summary>
        /// Generate unique booking code
        /// </summary>
        /// <returns>Booking code unique</returns>
        Task<string> GenerateBookingCodeAsync();

        /// <summary>
        /// Tính toán giá tiền cho booking
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <param name="numberOfGuests">Số lượng khách</param>
        /// <returns>Tổng giá tiền</returns>
        Task<decimal> CalculateBookingPriceAsync(Guid tourOperationId, int numberOfGuests);
    }
}
