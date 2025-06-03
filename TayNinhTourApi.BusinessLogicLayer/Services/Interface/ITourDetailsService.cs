using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho quản lý timeline chi tiết của tour template
    /// Cung cấp các operations để thêm, sửa, xóa và sắp xếp lại timeline items
    /// </summary>
    public interface ITourDetailsService
    {
        /// <summary>
        /// Lấy full timeline cho tour template với sort order
        /// </summary>
        /// <param name="request">Request chứa TourTemplateId và các options</param>
        /// <returns>Timeline đầy đủ với thông tin shop</returns>
        Task<ResponseGetTimelineDto> GetTimelineAsync(RequestGetTimelineDto request);

        /// <summary>
        /// Thêm mốc thời gian mới vào timeline
        /// </summary>
        /// <param name="request">Thông tin tour detail cần tạo</param>
        /// <param name="createdById">ID của user tạo</param>
        /// <returns>Tour detail vừa được tạo</returns>
        Task<ResponseCreateTourDetailDto> AddTimelineItemAsync(RequestCreateTourDetailDto request, Guid createdById);

        /// <summary>
        /// Cập nhật thông tin timeline item
        /// </summary>
        /// <param name="tourDetailId">ID của tour detail cần cập nhật</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <param name="updatedById">ID của user cập nhật</param>
        /// <returns>Tour detail sau khi cập nhật</returns>
        Task<ResponseUpdateTourDetailDto> UpdateTimelineItemAsync(Guid tourDetailId, RequestUpdateTourDetailDto request, Guid updatedById);

        /// <summary>
        /// Xóa timeline item và tự động reorder các items còn lại
        /// </summary>
        /// <param name="tourDetailId">ID của tour detail cần xóa</param>
        /// <param name="deletedById">ID của user thực hiện xóa</param>
        /// <returns>Kết quả xóa</returns>
        Task<ResponseDeleteTourDetailDto> DeleteTimelineItemAsync(Guid tourDetailId, Guid deletedById);

        /// <summary>
        /// Sắp xếp lại timeline theo thứ tự mới (drag-and-drop)
        /// </summary>
        /// <param name="request">Thông tin reorder với danh sách ID theo thứ tự mới</param>
        /// <param name="updatedById">ID của user thực hiện reorder</param>
        /// <returns>Timeline sau khi reorder</returns>
        Task<ResponseReorderTimelineDto> ReorderTimelineAsync(RequestReorderTimelineDto request, Guid updatedById);

        /// <summary>
        /// Lấy danh sách shops có sẵn cho dropdown selection
        /// </summary>
        /// <param name="includeInactive">Có bao gồm shops không active không</param>
        /// <param name="searchKeyword">Từ khóa tìm kiếm (tùy chọn)</param>
        /// <returns>Danh sách shops có sẵn</returns>
        Task<ResponseGetAvailableShopsDto> GetAvailableShopsAsync(bool includeInactive = false, string? searchKeyword = null);

        /// <summary>
        /// Validate timeline để kiểm tra tính hợp lệ
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <returns>Kết quả validation với danh sách lỗi (nếu có)</returns>
        Task<ResponseValidateTimelineDto> ValidateTimelineAsync(Guid tourTemplateId);

        /// <summary>
        /// Lấy thống kê về timeline của tour template
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <returns>Thống kê timeline</returns>
        Task<ResponseTimelineStatisticsDto> GetTimelineStatisticsAsync(Guid tourTemplateId);

        /// <summary>
        /// Kiểm tra xem có thể xóa tour detail không (business rules)
        /// </summary>
        /// <param name="tourDetailId">ID của tour detail</param>
        /// <returns>True nếu có thể xóa, false nếu không</returns>
        Task<bool> CanDeleteTimelineItemAsync(Guid tourDetailId);

        /// <summary>
        /// Duplicate một timeline item
        /// </summary>
        /// <param name="tourDetailId">ID của tour detail cần duplicate</param>
        /// <param name="createdById">ID của user tạo</param>
        /// <returns>Tour detail mới được tạo</returns>
        Task<ResponseCreateTourDetailDto> DuplicateTimelineItemAsync(Guid tourDetailId, Guid createdById);

        /// <summary>
        /// Lấy timeline item theo ID với đầy đủ thông tin
        /// </summary>
        /// <param name="tourDetailId">ID của tour detail</param>
        /// <returns>Thông tin chi tiết của timeline item</returns>
        Task<ResponseUpdateTourDetailDto> GetTimelineItemByIdAsync(Guid tourDetailId);
    }
}
