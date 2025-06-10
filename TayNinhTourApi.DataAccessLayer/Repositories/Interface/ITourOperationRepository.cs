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
        /// Lấy tour operation theo tour details
        /// </summary>
        /// <param name="tourDetailsId">ID của tour details</param>
        /// <returns>Tour operation nếu tồn tại</returns>
        Task<TourOperation?> GetByTourDetailsAsync(Guid tourDetailsId);

        /// <summary>
        /// Lấy danh sách tour operations theo guide
        /// </summary>
        /// <param name="guideId">ID của guide</param>
        /// <param name="includeInactive">Có bao gồm operations không active không</param>
        /// <returns>Danh sách tour operations</returns>
        Task<IEnumerable<TourOperation>> GetByGuideAsync(Guid guideId, bool includeInactive = false);

        /// <summary>
        /// Lấy tour operation với đầy đủ thông tin relationships
        /// </summary>
        /// <param name="id">ID của tour operation</param>
        /// <returns>Tour operation với relationships</returns>
        Task<TourOperation?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Lấy danh sách tour operations với pagination và filtering
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="guideId">Filter theo guide (optional)</param>
        /// <param name="tourTemplateId">Filter theo tour template (optional)</param>
        /// <param name="includeInactive">Có bao gồm operations không active không</param>
        /// <returns>Tuple chứa danh sách tour operations và tổng số records</returns>
        Task<(IEnumerable<TourOperation> Operations, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            Guid? guideId = null,
            Guid? tourTemplateId = null,
            bool includeInactive = false);

        /// <summary>
        /// Kiểm tra xem tour operation có thể được xóa không
        /// </summary>
        /// <param name="id">ID của tour operation</param>
        /// <returns>True nếu có thể xóa</returns>
        Task<bool> CanDeleteOperationAsync(Guid id);

        /// <summary>
        /// Lấy danh sách tour operations theo ngày
        /// </summary>
        /// <param name="date">Ngày cần lấy operations</param>
        /// <param name="includeInactive">Có bao gồm operations không active không</param>
        /// <returns>Danh sách tour operations trong ngày</returns>
        Task<IEnumerable<TourOperation>> GetOperationsByDateAsync(DateOnly date, bool includeInactive = false);
    }
}
