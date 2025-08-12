    using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Interface cho TimelineItem repository
    /// Định nghĩa các method cần thiết cho việc quản lý TimelineItem
    /// </summary>
    public interface ITimelineItemRepository : IGenericRepository<TimelineItem>
    {
        /// <summary>
        /// Lấy danh sách timeline items theo TourDetails ID, sắp xếp theo SortOrder và CheckInTime
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="includeInactive">Có bao gồm items không active không</param>
        /// <returns>Danh sách timeline items đã sắp xếp</returns>
        Task<IEnumerable<TimelineItem>> GetByTourDetailsOrderedAsync(Guid tourDetailsId, bool includeInactive = false);

        /// <summary>
        /// Lấy timeline item với đầy đủ thông tin liên quan
        /// </summary>
        /// <param name="id">ID của timeline item</param>
        /// <returns>Timeline item với thông tin chi tiết hoặc null nếu không tìm thấy</returns>
        Task<TimelineItem?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Lấy danh sách timeline items theo SpecialtyShop ID
        /// </summary>
        /// <param name="specialtyShopId">ID của SpecialtyShop</param>
        /// <param name="includeInactive">Có bao gồm items không active không</param>
        /// <returns>Danh sách timeline items liên quan đến specialty shop</returns>
        Task<IEnumerable<TimelineItem>> GetBySpecialtyShopAsync(Guid specialtyShopId, bool includeInactive = false);

        /// <summary>
        /// Lấy SortOrder lớn nhất trong một TourDetails
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>SortOrder lớn nhất, hoặc 0 nếu không có items</returns>
        Task<int> GetMaxSortOrderAsync(Guid tourDetailsId);

        /// <summary>
        /// Kiểm tra xem SortOrder đã tồn tại trong TourDetails chưa
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="sortOrder">SortOrder cần kiểm tra</param>
        /// <param name="excludeId">ID của timeline item cần loại trừ khỏi kiểm tra</param>
        /// <returns>True nếu SortOrder đã tồn tại</returns>
        Task<bool> ExistsBySortOrderAsync(Guid tourDetailsId, int sortOrder, Guid? excludeId = null);

        /// <summary>
        /// Cập nhật SortOrder cho các timeline items
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="fromSortOrder">SortOrder bắt đầu cập nhật</param>
        /// <param name="increment">Số lượng tăng/giảm</param>
        Task UpdateSortOrdersAsync(Guid tourDetailsId, int fromSortOrder, int increment);

        /// <summary>
        /// Tìm kiếm timeline items theo từ khóa
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="tourDetailsId">ID của TourDetails (optional)</param>
        /// <param name="includeInactive">Có bao gồm items không active không</param>
        /// <returns>Danh sách timeline items phù hợp</returns>
        Task<IEnumerable<TimelineItem>> SearchAsync(string keyword, Guid? tourDetailsId = null, bool includeInactive = false);

        /// <summary>
        /// Kiểm tra xem SpecialtyShop đã được sử dụng trong timeline của TourDetails chưa
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="specialtyShopId">ID của SpecialtyShop cần kiểm tra</param>
        /// <param name="excludeTimelineItemId">ID của timeline item cần loại trừ khỏi kiểm tra (cho trường hợp update)</param>
        /// <returns>True nếu shop đã được sử dụng, False nếu chưa</returns>
        Task<bool> IsSpecialtyShopUsedInTimelineAsync(Guid tourDetailsId, Guid specialtyShopId, Guid? excludeTimelineItemId = null);

        /// <summary>
        /// Lấy danh sách SpecialtyShop IDs đã được sử dụng trong timeline của TourDetails
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="excludeTimelineItemId">ID của timeline item cần loại trừ khỏi danh sách (cho trường hợp update)</param>
        /// <returns>Danh sách SpecialtyShop IDs đã được sử dụng</returns>
        Task<IEnumerable<Guid>> GetUsedSpecialtyShopIdsAsync(Guid tourDetailsId, Guid? excludeTimelineItemId = null);
    }
}
