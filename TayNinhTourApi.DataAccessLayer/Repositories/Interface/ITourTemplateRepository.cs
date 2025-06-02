using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourTemplate entity
    /// Kế thừa từ IGenericRepository và thêm các methods specific cho TourTemplate
    /// </summary>
    public interface ITourTemplateRepository : IGenericRepository<TourTemplate>
    {
        /// <summary>
        /// Lấy danh sách tour templates theo loại template
        /// </summary>
        /// <param name="templateType">Loại template</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates</returns>
        Task<IEnumerable<TourTemplate>> GetByTemplateTypeAsync(TourTemplateType templateType, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour templates theo user tạo
        /// </summary>
        /// <param name="createdById">ID của user tạo</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates</returns>
        Task<IEnumerable<TourTemplate>> GetByCreatedByAsync(Guid createdById, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour templates theo khoảng giá
        /// </summary>
        /// <param name="minPrice">Giá tối thiểu</param>
        /// <param name="maxPrice">Giá tối đa</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates</returns>
        Task<IEnumerable<TourTemplate>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour templates theo điểm khởi hành
        /// </summary>
        /// <param name="startLocation">Điểm khởi hành</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates</returns>
        Task<IEnumerable<TourTemplate>> GetByStartLocationAsync(string startLocation, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour templates theo điểm kết thúc
        /// </summary>
        /// <param name="endLocation">Điểm kết thúc</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates</returns>
        Task<IEnumerable<TourTemplate>> GetByEndLocationAsync(string endLocation, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour templates theo ngày trong tuần có thể tổ chức
        /// </summary>
        /// <param name="scheduleDay">Ngày trong tuần</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates</returns>
        Task<IEnumerable<TourTemplate>> GetByScheduleDayAsync(ScheduleDay scheduleDay, bool includeInactive = false);

        /// <summary>
        /// Tìm kiếm tour templates theo từ khóa
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates</returns>
        Task<IEnumerable<TourTemplate>> SearchAsync(string keyword, bool includeInactive = false);

        /// <summary>
        /// Lấy tour template với đầy đủ thông tin relationships
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <returns>Tour template với relationships</returns>
        Task<TourTemplate?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Lấy danh sách tour templates phổ biến (được sử dụng nhiều)
        /// </summary>
        /// <param name="top">Số lượng templates lấy về</param>
        /// <returns>Danh sách tour templates phổ biến</returns>
        Task<IEnumerable<TourTemplate>> GetPopularTemplatesAsync(int top = 10);

        /// <summary>
        /// Kiểm tra xem tour template có đang được sử dụng không
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <returns>True nếu đang được sử dụng</returns>
        Task<bool> IsTemplateInUseAsync(Guid id);

        /// <summary>
        /// Lấy danh sách tour templates với pagination và filter
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="templateType">Loại template (optional)</param>
        /// <param name="minPrice">Giá tối thiểu (optional)</param>
        /// <param name="maxPrice">Giá tối đa (optional)</param>
        /// <param name="startLocation">Điểm khởi hành (optional)</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates với pagination</returns>
        Task<(IEnumerable<TourTemplate> Templates, int TotalCount)> GetPaginatedAsync(
            int pageIndex, 
            int pageSize, 
            TourTemplateType? templateType = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? startLocation = null,
            bool includeInactive = false);
    }
}
