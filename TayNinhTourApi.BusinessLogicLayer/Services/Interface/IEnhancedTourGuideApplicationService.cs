using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Enhanced service interface cho TourGuideApplication business logic
    /// Clean implementation thay thế cho ITourGuideApplicationService cũ
    /// </summary>
    public interface IEnhancedTourGuideApplicationService
    {
        /// <summary>
        /// User nộp đơn đăng ký TourGuide (Enhanced version)
        /// </summary>
        /// <param name="dto">Thông tin đăng ký</param>
        /// <param name="currentUser">User hiện tại</param>
        /// <returns>Response với thông tin đơn đã nộp</returns>
        Task<TourGuideApplicationSubmitResponseDto> SubmitApplicationAsync(
            SubmitTourGuideApplicationDto dto,
            CurrentUserObject currentUser);

        /// <summary>
        /// User xem danh sách đơn đăng ký của mình
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Danh sách đơn đăng ký</returns>
        Task<IEnumerable<TourGuideApplicationSummaryDto>> GetMyApplicationsAsync(Guid userId);

        /// <summary>
        /// User xem chi tiết đơn đăng ký của mình
        /// </summary>
        /// <param name="applicationId">ID của đơn đăng ký</param>
        /// <param name="userId">ID của user (để check ownership)</param>
        /// <returns>Chi tiết đơn đăng ký hoặc null nếu không tìm thấy</returns>
        Task<TourGuideApplicationDto?> GetMyApplicationByIdAsync(Guid applicationId, Guid userId);

        /// <summary>
        /// Admin xem danh sách tất cả đơn đăng ký với pagination
        /// </summary>
        /// <param name="page">Trang hiện tại</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="status">Filter theo status (optional)</param>
        /// <returns>Danh sách đơn đăng ký với pagination</returns>
        Task<(IEnumerable<TourGuideApplicationSummaryDto> Applications, int TotalCount)> GetAllApplicationsAsync(
            int page = 1, 
            int pageSize = 10, 
            int? status = null);

        /// <summary>
        /// Admin xem chi tiết đơn đăng ký
        /// </summary>
        /// <param name="applicationId">ID của đơn đăng ký</param>
        /// <returns>Chi tiết đơn đăng ký hoặc null nếu không tìm thấy</returns>
        Task<TourGuideApplicationDto?> GetApplicationByIdAsync(Guid applicationId);

        /// <summary>
        /// Admin duyệt đơn đăng ký
        /// Tự động: Update user role → "TourGuide"
        /// </summary>
        /// <param name="applicationId">ID của đơn đăng ký</param>
        /// <param name="adminUser">Admin thực hiện duyệt</param>
        /// <returns>Kết quả duyệt</returns>
        Task<BaseResposeDto> ApproveApplicationAsync(Guid applicationId, CurrentUserObject adminUser);

        /// <summary>
        /// Admin từ chối đơn đăng ký
        /// </summary>
        /// <param name="applicationId">ID của đơn đăng ký</param>
        /// <param name="dto">Lý do từ chối</param>
        /// <param name="adminUser">Admin thực hiện từ chối</param>
        /// <returns>Kết quả từ chối</returns>
        Task<BaseResposeDto> RejectApplicationAsync(
            Guid applicationId, 
            RejectTourGuideApplicationDto dto, 
            CurrentUserObject adminUser);

        /// <summary>
        /// Kiểm tra user có thể nộp đơn mới không
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>True nếu có thể nộp đơn mới</returns>
        Task<bool> CanSubmitNewApplicationAsync(Guid userId);
    }
}
