using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho Shop entity
    /// Kế thừa từ IGenericRepository và thêm các methods specific cho Shop
    /// </summary>
    public interface IShopRepository : IGenericRepository<Shop>
    {
        /// <summary>
        /// Lấy danh sách shops theo location
        /// </summary>
        /// <param name="location">Địa điểm cần tìm</param>
        /// <param name="includeInactive">Có bao gồm shops không active không</param>
        /// <returns>Danh sách shops</returns>
        Task<IEnumerable<Shop>> GetByLocationAsync(string location, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách shops theo loại shop
        /// </summary>
        /// <param name="shopType">Loại shop (Souvenir, Food, Craft, etc.)</param>
        /// <param name="includeInactive">Có bao gồm shops không active không</param>
        /// <returns>Danh sách shops</returns>
        Task<IEnumerable<Shop>> GetByShopTypeAsync(string shopType, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách shops theo rating tối thiểu
        /// </summary>
        /// <param name="minRating">Rating tối thiểu (1-5)</param>
        /// <param name="includeInactive">Có bao gồm shops không active không</param>
        /// <returns>Danh sách shops</returns>
        Task<IEnumerable<Shop>> GetByMinRatingAsync(decimal minRating, bool includeInactive = false);

        /// <summary>
        /// Tìm kiếm shops theo tên
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="includeInactive">Có bao gồm shops không active không</param>
        /// <returns>Danh sách shops</returns>
        Task<IEnumerable<Shop>> SearchByNameAsync(string keyword, bool includeInactive = false);

        /// <summary>
        /// Lấy shop với đầy đủ thông tin relationships
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <returns>Shop với relationships</returns>
        Task<Shop?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Lấy danh sách shops được sử dụng trong tour templates
        /// </summary>
        /// <param name="includeInactive">Có bao gồm shops không active không</param>
        /// <returns>Danh sách shops đang được sử dụng</returns>
        Task<IEnumerable<Shop>> GetShopsInUseAsync(bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách shops với pagination và filtering
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="location">Filter theo location (optional)</param>
        /// <param name="shopType">Filter theo shop type (optional)</param>
        /// <param name="minRating">Filter theo rating tối thiểu (optional)</param>
        /// <param name="includeInactive">Có bao gồm shops không active không</param>
        /// <returns>Tuple chứa danh sách shops và tổng số records</returns>
        Task<(IEnumerable<Shop> Shops, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            string? location = null,
            string? shopType = null,
            decimal? minRating = null,
            bool includeInactive = false);

        /// <summary>
        /// Kiểm tra xem shop có đang được sử dụng trong tour details không
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <returns>True nếu đang được sử dụng</returns>
        Task<bool> IsShopInUseAsync(Guid id);

        /// <summary>
        /// Lấy danh sách shops theo user tạo
        /// </summary>
        /// <param name="createdById">ID của user tạo</param>
        /// <param name="includeInactive">Có bao gồm shops không active không</param>
        /// <returns>Danh sách shops</returns>
        Task<IEnumerable<Shop>> GetByCreatedByAsync(Guid createdById, bool includeInactive = false);
    }
}
