using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho TourTemplate management
    /// Cung cấp business logic cho việc quản lý tour templates
    /// </summary>
    public interface ITourTemplateService
    {
        /// <summary>
        /// Tạo tour template mới
        /// </summary>
        /// <param name="request">Thông tin tour template</param>
        /// <param name="createdById">ID của user tạo</param>
        /// <returns>Tour template đã tạo</returns>
        Task<TourTemplate> CreateTourTemplateAsync(RequestCreateTourTemplateDto request, Guid createdById);

        /// <summary>
        /// Cập nhật tour template
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <param name="updatedById">ID của user cập nhật</param>
        /// <returns>Tour template đã cập nhật</returns>
        Task<TourTemplate?> UpdateTourTemplateAsync(Guid id, RequestUpdateTourTemplateDto request, Guid updatedById);

        /// <summary>
        /// Xóa tour template (soft delete)
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <param name="deletedById">ID của user xóa</param>
        /// <returns>True nếu xóa thành công</returns>
        Task<bool> DeleteTourTemplateAsync(Guid id, Guid deletedById);

        /// <summary>
        /// Lấy tour template theo ID
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <returns>Tour template</returns>
        Task<TourTemplate?> GetTourTemplateByIdAsync(Guid id);

        /// <summary>
        /// Lấy tour template với đầy đủ thông tin
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <returns>Tour template với relationships</returns>
        Task<TourTemplate?> GetTourTemplateWithDetailsAsync(Guid id);

        /// <summary>
        /// Lấy danh sách tour templates theo user tạo
        /// </summary>
        /// <param name="createdById">ID của user tạo</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates</returns>
        Task<IEnumerable<TourTemplate>> GetTourTemplatesByCreatedByAsync(Guid createdById, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour templates theo loại
        /// </summary>
        /// <param name="templateType">Loại template</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates</returns>
        Task<IEnumerable<TourTemplate>> GetTourTemplatesByTypeAsync(TourTemplateType templateType, bool includeInactive = false);

        /// <summary>
        /// Tìm kiếm tour templates
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates</returns>
        Task<IEnumerable<TourTemplate>> SearchTourTemplatesAsync(string keyword, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour templates với pagination
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="templateType">Loại template (optional)</param>
        /// <param name="minPrice">Giá tối thiểu (optional)</param>
        /// <param name="maxPrice">Giá tối đa (optional)</param>
        /// <param name="startLocation">Điểm khởi hành (optional)</param>
        /// <param name="includeInactive">Có bao gồm templates không active không</param>
        /// <returns>Danh sách tour templates với pagination info</returns>
        Task<ResponseGetTourTemplatesDto> GetTourTemplatesPaginatedAsync(
            int pageIndex, 
            int pageSize, 
            TourTemplateType? templateType = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? startLocation = null,
            bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour templates phổ biến
        /// </summary>
        /// <param name="top">Số lượng templates lấy về</param>
        /// <returns>Danh sách tour templates phổ biến</returns>
        Task<IEnumerable<TourTemplate>> GetPopularTourTemplatesAsync(int top = 10);

        /// <summary>
        /// Kích hoạt/vô hiệu hóa tour template
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <param name="isActive">Trạng thái active</param>
        /// <param name="updatedById">ID của user cập nhật</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> SetTourTemplateActiveStatusAsync(Guid id, bool isActive, Guid updatedById);

        /// <summary>
        /// Sao chép tour template
        /// </summary>
        /// <param name="id">ID của tour template gốc</param>
        /// <param name="newTitle">Tiêu đề mới</param>
        /// <param name="createdById">ID của user tạo</param>
        /// <returns>Tour template đã sao chép</returns>
        Task<TourTemplate?> CopyTourTemplateAsync(Guid id, string newTitle, Guid createdById);

        /// <summary>
        /// Kiểm tra xem tour template có thể xóa không
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <returns>True nếu có thể xóa</returns>
        Task<bool> CanDeleteTourTemplateAsync(Guid id);

        /// <summary>
        /// Lấy thống kê tour templates
        /// </summary>
        /// <param name="createdById">ID của user (optional, nếu null thì lấy tất cả)</param>
        /// <returns>Thống kê tour templates</returns>
        Task<object> GetTourTemplateStatisticsAsync(Guid? createdById = null);
    }
}
