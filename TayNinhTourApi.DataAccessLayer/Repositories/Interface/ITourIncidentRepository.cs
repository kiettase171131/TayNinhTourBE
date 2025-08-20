using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourIncident entity
    /// Quản lý các operations liên quan đến incident reporting
    /// </summary>
    public interface ITourIncidentRepository : IGenericRepository<TourIncident>
    {
        /// <summary>
        /// Lấy tất cả incidents cho một TourOperation cụ thể
        /// </summary>
        /// <param name="tourOperationId">ID của TourOperation</param>
        /// <returns>Danh sách incidents</returns>
        Task<IEnumerable<TourIncident>> GetByTourOperationAsync(Guid tourOperationId);

        /// <summary>
        /// Lấy tất cả incidents được báo cáo bởi một TourGuide cụ thể
        /// </summary>
        /// <param name="guideId">ID của TourGuide</param>
        /// <returns>Danh sách incidents</returns>
        Task<IEnumerable<TourIncident>> GetByTourGuideAsync(Guid guideId);

        /// <summary>
        /// Lấy incidents theo severity level
        /// </summary>
        /// <param name="severity">Mức độ nghiêm trọng</param>
        /// <returns>Danh sách incidents</returns>
        Task<IEnumerable<TourIncident>> GetBySeverityAsync(string severity);

        /// <summary>
        /// Lấy incidents theo status
        /// </summary>
        /// <param name="status">Trạng thái xử lý</param>
        /// <returns>Danh sách incidents</returns>
        Task<IEnumerable<TourIncident>> GetByStatusAsync(string status);

        /// <summary>
        /// Lấy incidents trong khoảng thời gian
        /// </summary>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <returns>Danh sách incidents</returns>
        Task<IEnumerable<TourIncident>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    }
}
