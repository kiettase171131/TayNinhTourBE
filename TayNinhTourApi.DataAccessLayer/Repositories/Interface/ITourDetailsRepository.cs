using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourDetails entity
    /// Kế thừa từ IGenericRepository và thêm các methods specific cho TourDetails
    /// </summary>
    public interface ITourDetailsRepository : IGenericRepository<TourDetails>
    {
        /// <summary>
        /// Lấy danh sách tour details theo tour template, sắp xếp theo thời gian
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="includeInactive">Có bao gồm details không active không</param>
        /// <returns>Danh sách tour details được sắp xếp theo SortOrder và TimeSlot</returns>
        Task<IEnumerable<TourDetails>> GetByTourTemplateOrderedAsync(Guid tourTemplateId, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour details theo shop
        /// </summary>
        /// <param name="shopId">ID của shop</param>
        /// <param name="includeInactive">Có bao gồm details không active không</param>
        /// <returns>Danh sách tour details</returns>
        Task<IEnumerable<TourDetails>> GetByShopAsync(Guid shopId, bool includeInactive = false);

        /// <summary>
        /// Lấy tour detail với đầy đủ thông tin relationships
        /// </summary>
        /// <param name="id">ID của tour detail</param>
        /// <returns>Tour detail với relationships</returns>
        Task<TourDetails?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Lấy tour detail theo tour template và sort order
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="sortOrder">Thứ tự sắp xếp</param>
        /// <returns>Tour detail nếu tồn tại</returns>
        Task<TourDetails?> GetByTemplateAndSortOrderAsync(Guid tourTemplateId, int sortOrder);

        /// <summary>
        /// Lấy tour detail cuối cùng trong timeline của tour template
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <returns>Tour detail có SortOrder cao nhất</returns>
        Task<TourDetails?> GetLastDetailAsync(Guid tourTemplateId);

        /// <summary>
        /// Lấy tour detail đầu tiên trong timeline của tour template
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <returns>Tour detail có SortOrder thấp nhất</returns>
        Task<TourDetails?> GetFirstDetailAsync(Guid tourTemplateId);

        /// <summary>
        /// Lấy danh sách tour details trong khoảng thời gian
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="startTime">Thời gian bắt đầu</param>
        /// <param name="endTime">Thời gian kết thúc</param>
        /// <param name="includeInactive">Có bao gồm details không active không</param>
        /// <returns>Danh sách tour details</returns>
        Task<IEnumerable<TourDetails>> GetByTimeRangeAsync(Guid tourTemplateId, TimeOnly startTime, TimeOnly endTime, bool includeInactive = false);

        /// <summary>
        /// Kiểm tra xem có tour detail nào với sort order cụ thể không
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="sortOrder">Thứ tự sắp xếp</param>
        /// <param name="excludeId">ID cần loại trừ (optional, dùng khi update)</param>
        /// <returns>True nếu đã tồn tại</returns>
        Task<bool> ExistsBySortOrderAsync(Guid tourTemplateId, int sortOrder, Guid? excludeId = null);

        /// <summary>
        /// Lấy số lượng tour details của tour template
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="includeInactive">Có bao gồm details không active không</param>
        /// <returns>Số lượng tour details</returns>
        Task<int> CountByTourTemplateAsync(Guid tourTemplateId, bool includeInactive = false);

        /// <summary>
        /// Cập nhật sort order cho các tour details sau khi insert/delete
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="fromSortOrder">Từ sort order nào</param>
        /// <param name="increment">Số lượng tăng/giảm (có thể âm)</param>
        /// <returns>Task</returns>
        Task UpdateSortOrdersAsync(Guid tourTemplateId, int fromSortOrder, int increment);

        /// <summary>
        /// Lấy danh sách tour details với pagination
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="tourTemplateId">Filter theo tour template (optional)</param>
        /// <param name="shopId">Filter theo shop (optional)</param>
        /// <param name="includeInactive">Có bao gồm details không active không</param>
        /// <returns>Tuple chứa danh sách tour details và tổng số records</returns>
        Task<(IEnumerable<TourDetails> Details, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            Guid? tourTemplateId = null,
            Guid? shopId = null,
            bool includeInactive = false);

        /// <summary>
        /// Tìm kiếm tour details theo location hoặc description
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="tourTemplateId">Filter theo tour template (optional)</param>
        /// <param name="includeInactive">Có bao gồm details không active không</param>
        /// <returns>Danh sách tour details</returns>
        Task<IEnumerable<TourDetails>> SearchAsync(string keyword, Guid? tourTemplateId = null, bool includeInactive = false);
    }
}
