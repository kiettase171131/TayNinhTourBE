using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourDetails entity
    /// Kế thừa từ IGenericRepository và thêm các methods specific cho TourDetails
    /// </summary>
    public interface ITourDetailsRepository : IGenericRepository<TourDetails>
    {
        /// <summary>
        /// Lấy danh sách tour details theo tour template, sắp xếp theo title
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="includeInactive">Có bao gồm details không active không</param>
        /// <returns>Danh sách tour details được sắp xếp theo Title</returns>
        Task<IEnumerable<TourDetails>> GetByTourTemplateOrderedAsync(Guid tourTemplateId, bool includeInactive = false);

        /// <summary>
        /// Lấy tour detail với đầy đủ thông tin relationships
        /// </summary>
        /// <param name="id">ID của tour detail</param>
        /// <returns>Tour detail với relationships</returns>
        Task<TourDetails?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Tìm kiếm tour details theo title
        /// </summary>
        /// <param name="title">Title cần tìm</param>
        /// <param name="includeInactive">Có bao gồm details không active không</param>
        /// <returns>Danh sách tour details</returns>
        Task<IEnumerable<TourDetails>> SearchByTitleAsync(string title, bool includeInactive = false);

        /// <summary>
        /// Lấy tour detail theo tour template và title
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="title">Title của tour detail</param>
        /// <returns>Tour detail nếu tồn tại</returns>
        Task<TourDetails?> GetByTitleAsync(Guid tourTemplateId, string title);

        /// <summary>
        /// Kiểm tra xem có tour detail nào với title cụ thể không
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="title">Title cần kiểm tra</param>
        /// <param name="excludeId">ID cần loại trừ (optional, dùng khi update)</param>
        /// <returns>True nếu đã tồn tại</returns>
        Task<bool> ExistsByTitleAsync(Guid tourTemplateId, string title, Guid? excludeId = null);

        /// <summary>
        /// Lấy số lượng tour details của tour template
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="includeInactive">Có bao gồm details không active không</param>
        /// <returns>Số lượng tour details</returns>
        Task<int> CountByTourTemplateAsync(Guid tourTemplateId, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour details với pagination
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="tourTemplateId">Filter theo tour template (optional)</param>
        /// <param name="titleFilter">Filter theo title (optional)</param>
        /// <param name="includeInactive">Có bao gồm details không active không</param>
        /// <returns>Tuple chứa danh sách tour details và tổng số records</returns>
        Task<(IEnumerable<TourDetails> Details, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            Guid? tourTemplateId = null,
            string? titleFilter = null,
            bool includeInactive = false);

        /// <summary>
        /// Tìm kiếm tour details theo title hoặc description
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="tourTemplateId">Filter theo tour template (optional)</param>
        /// <param name="includeInactive">Có bao gồm details không active không</param>
        /// <returns>Danh sách tour details</returns>
        Task<IEnumerable<TourDetails>> SearchAsync(string keyword, Guid? tourTemplateId = null, bool includeInactive = false);

        /// <summary>
        /// Kiểm tra xem có thể xóa tour detail không
        /// </summary>
        /// <param name="id">ID của tour detail</param>
        /// <returns>True nếu có thể xóa</returns>
        Task<bool> CanDeleteDetailAsync(Guid id);
        
    }
}
