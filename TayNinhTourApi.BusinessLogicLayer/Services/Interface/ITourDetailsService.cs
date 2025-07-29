using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho quản lý TourDetails và timeline chi tiết của tour template
    /// Cung cấp các operations để tạo, sửa, xóa TourDetails và quản lý timeline items
    /// </summary>
    public interface ITourDetailsService
    {
        // ===== TOURDETAILS CRUD OPERATIONS (NEW) =====

        /// <summary>
        /// Lấy danh sách TourDetails của một tour template
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="includeInactive">Có bao gồm TourDetails không active không</param>
        /// <returns>Danh sách TourDetails của template</returns>
        Task<ResponseGetTourDetailsDto> GetTourDetailsAsync(Guid tourTemplateId, bool includeInactive = false);

        /// <summary>
        /// Lấy chi tiết TourDetails theo ID
        /// </summary>
        /// <param name="tourDetailId">ID của TourDetails</param>
        /// <returns>Thông tin chi tiết TourDetails</returns>
        Task<ResponseGetTourDetailDto> GetTourDetailByIdAsync(Guid tourDetailId);

        /// <summary>
        /// Tạo TourDetails mới (trigger clone logic cho TourSlots)
        /// </summary>
        /// <param name="request">Thông tin TourDetails cần tạo</param>
        /// <param name="createdById">ID của user tạo</param>
        /// <returns>TourDetails vừa được tạo</returns>
        Task<ResponseCreateTourDetailDto> CreateTourDetailAsync(RequestCreateTourDetailDto request, Guid createdById);

        /// <summary>
        /// Cập nhật thông tin TourDetails
        /// </summary>
        /// <param name="tourDetailId">ID của TourDetails cần cập nhật</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <param name="updatedById">ID của user cập nhật</param>
        /// <returns>TourDetails sau khi cập nhật</returns>
        Task<ResponseUpdateTourDetailDto> UpdateTourDetailAsync(Guid tourDetailId, RequestUpdateTourDetailDto request, Guid updatedById);

        /// <summary>
        /// Xóa TourDetails và cleanup related data
        /// </summary>
        /// <param name="tourDetailId">ID của TourDetails cần xóa</param>
        /// <param name="deletedById">ID của user thực hiện xóa</param>
        /// <returns>Kết quả xóa</returns>
        Task<ResponseDeleteTourDetailDto> DeleteTourDetailAsync(Guid tourDetailId, Guid deletedById);

        /// <summary>
        /// Tìm kiếm TourDetails theo keyword
        /// </summary>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="tourTemplateId">ID template để lọc (optional)</param>
        /// <param name="includeInactive">Bao gồm inactive records</param>
        /// <returns>Kết quả tìm kiếm</returns>
        Task<ResponseSearchTourDetailsDto> SearchTourDetailsAsync(string keyword, Guid? tourTemplateId = null, bool includeInactive = false);

        /// <summary>
        /// Lấy TourDetails với phân trang
        /// </summary>
        /// <param name="pageIndex">Chỉ số trang (0-based)</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <param name="tourTemplateId">Filter theo template (optional)</param>
        /// <param name="titleFilter">Filter theo title (optional)</param>
        /// <param name="includeInactive">Bao gồm inactive records</param>
        /// <returns>Danh sách TourDetails có phân trang</returns>
        Task<ResponseGetTourDetailsPaginatedDto> GetTourDetailsPaginatedAsync(
            int pageIndex,
            int pageSize,
            Guid? tourTemplateId = null,
            string? titleFilter = null,
            bool includeInactive = false);

        // ===== TIMELINE OPERATIONS (EXISTING & NEW) =====

        /// <summary>
        /// Lấy timeline cho TourDetails cụ thể (NEW - theo design mới)
        /// </summary>
        /// <param name="request">Request chứa TourDetailsId và các options</param>
        /// <returns>Timeline của TourDetails với thông tin shop</returns>
        Task<ResponseGetTimelineDto> GetTimelineByTourDetailsAsync(RequestGetTimelineByTourDetailsDto request);

        /// <summary>
        /// Lấy full timeline cho tour template với sort order (DEPRECATED - backward compatibility)
        /// </summary>
        /// <param name="request">Request chứa TourTemplateId và các options</param>
        /// <returns>Timeline đầy đủ với thông tin shop</returns>
        [Obsolete("Use GetTimelineByTourDetailsAsync instead. This method will be removed in future versions.")]
        Task<ResponseGetTimelineDto> GetTimelineAsync(RequestGetTimelineDto request);

        /// <summary>
        /// Thêm mốc thời gian mới vào timeline
        /// </summary>
        /// <param name="request">Thông tin tour detail cần tạo</param>
        /// <param name="createdById">ID của user tạo</param>
        /// <returns>Tour detail vừa được tạo</returns>
        Task<ResponseCreateTourDetailDto> AddTimelineItemAsync(RequestCreateTourDetailDto request, Guid createdById);

        /// <summary>
        /// Tạo timeline item mới cho TourDetails
        /// </summary>
        /// <param name="request">Thông tin timeline item cần tạo</param>
        /// <param name="createdById">ID của user tạo</param>
        /// <returns>Timeline item vừa được tạo</returns>
        Task<BaseResposeDto> CreateTimelineItemAsync(RequestCreateTimelineItemDto request, Guid createdById);

        /// <summary>
        /// Tạo nhiều timeline items cho TourDetails (bulk create)
        /// </summary>
        /// <param name="request">Thông tin timeline items cần tạo</param>
        /// <param name="createdById">ID của user tạo</param>
        /// <returns>Danh sách timeline items vừa được tạo</returns>
        Task<ResponseCreateTimelineItemsDto> CreateTimelineItemsAsync(RequestCreateTimelineItemsDto request, Guid createdById);

        /// <summary>
        /// Cập nhật thông tin timeline item
        /// </summary>
        /// <param name="timelineItemId">ID của timeline item cần cập nhật</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <param name="updatedById">ID của user cập nhật</param>
        /// <returns>Timeline item sau khi cập nhật</returns>
        Task<ResponseUpdateTourDetailDto> UpdateTimelineItemAsync(Guid timelineItemId, RequestUpdateTimelineItemDto request, Guid updatedById);

        /// <summary>
        /// Xóa timeline item và tự động reorder các items còn lại
        /// </summary>
        /// <param name="timelineItemId">ID của timeline item cần xóa</param>
        /// <param name="deletedById">ID của user thực hiện xóa</param>
        /// <returns>Kết quả xóa</returns>
        Task<ResponseDeleteTourDetailDto> DeleteTimelineItemAsync(Guid timelineItemId, Guid deletedById);

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
        /// <param name="timelineItemId">ID của timeline item</param>
        /// <returns>True nếu có thể xóa, false nếu không</returns>
        Task<bool> CanDeleteTimelineItemAsync(Guid timelineItemId);

        /// <summary>
        /// Admin duyệt hoặc từ chối tour details
        /// </summary>
        /// <param name="tourDetailId">ID của tour detail cần duyệt/từ chối</param>
        /// <param name="request">Thông tin duyệt/từ chối</param>
        /// <param name="adminId">ID của admin thực hiện</param>
        /// <returns>Kết quả duyệt/từ chối</returns>
        Task<BaseResposeDto> ApproveRejectTourDetailAsync(Guid tourDetailId, RequestApprovalTourDetailDto request, Guid adminId);

        /// <summary>
        /// Lấy danh sách TourDetails với filter theo status và quyền user
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="currentUserId">ID của user hiện tại</param>
        /// <param name="userRole">Role của user hiện tại</param>
        /// <param name="includeInactive">Có bao gồm TourDetails không active không</param>
        /// <returns>Danh sách TourDetails được filter theo quyền</returns>
        Task<ResponseGetTourDetailsDto> GetTourDetailsWithPermissionAsync(Guid tourTemplateId, Guid currentUserId, string userRole, bool includeInactive = false);

        /// <summary>
        /// Duplicate một timeline item
        /// </summary>
        /// <param name="timelineItemId">ID của timeline item cần duplicate</param>
        /// <param name="createdById">ID của user tạo</param>
        /// <returns>Tour detail mới được tạo</returns>
        Task<ResponseCreateTourDetailDto> DuplicateTimelineItemAsync(Guid timelineItemId, Guid createdById);

        /// <summary>
        /// Lấy timeline item theo ID với đầy đủ thông tin
        /// </summary>
        /// <param name="timelineItemId">ID của timeline item</param>
        /// <returns>Thông tin chi tiết của timeline item</returns>
        Task<ResponseUpdateTourDetailDto> GetTimelineItemByIdAsync(Guid timelineItemId);

        // ===== TOUR GUIDE ASSIGNMENT WORKFLOW =====

        /// <summary>
        /// Lấy trạng thái phân công hướng dẫn viên cho TourDetails
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Thông tin trạng thái assignment</returns>
        Task<BaseResposeDto> GetGuideAssignmentStatusAsync(Guid tourDetailsId);

        /// <summary>
        /// TourCompany mời thủ công một TourGuide cụ thể
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="guideId">ID của TourGuide được mời</param>
        /// <param name="companyId">ID của TourCompany</param>
        /// <param name="invitationMessage">Tin nhắn từ TourCompany khi mời</param>
        /// <returns>Kết quả gửi lời mời</returns>
        Task<BaseResposeDto> ManualInviteGuideAsync(Guid tourDetailsId, Guid guideId, Guid companyId, string? invitationMessage = null);
    }
}
