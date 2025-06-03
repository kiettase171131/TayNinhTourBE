using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho quản lý Shop operations
    /// Cung cấp các chức năng CRUD và business logic cho Shop entity
    /// </summary>
    public interface IShopService
    {
        /// <summary>
        /// Lấy danh sách shops với pagination và filtering
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (bắt đầu từ 1)</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="textSearch">Từ khóa tìm kiếm (tên hoặc mô tả shop)</param>
        /// <param name="location">Filter theo location (optional)</param>
        /// <param name="shopType">Filter theo shop type (optional)</param>
        /// <param name="status">Filter theo status (true: active, false: inactive, null: all)</param>
        /// <returns>Danh sách shops với pagination info</returns>
        Task<ResponseGetShopsDto> GetShopsAsync(
            int? pageIndex, 
            int? pageSize, 
            string? textSearch = null,
            string? location = null,
            string? shopType = null,
            bool? status = null);

        /// <summary>
        /// Lấy thông tin chi tiết shop theo ID
        /// </summary>
        /// <param name="id">ID của shop</param>
        /// <returns>Thông tin chi tiết shop</returns>
        Task<ResponseGetShopByIdDto> GetShopByIdAsync(Guid id);

        /// <summary>
        /// Tạo shop mới
        /// </summary>
        /// <param name="request">Thông tin shop cần tạo</param>
        /// <param name="currentUserObject">Thông tin user hiện tại</param>
        /// <returns>Thông tin shop vừa được tạo</returns>
        Task<ResponseCreateShopDto> CreateShopAsync(RequestCreateShopDto request, CurrentUserObject currentUserObject);

        /// <summary>
        /// Cập nhật thông tin shop
        /// </summary>
        /// <param name="request">Thông tin cần cập nhật</param>
        /// <param name="id">ID của shop cần cập nhật</param>
        /// <param name="currentUserObject">Thông tin user hiện tại</param>
        /// <returns>Kết quả cập nhật</returns>
        Task<BaseResposeDto> UpdateShopAsync(RequestUpdateShopDto request, Guid id, CurrentUserObject currentUserObject);

        /// <summary>
        /// Xóa shop (soft delete)
        /// </summary>
        /// <param name="id">ID của shop cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        Task<BaseResposeDto> DeleteShopAsync(Guid id);

        /// <summary>
        /// Lấy danh sách shops active cho dropdown/select
        /// </summary>
        /// <param name="location">Filter theo location (optional)</param>
        /// <param name="shopType">Filter theo shop type (optional)</param>
        /// <returns>Danh sách shops active dạng summary</returns>
        Task<ResponseGetActiveShopsDto> GetActiveShopsAsync(string? location = null, string? shopType = null);
    }
}
