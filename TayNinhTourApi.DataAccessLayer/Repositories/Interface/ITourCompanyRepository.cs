using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourCompany entity
    /// </summary>
    public interface ITourCompanyRepository : IGenericRepository<TourCompany>
    {
        /// <summary>
        /// Lấy TourCompany theo UserId
        /// </summary>
        /// <param name="userId">ID của User</param>
        /// <returns>TourCompany entity hoặc null</returns>
        Task<TourCompany?> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Lấy TourCompany theo UserId với includes
        /// </summary>
        /// <param name="userId">ID của User</param>
        /// <param name="includes">Các navigation properties cần include</param>
        /// <returns>TourCompany entity hoặc null</returns>
        Task<TourCompany?> GetByUserIdAsync(Guid userId, params string[] includes);

        /// <summary>
        /// Kiểm tra xem User có phải là TourCompany không
        /// </summary>
        /// <param name="userId">ID của User</param>
        /// <returns>True nếu User là TourCompany</returns>
        Task<bool> IsUserTourCompanyAsync(Guid userId);

        /// <summary>
        /// Lấy danh sách TourCompany đang hoạt động
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số lượng items per page</param>
        /// <returns>Danh sách TourCompany</returns>
        Task<(List<TourCompany> companies, int totalCount)> GetActiveTourCompaniesAsync(int pageIndex, int pageSize);

        /// <summary>
        /// Cập nhật wallet và revenue hold với optimistic concurrency
        /// </summary>
        /// <param name="tourCompanyId">ID của TourCompany</param>
        /// <param name="walletChange">Thay đổi wallet (có thể âm)</param>
        /// <param name="revenueHoldChange">Thay đổi revenue hold (có thể âm)</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> UpdateFinancialAsync(Guid tourCompanyId, decimal walletChange, decimal revenueHoldChange);
    }
}
