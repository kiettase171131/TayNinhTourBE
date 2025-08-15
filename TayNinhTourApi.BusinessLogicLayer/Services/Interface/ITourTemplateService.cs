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
        /// <returns>Response với tour template đã tạo</returns>
        Task<ResponseCreateTourTemplateDto> CreateTourTemplateAsync(RequestCreateTourTemplateDto request, Guid createdById);

        /// <summary>
        /// Cập nhật tour template
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <param name="updatedById">ID của user cập nhật</param>
        /// <returns>Response với tour template đã cập nhật</returns>
        Task<ResponseUpdateTourTemplateDto> UpdateTourTemplateAsync(Guid id, RequestUpdateTourTemplateDto request, Guid updatedById);

        /// <summary>
        /// Xóa tour template (soft delete)
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <param name="deletedById">ID của user xóa</param>
        /// <returns>Response với kết quả xóa</returns>
        Task<ResponseDeleteTourTemplateDto> DeleteTourTemplateAsync(Guid id, Guid deletedById);

        /// <summary>
        /// Lấy tour template theo ID
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <returns>Response với tour template</returns>
        Task<ResponseGetTourTemplateDto> GetTourTemplateByIdAsync(Guid id);

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
        /// Sao chép tour template
        /// </summary>
        /// <param name="id">ID của tour template gốc</param>
        /// <param name="newTitle">Tiêu đề mới</param>
        /// <param name="createdById">ID của user tạo</param>
        /// <returns>Response với tour template đã sao chép</returns>
        Task<ResponseCopyTourTemplateDto> CopyTourTemplateAsync(Guid id, string newTitle, Guid createdById);

        /// <summary>
        /// Kiểm tra xem tour template có thể xóa không
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <returns>Response với thông tin có thể xóa</returns>
        Task<ResponseCanDeleteDto> CanDeleteTourTemplateAsync(Guid id);

        /// <summary>
        /// Lấy thống kê tour templates
        /// </summary>
        /// <param name="createdById">ID của user (optional, nếu null thì lấy tất cả)</param>
        /// <returns>Response với thống kê tour templates</returns>
        Task<ResponseTourTemplateStatisticsDto> GetTourTemplateStatisticsAsync(Guid? createdById = null);

        /// <summary>
        /// Kích hoạt/vô hiệu hóa tour template
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <param name="isActive">Trạng thái active</param>
        /// <param name="updatedById">ID của user cập nhật</param>
        /// <returns>Response với kết quả cập nhật</returns>
        Task<ResponseSetActiveStatusDto> SetTourTemplateActiveStatusAsync(Guid id, bool isActive, Guid updatedById);

        /// <summary>
        /// Validate tour template data
        /// </summary>
        /// <param name="request">Request data để validate</param>
        /// <returns>Response với kết quả validation</returns>
        Task<ResponseValidationDto> ValidateCreateRequestAsync(RequestCreateTourTemplateDto request);

        /// <summary>
        /// Validate tour template update data
        /// </summary>
        /// <param name="id">ID của tour template</param>
        /// <param name="request">Request data để validate</param>
        /// <returns>Response với kết quả validation</returns>
        Task<ResponseValidationDto> ValidateUpdateRequestAsync(Guid id, RequestUpdateTourTemplateDto request);

        /// <summary>
        /// Tự động tạo tour slots cho template
        /// </summary>
        /// <param name="templateId">ID của tour template</param>
        /// <param name="month">Tháng</param>
        /// <param name="year">Năm</param>
        /// <param name="overwriteExisting">Có ghi đè slots đã tồn tại không</param>
        /// <param name="autoActivate">Có tự động kích hoạt slots không</param>
        /// <returns>Response với thông tin slots đã tạo</returns>
        Task<(bool success, string Message, int CreatedSlotsCount)> GenerateSlotsForTemplateAsync(
            Guid templateId, int month, int year, bool overwriteExisting = false, bool autoActivate = true);

        /// <summary>
        /// Tạo tour template ngày lễ với ngày cụ thể
        /// Tạo template và tự động tạo 1 slot duy nhất cho ngày được chọn
        /// </summary>
        /// <param name="request">Request chứa thông tin holiday template</param>
        /// <param name="createdById">ID của user tạo template</param>
        /// <returns>Response với thông tin template và slot đã tạo</returns>
        Task<ResponseCreateTourTemplateDto> CreateHolidayTourTemplateAsync(RequestCreateHolidayTourTemplateDto request, Guid createdById);
    }
}
