using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service quản lý doanh thu và ví tiền của TourCompany
    /// Updated to work with booking-level revenue hold
    /// </summary>
    public interface ITourRevenueService
    {
        /// <summary>
        /// Thêm tiền vào revenue hold sau khi booking thành công
        /// </summary>
        /// <param name="tourCompanyUserId">ID của User có role Tour Company</param>
        /// <param name="amount">Số tiền cần thêm</param>
        /// <param name="bookingId">ID của booking</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<BaseResposeDto> AddToRevenueHoldAsync(Guid tourCompanyUserId, decimal amount, Guid bookingId);

        /// <summary>
        /// Chuyển tiền từ revenue hold sang wallet (sau 3 ngày từ ngày tour kết thúc)
        /// NEW: Works with booking-level revenue hold
        /// </summary>
        /// <param name="tourCompanyUserId">ID của User có role Tour Company</param>
        /// <param name="amount">Số tiền cần chuyển</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<BaseResposeDto> TransferFromHoldToWalletAsync(Guid tourCompanyUserId, decimal amount);

        /// <summary>
        /// Chuyển tiền từ booking revenue hold sang TourCompany wallet
        /// NEW: Transfer revenue from specific booking to tour company wallet
        /// </summary>
        /// <param name="bookingId">ID của booking cần chuyển tiền</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<BaseResposeDto> TransferBookingRevenueToWalletAsync(Guid bookingId);

        /// <summary>
        /// Trừ tiền từ revenue hold để chuẩn bị hoàn tiền (khi hủy tour)
        /// </summary>
        /// <param name="tourCompanyUserId">ID của User có role Tour Company</param>
        /// <param name="amount">Số tiền cần trừ</param>
        /// <param name="bookingId">ID của booking bị hủy</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<BaseResposeDto> RefundFromRevenueHoldAsync(Guid tourCompanyUserId, decimal amount, Guid bookingId);

        /// <summary>
        /// Lấy thông tin tài chính của TourCompany
        /// </summary>
        /// <param name="tourCompanyUserId">ID của User có role Tour Company</param>
        /// <returns>Thông tin tài chính</returns>
        Task<DTOs.Response.TourCompany.TourCompanyFinancialInfo?> GetFinancialInfoAsync(Guid tourCompanyUserId);

        /// <summary>
        /// Tạo TourCompany record cho User mới có role Tour Company
        /// </summary>
        /// <param name="userId">ID của User</param>
        /// <param name="companyName">Tên công ty</param>
        /// <returns>Kết quả tạo</returns>
        Task<BaseResposeDto> CreateTourCompanyAsync(Guid userId, string companyName);

        /// <summary>
        /// Kiểm tra xem User có phải là Tour Company không
        /// </summary>
        /// <param name="userId">ID của User</param>
        /// <returns>True nếu là Tour Company</returns>
        Task<bool> IsTourCompanyAsync(Guid userId);

        /// <summary>
        /// Get all bookings with revenue hold that are eligible for transfer (after 3 days)
        /// NEW: Helper method for automated revenue transfer process
        /// </summary>
        /// <returns>List of bookings eligible for revenue transfer</returns>
        Task<List<TourBooking>> GetBookingsEligibleForRevenueTransferAsync();
    }
}
