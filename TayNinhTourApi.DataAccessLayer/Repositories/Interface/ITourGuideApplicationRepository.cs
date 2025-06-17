using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Enhanced repository interface cho TourGuideApplication entity
    /// Nâng cấp từ version cũ với đầy đủ methods
    /// </summary>
    public interface ITourGuideApplicationRepository : IGenericRepository<TourGuideApplication>
    {
        /// <summary>
        /// Lấy danh sách applications theo status (Enhanced version)
        /// </summary>
        /// <param name="status">Trạng thái cần filter</param>
        /// <returns>Danh sách applications</returns>
        Task<IEnumerable<TourGuideApplication>> GetByStatusAsync(TourGuideApplicationStatus status);

        /// <summary>
        /// Lấy danh sách applications theo user ID (Enhanced version)
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Danh sách applications của user</returns>
        Task<IEnumerable<TourGuideApplication>> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Lấy application theo ID với thông tin User và ProcessedBy
        /// </summary>
        /// <param name="applicationId">ID của application</param>
        /// <returns>Application với navigation properties</returns>
        Task<TourGuideApplication?> GetByIdWithDetailsAsync(Guid applicationId);

        /// <summary>
        /// Lấy application của user theo ID (để check ownership)
        /// </summary>
        /// <param name="applicationId">ID của application</param>
        /// <param name="userId">ID của user</param>
        /// <returns>Application nếu thuộc về user</returns>
        Task<TourGuideApplication?> GetByIdAndUserIdAsync(Guid applicationId, Guid userId);

        /// <summary>
        /// Lấy danh sách applications với pagination và filter
        /// </summary>
        /// <param name="pageIndex">Index của page (0-based)</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="status">Filter theo status (optional)</param>
        /// <param name="searchTerm">Search term (optional)</param>
        /// <returns>Tuple với danh sách applications và total count</returns>
        Task<(IEnumerable<TourGuideApplication> Applications, int TotalCount)> GetPagedAsync(
            int pageIndex,
            int pageSize,
            TourGuideApplicationStatus? status = null,
            string? searchTerm = null);

        /// <summary>
        /// Kiểm tra user có application pending hoặc approved không
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>True nếu có application active</returns>
        Task<bool> HasActiveApplicationAsync(Guid userId);

        /// <summary>
        /// Lấy application mới nhất của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Application mới nhất hoặc null</returns>
        Task<TourGuideApplication?> GetLatestByUserIdAsync(Guid userId);

        /// <summary>
        /// Đếm số applications theo status
        /// </summary>
        /// <param name="status">Status cần đếm</param>
        /// <returns>Số lượng applications</returns>
        Task<int> CountByStatusAsync(TourGuideApplicationStatus status);

        // Legacy methods for backward compatibility
        [Obsolete("Use GetByStatusAsync instead")]
        Task<IEnumerable<TourGuideApplication>> ListByStatusAsync(ApplicationStatus status);

        [Obsolete("Use GetByUserIdAsync instead")]
        Task<IEnumerable<TourGuideApplication>> ListByUserAsync(Guid userId);
    }
}
