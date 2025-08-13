using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourBookingGuest entity
    /// Cung cấp các method specialized cho guest management
    /// </summary>
    public interface ITourBookingGuestRepository : IGenericRepository<TourBookingGuest>
    {
        /// <summary>
        /// Lấy danh sách guests theo booking ID
        /// </summary>
        /// <param name="bookingId">ID của tour booking</param>
        /// <returns>Danh sách guests trong booking</returns>
        Task<List<TourBookingGuest>> GetGuestsByBookingIdAsync(Guid bookingId);

        /// <summary>
        /// Tìm guest theo QR code data
        /// Sử dụng khi tour guide scan QR code để check-in
        /// </summary>
        /// <param name="qrCodeData">QR code data để tìm kiếm</param>
        /// <returns>Guest record nếu tìm thấy, null nếu không</returns>
        Task<TourBookingGuest?> GetGuestByQRCodeAsync(string qrCodeData);

        /// <summary>
        /// Kiểm tra email có unique trong booking không
        /// Sử dụng khi validate guest info
        /// </summary>
        /// <param name="bookingId">ID của booking</param>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="excludeGuestId">ID của guest cần exclude (cho update)</param>
        /// <returns>True nếu email unique, false nếu đã tồn tại</returns>
        Task<bool> IsEmailUniqueInBookingAsync(Guid bookingId, string email, Guid? excludeGuestId = null);

        /// <summary>
        /// Lấy guest với thông tin booking và tour operation
        /// Sử dụng cho check-in flow
        /// </summary>
        /// <param name="guestId">ID của guest</param>
        /// <returns>Guest với navigation properties</returns>
        Task<TourBookingGuest?> GetGuestWithBookingDetailsAsync(Guid guestId);

        /// <summary>
        /// Lấy danh sách guests đã check-in cho một tour slot
        /// Sử dụng cho tour guide dashboard
        /// </summary>
        /// <param name="tourSlotId">ID của tour slot</param>
        /// <returns>Danh sách guests đã check-in</returns>
        Task<List<TourBookingGuest>> GetCheckedInGuestsByTourSlotAsync(Guid tourSlotId);

        /// <summary>
        /// Đếm số guests đã check-in cho một tour slot
        /// </summary>
        /// <param name="tourSlotId">ID của tour slot</param>
        /// <returns>Số lượng guests đã check-in</returns>
        Task<int> CountCheckedInGuestsByTourSlotAsync(Guid tourSlotId);

        /// <summary>
        /// Bulk update check-in status cho multiple guests
        /// Sử dụng khi tour guide check-in hàng loạt
        /// </summary>
        /// <param name="guestIds">Danh sách guest IDs</param>
        /// <param name="checkInTime">Thời gian check-in</param>
        /// <param name="notes">Ghi chú chung</param>
        /// <returns>Số lượng guests được update</returns>
        Task<int> BulkCheckInGuestsAsync(List<Guid> guestIds, DateTime checkInTime, string? notes = null);
    }
}
