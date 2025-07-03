using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourBooking entity
    /// </summary>
    public interface ITourBookingRepository : IGenericRepository<TourBooking>
    {
        /// <summary>
        /// Lấy danh sách bookings theo TourOperation ID
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <param name="includeInactive">Có bao gồm bookings đã hủy không</param>
        /// <returns>Danh sách bookings</returns>
        Task<List<TourBooking>> GetBookingsByOperationIdAsync(Guid tourOperationId, bool includeInactive = false);

        /// <summary>
        /// Đếm số lượng bookings active cho một TourOperation
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <returns>Số lượng bookings active</returns>
        Task<int> GetActiveBookingsCountAsync(Guid tourOperationId);

        /// <summary>
        /// Tính tổng số guests đã booking cho một TourOperation
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <returns>Tổng số guests đã booking</returns>
        Task<int> GetTotalBookedGuestsAsync(Guid tourOperationId);

        /// <summary>
        /// Kiểm tra capacity còn lại cho một TourOperation
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <param name="maxCapacity">Capacity tối đa</param>
        /// <returns>Số chỗ còn trống</returns>
        Task<int> GetAvailableCapacityAsync(Guid tourOperationId, int maxCapacity);

        /// <summary>
        /// Kiểm tra xem có thể booking thêm số guests này không
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <param name="requestedGuests">Số guests muốn booking</param>
        /// <param name="maxCapacity">Capacity tối đa</param>
        /// <returns>True nếu có thể booking</returns>
        Task<bool> CanBookAsync(Guid tourOperationId, int requestedGuests, int maxCapacity);

        /// <summary>
        /// Lấy danh sách bookings của một user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="includeInactive">Có bao gồm bookings đã hủy không</param>
        /// <returns>Danh sách bookings của user</returns>
        Task<List<TourBooking>> GetBookingsByUserIdAsync(Guid userId, bool includeInactive = false);

        /// <summary>
        /// Lấy booking theo booking code
        /// </summary>
        /// <param name="bookingCode">Mã booking</param>
        /// <returns>Booking entity hoặc null</returns>
        Task<TourBooking?> GetBookingByCodeAsync(string bookingCode);

        /// <summary>
        /// Kiểm tra booking code đã tồn tại chưa
        /// </summary>
        /// <param name="bookingCode">Mã booking</param>
        /// <returns>True nếu đã tồn tại</returns>
        Task<bool> IsBookingCodeExistsAsync(string bookingCode);

        /// <summary>
        /// Cập nhật trạng thái booking
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <param name="newStatus">Trạng thái mới</param>
        /// <param name="reason">Lý do thay đổi (nếu có)</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> UpdateBookingStatusAsync(Guid bookingId, BookingStatus newStatus, string? reason = null);

        /// <summary>
        /// Lấy thống kê bookings theo TourOperation
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <returns>Dictionary với key là status và value là count</returns>
        Task<Dictionary<BookingStatus, int>> GetBookingStatisticsAsync(Guid tourOperationId);

        /// <summary>
        /// Lấy danh sách bookings với pagination
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <param name="tourOperationId">Filter theo TourOperation (optional)</param>
        /// <param name="userId">Filter theo User (optional)</param>
        /// <param name="status">Filter theo Status (optional)</param>
        /// <returns>Danh sách bookings với pagination</returns>
        Task<List<TourBooking>> GetBookingsWithPaginationAsync(
            int pageIndex, 
            int pageSize, 
            Guid? tourOperationId = null, 
            Guid? userId = null, 
            BookingStatus? status = null);

        /// <summary>
        /// Đếm tổng số bookings theo filter
        /// </summary>
        /// <param name="tourOperationId">Filter theo TourOperation (optional)</param>
        /// <param name="userId">Filter theo User (optional)</param>
        /// <param name="status">Filter theo Status (optional)</param>
        /// <returns>Tổng số bookings</returns>
        Task<int> CountBookingsAsync(
            Guid? tourOperationId = null,
            Guid? userId = null,
            BookingStatus? status = null);

        /// <summary>
        /// Kiểm tra và reserve capacity với optimistic concurrency control
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <param name="requestedGuests">Số guests muốn booking</param>
        /// <returns>True nếu reserve thành công</returns>
        Task<bool> TryReserveCapacityAsync(Guid tourOperationId, int requestedGuests);

        /// <summary>
        /// Release capacity khi hủy booking
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <param name="guestsToRelease">Số guests cần release</param>
        /// <returns>True nếu release thành công</returns>
        Task<bool> ReleaseCapacityAsync(Guid tourOperationId, int guestsToRelease);
    }
}
