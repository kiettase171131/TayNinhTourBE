using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Voucher;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Voucher;
using TayNinhTourApi.BusinessLogicLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho SpecialtyShop business logic
    /// Cung cấp các methods để quản lý SpecialtyShop data
    /// </summary>
    public interface ISpecialtyShopService
    {
        /// <summary>
        /// Lấy thông tin shop của user hiện tại
        /// Chỉ user có role "Specialty Shop" mới có thể gọi
        /// </summary>
        /// <param name="currentUser">Thông tin user hiện tại</param>
        /// <returns>Thông tin SpecialtyShop của user</returns>
        Task<ApiResponse<SpecialtyShopResponseDto>> GetMyShopAsync(CurrentUserObject currentUser);

        /// <summary>
        /// Cập nhật thông tin shop của user hiện tại
        /// Chỉ user có role "Specialty Shop" mới có thể gọi
        /// </summary>
        /// <param name="updateDto">Dữ liệu cập nhật</param>
        /// <param name="currentUser">Thông tin user hiện tại</param>
        /// <returns>Thông tin SpecialtyShop sau khi cập nhật</returns>
        Task<ApiResponse<SpecialtyShopResponseDto>> UpdateMyShopAsync(UpdateSpecialtyShopDto updateDto, CurrentUserObject currentUser);

        /// <summary>
        /// Lấy danh sách tất cả shops đang hoạt động
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <returns>Danh sách SpecialtyShops đang hoạt động</returns>
        Task<ApiResponse<List<SpecialtyShopResponseDto>>> GetAllActiveShopsAsync();

        /// <summary>
        /// Lấy danh sách shops theo loại
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <param name="shopType">Loại shop (Souvenir, Food, Craft, etc.)</param>
        /// <returns>Danh sách SpecialtyShops theo loại</returns>
        Task<ApiResponse<List<SpecialtyShopResponseDto>>> GetShopsByTypeAsync(string shopType);

        /// <summary>
        /// Lấy thông tin chi tiết của một shop theo ID
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <param name="shopId">ID của SpecialtyShop</param>
        /// <returns>Thông tin chi tiết SpecialtyShop</returns>
        Task<ApiResponse<SpecialtyShopResponseDto>> GetShopByIdAsync(Guid shopId);

        /// <summary>
        /// Tìm kiếm shops theo từ khóa
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <returns>Danh sách SpecialtyShops phù hợp</returns>
        Task<ApiResponse<List<SpecialtyShopResponseDto>>> SearchShopsAsync(string searchTerm);

        /// <summary>
        /// Lấy danh sách shops với phân trang
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số lượng items per page</param>
        /// <returns>Danh sách SpecialtyShops với thông tin phân trang</returns>
        Task<ApiResponse<PagedResult<SpecialtyShopResponseDto>>> GetPagedShopsAsync(int pageIndex, int pageSize);

        /// <summary>
        /// Lấy danh sách shops theo rating tối thiểu
        /// Public endpoint, không cần authentication
        /// </summary>
        /// <param name="minRating">Rating tối thiểu (1-5)</param>
        /// <returns>Danh sách SpecialtyShops có rating >= minRating</returns>
        Task<ApiResponse<List<SpecialtyShopResponseDto>>> GetShopsByMinRatingAsync(decimal minRating);

        // ========== TIMELINE INTEGRATION METHODS ==========

        /// <summary>
        /// Lấy danh sách SpecialtyShops với pagination và filters cho timeline integration
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="textSearch">Từ khóa tìm kiếm (tên hoặc mô tả shop)</param>
        /// <param name="location">Filter theo location (optional)</param>
        /// <param name="shopType">Filter theo shop type (optional)</param>
        /// <param name="status">Filter theo status (true: active, false: inactive, null: all)</param>
        /// <returns>Danh sách SpecialtyShops với pagination info</returns>
        Task<ApiResponse<PagedResult<SpecialtyShopResponseDto>>> GetShopsForTimelineAsync(
            int? pageIndex,
            int? pageSize,
            string? textSearch = null,
            string? location = null,
            string? shopType = null,
            bool? status = null);

        /// <summary>
        /// Lấy SpecialtyShop theo ID cho timeline integration
        /// </summary>
        /// <param name="id">ID của SpecialtyShop</param>
        /// <returns>Thông tin chi tiết SpecialtyShop</returns>
        Task<ApiResponse<SpecialtyShopResponseDto>> GetShopByIdForTimelineAsync(Guid id);
        
    }

    /// <summary>
    /// Helper class cho phân trang
    /// </summary>
    /// <typeparam name="T">Type của data items</typeparam>
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }
}
