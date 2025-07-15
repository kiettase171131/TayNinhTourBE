using TayNinhTourApi.BusinessLogicLayer.DTOs;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service quản lý doanh thu và ví tiền của TourCompany
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
        /// </summary>
        /// <param name="tourCompanyUserId">ID của User có role Tour Company</param>
        /// <param name="amount">Số tiền cần chuyển</param>
        /// <returns>Kết quả thực hiện</returns>
        Task<BaseResposeDto> TransferFromHoldToWalletAsync(Guid tourCompanyUserId, decimal amount);

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
        Task<TourCompanyFinancialInfo?> GetFinancialInfoAsync(Guid tourCompanyUserId);

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
    }

    /// <summary>
    /// Thông tin tài chính của TourCompany
    /// </summary>
    public class TourCompanyFinancialInfo
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public decimal Wallet { get; set; }
        public decimal RevenueHold { get; set; }
        public decimal TotalRevenue => Wallet + RevenueHold;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
