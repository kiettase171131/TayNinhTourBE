using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourOperation entity
    /// Kế thừa từ IGenericRepository và thêm các methods specific cho TourOperation
    /// </summary>
    public interface ITourOperationRepository : IGenericRepository<TourOperation>
    {
        /// <summary>
        /// Lấy tour operation theo tour slot
        /// </summary>
        /// <param name="tourSlotId">ID của tour slot</param>
        /// <returns>Tour operation nếu tồn tại</returns>
        Task<TourOperation?> GetByTourSlotAsync(Guid tourSlotId);

        /// <summary>
        /// Lấy danh sách tour operations theo guide
        /// </summary>
        /// <param name="guideId">ID của guide</param>
        /// <param name="includeInactive">Có bao gồm operations không active không</param>
        /// <returns>Danh sách tour operations</returns>
        Task<IEnumerable<TourOperation>> GetByGuideAsync(Guid guideId, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour operations active (có thể booking)
        /// </summary>
        /// <param name="fromDate">Từ ngày (optional)</param>
        /// <param name="toDate">Đến ngày (optional)</param>
        /// <returns>Danh sách tour operations active</returns>
        Task<IEnumerable<TourOperation>> GetActiveOperationsAsync(DateOnly? fromDate = null, DateOnly? toDate = null);

        /// <summary>
        /// Lấy tour operation với đầy đủ thông tin relationships
        /// </summary>
        /// <param name="id">ID của tour operation</param>
        /// <returns>Tour operation với relationships</returns>
        Task<TourOperation?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Lấy danh sách tour operations theo guide trong khoảng thời gian
        /// </summary>
        /// <param name="guideId">ID của guide</param>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <param name="includeInactive">Có bao gồm operations không active không</param>
        /// <returns>Danh sách tour operations</returns>
        Task<IEnumerable<TourOperation>> GetByGuideAndDateRangeAsync(Guid guideId, DateOnly startDate, DateOnly endDate, bool includeInactive = false);

        /// <summary>
        /// Kiểm tra xem guide có bận trong ngày cụ thể không
        /// </summary>
        /// <param name="guideId">ID của guide</param>
        /// <param name="tourDate">Ngày tour</param>
        /// <param name="excludeOperationId">ID operation cần loại trừ (optional, dùng khi update)</param>
        /// <returns>True nếu guide đã bận</returns>
        Task<bool> IsGuideBusyOnDateAsync(Guid guideId, DateOnly tourDate, Guid? excludeOperationId = null);

        /// <summary>
        /// Lấy danh sách tour operations sắp tới của guide
        /// </summary>
        /// <param name="guideId">ID của guide</param>
        /// <param name="top">Số lượng operations lấy về</param>
        /// <returns>Danh sách tour operations sắp tới</returns>
        Task<IEnumerable<TourOperation>> GetUpcomingOperationsByGuideAsync(Guid guideId, int top = 10);

        /// <summary>
        /// Lấy danh sách tour operations với pagination và filtering
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="guideId">Filter theo guide (optional)</param>
        /// <param name="fromDate">Filter từ ngày (optional)</param>
        /// <param name="toDate">Filter đến ngày (optional)</param>
        /// <param name="includeInactive">Có bao gồm operations không active không</param>
        /// <returns>Tuple chứa danh sách tour operations và tổng số records</returns>
        Task<(IEnumerable<TourOperation> Operations, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            Guid? guideId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            bool includeInactive = false);

        /// <summary>
        /// Lấy thống kê tour operations theo guide
        /// </summary>
        /// <param name="guideId">ID của guide</param>
        /// <param name="fromDate">Từ ngày (optional)</param>
        /// <param name="toDate">Đến ngày (optional)</param>
        /// <returns>Object chứa thống kê</returns>
        Task<object> GetGuideStatisticsAsync(Guid guideId, DateOnly? fromDate = null, DateOnly? toDate = null);

        /// <summary>
        /// Lấy danh sách guides có sẵn trong ngày cụ thể
        /// </summary>
        /// <param name="tourDate">Ngày tour</param>
        /// <returns>Danh sách guide IDs có sẵn</returns>
        Task<IEnumerable<Guid>> GetAvailableGuidesOnDateAsync(DateOnly tourDate);

        /// <summary>
        /// Đếm số lượng tour operations của guide trong tháng
        /// </summary>
        /// <param name="guideId">ID của guide</param>
        /// <param name="year">Năm</param>
        /// <param name="month">Tháng</param>
        /// <param name="includeInactive">Có bao gồm operations không active không</param>
        /// <returns>Số lượng tour operations</returns>
        Task<int> CountByGuideAndMonthAsync(Guid guideId, int year, int month, bool includeInactive = false);

        /// <summary>
        /// Kiểm tra xem tour operation có thể được xóa không
        /// </summary>
        /// <param name="id">ID của tour operation</param>
        /// <returns>True nếu có thể xóa</returns>
        Task<bool> CanDeleteOperationAsync(Guid id);

        /// <summary>
        /// Lấy danh sách operations với filtering
        /// </summary>
        Task<IEnumerable<TourOperation>> GetOperationsAsync(
            Guid? tourTemplateId = null,
            Guid? guideId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool includeInactive = false);

        /// <summary>
        /// Lấy operations theo ngày cụ thể
        /// </summary>
        Task<IEnumerable<TourOperation>> GetOperationsByDateAsync(DateOnly date);
    }
}
