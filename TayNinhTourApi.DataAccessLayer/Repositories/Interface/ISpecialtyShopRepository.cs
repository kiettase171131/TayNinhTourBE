using System.Xml.Linq;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho SpecialtyShop entity
    /// Cung cấp các methods để truy cập và thao tác với dữ liệu SpecialtyShop
    /// </summary>
    public interface ISpecialtyShopRepository : IGenericRepository<SpecialtyShop>
    {
        /// <summary>
        /// Lấy SpecialtyShop theo UserId (1:1 relationship)
        /// </summary>
        /// <param name="userId">ID của User</param>
        /// <returns>SpecialtyShop entity hoặc null nếu không tìm thấy</returns>
        Task<SpecialtyShop?> GetByUserIdAsync(Guid userId);

        /// <summary>
        /// Lấy danh sách tất cả SpecialtyShops đang hoạt động
        /// </summary>
        /// <returns>Danh sách SpecialtyShops có IsShopActive = true và IsActive = true</returns>
        Task<IEnumerable<SpecialtyShop>> GetActiveShopsAsync(string? name = null);

        /// <summary>
        /// Lấy danh sách SpecialtyShops theo loại shop
        /// </summary>
        /// <param name="shopType">Loại shop (Souvenir, Food, Craft, etc.)</param>
        /// <returns>Danh sách SpecialtyShops theo loại và đang hoạt động</returns>
        Task<IEnumerable<SpecialtyShop>> GetShopsByTypeAsync(string shopType);

        /// <summary>
        /// Kiểm tra xem User đã có SpecialtyShop chưa
        /// </summary>
        /// <param name="userId">ID của User</param>
        /// <returns>True nếu User đã có SpecialtyShop, False nếu chưa</returns>
        Task<bool> ExistsByUserIdAsync(Guid userId);

        /// <summary>
        /// Lấy danh sách SpecialtyShops với phân trang
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số lượng items per page</param>
        /// <param name="isActiveOnly">Chỉ lấy shops đang hoạt động</param>
        /// <returns>Danh sách SpecialtyShops với phân trang</returns>
        Task<(IEnumerable<SpecialtyShop> Items, int TotalCount)> GetPagedAsync(int pageIndex, int pageSize, bool isActiveOnly = true,string ? name = null);

        /// <summary>
        /// Tìm kiếm SpecialtyShops theo tên hoặc địa điểm
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <param name="isActiveOnly">Chỉ tìm trong shops đang hoạt động</param>
        /// <returns>Danh sách SpecialtyShops phù hợp với từ khóa</returns>
        Task<IEnumerable<SpecialtyShop>> SearchAsync(string searchTerm, bool isActiveOnly = true);

        /// <summary>
        /// Lấy SpecialtyShops theo rating tối thiểu
        /// </summary>
        /// <param name="minRating">Rating tối thiểu</param>
        /// <param name="isActiveOnly">Chỉ lấy shops đang hoạt động</param>
        /// <returns>Danh sách SpecialtyShops có rating >= minRating</returns>
        Task<IEnumerable<SpecialtyShop>> GetShopsByMinRatingAsync(decimal minRating, bool isActiveOnly = true);
    }
}
