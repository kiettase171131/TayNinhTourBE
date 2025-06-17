using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho SpecialtyShopApplication entity
    /// Thay thế cho IShopApplicationRepository với methods mở rộng
    /// </summary>
    public interface ISpecialtyShopApplicationRepository : IGenericRepository<SpecialtyShopApplication>
    {
        /// <summary>
        /// Lấy danh sách applications theo status
        /// </summary>
        /// <param name="status">Trạng thái cần filter</param>
        /// <returns>Danh sách applications</returns>
        Task<IEnumerable<SpecialtyShopApplication>> GetByStatusAsync(SpecialtyShopApplicationStatus status);

        /// <summary>
        /// Lấy danh sách applications theo user ID
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Danh sách applications của user</returns>
        Task<IEnumerable<SpecialtyShopApplication>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Lấy application mới nhất của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Application mới nhất hoặc null</returns>
        Task<SpecialtyShopApplication?> GetLatestByUserIdAsync(Guid userId);

        /// <summary>
        /// Kiểm tra user có application pending hoặc approved không
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>True nếu có application active</returns>
        Task<bool> HasActiveApplicationAsync(Guid userId);

        /// <summary>
        /// Lấy danh sách applications với pagination và filter
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (0-based)</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="status">Filter theo status (optional)</param>
        /// <param name="searchTerm">Tìm kiếm theo tên shop hoặc user (optional)</param>
        /// <returns>Tuple chứa (applications, totalCount)</returns>
        Task<(IEnumerable<SpecialtyShopApplication> Applications, int TotalCount)> GetPagedAsync(
            int pageIndex, 
            int pageSize, 
            SpecialtyShopApplicationStatus? status = null, 
            string? searchTerm = null);

        /// <summary>
        /// Lấy application với thông tin User và ProcessedBy
        /// </summary>
        /// <param name="id">ID của application</param>
        /// <returns>Application với navigation properties</returns>
        Task<SpecialtyShopApplication?> GetWithUserInfoAsync(Guid id);

        /// <summary>
        /// Lấy danh sách applications với thông tin User (cho admin list)
        /// </summary>
        /// <param name="status">Filter theo status (optional)</param>
        /// <returns>Applications với User info</returns>
        Task<IEnumerable<SpecialtyShopApplication>> GetWithUserInfoAsync(SpecialtyShopApplicationStatus? status = null);

        /// <summary>
        /// Đếm số applications theo status
        /// </summary>
        /// <param name="status">Status cần đếm</param>
        /// <returns>Số lượng applications</returns>
        Task<int> CountByStatusAsync(SpecialtyShopApplicationStatus status);

        /// <summary>
        /// Lấy applications được submit trong khoảng thời gian
        /// </summary>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <returns>Danh sách applications</returns>
        Task<IEnumerable<SpecialtyShopApplication>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
}
