using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho SpecialtyShopApplication business logic
    /// Thay thế cho IShopApplicationService với methods mở rộng
    /// </summary>
    public interface ISpecialtyShopApplicationService
    {
        /// <summary>
        /// User nộp đơn đăng ký Specialty Shop
        /// </summary>
        /// <param name="dto">Thông tin đăng ký</param>
        /// <param name="currentUser">User hiện tại</param>
        /// <returns>Response với thông tin đơn đã nộp</returns>
        Task<SpecialtyShopApplicationSubmitResponseDto> SubmitApplicationAsync(
            SubmitSpecialtyShopApplicationDto dto,
            CurrentUserObject currentUser);

        /// <summary>
        /// User xem trạng thái đơn đăng ký của mình
        /// </summary>
        /// <param name="currentUser">User hiện tại</param>
        /// <returns>Thông tin đơn đăng ký hoặc null nếu chưa có</returns>
        Task<SpecialtyShopApplicationDto?> GetMyApplicationAsync(CurrentUserObject currentUser);

        /// <summary>
        /// Admin lấy danh sách đơn đăng ký với pagination
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại (0-based)</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="status">Filter theo status (optional)</param>
        /// <param name="searchTerm">Tìm kiếm (optional)</param>
        /// <returns>Danh sách đơn đăng ký với pagination info</returns>
        Task<(IEnumerable<SpecialtyShopApplicationSummaryDto> Applications, int TotalCount)> GetApplicationsAsync(
            int pageIndex = 0,
            int pageSize = 10,
            SpecialtyShopApplicationStatus? status = null,
            string? searchTerm = null);

        /// <summary>
        /// Admin xem chi tiết đơn đăng ký
        /// </summary>
        /// <param name="applicationId">ID của đơn đăng ký</param>
        /// <returns>Chi tiết đơn đăng ký hoặc null nếu không tìm thấy</returns>
        Task<SpecialtyShopApplicationDto?> GetApplicationByIdAsync(Guid applicationId);

        /// <summary>
        /// Admin duyệt đơn đăng ký
        /// Tự động: Update user role → "Specialty Shop", tạo SpecialtyShop record
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
            RejectSpecialtyShopApplicationDto dto,
            CurrentUserObject adminUser);

        /// <summary>
        /// Lấy danh sách đơn đăng ký pending (cho admin dashboard)
        /// </summary>
        /// <returns>Danh sách đơn pending</returns>
        Task<IEnumerable<SpecialtyShopApplicationSummaryDto>> GetPendingApplicationsAsync();

        /// <summary>
        /// Đếm số đơn đăng ký theo status (cho admin statistics)
        /// </summary>
        /// <param name="status">Status cần đếm</param>
        /// <returns>Số lượng đơn</returns>
        Task<int> CountApplicationsByStatusAsync(SpecialtyShopApplicationStatus status);

        /// <summary>
        /// Lấy thống kê đơn đăng ký theo khoảng thời gian
        /// </summary>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <returns>Danh sách đơn trong khoảng thời gian</returns>
        Task<IEnumerable<SpecialtyShopApplicationSummaryDto>> GetApplicationsByDateRangeAsync(
            DateTime fromDate,
            DateTime toDate);

        /// <summary>
        /// Kiểm tra user có thể nộp đơn mới không
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>True nếu có thể nộp đơn mới</returns>
        Task<bool> CanSubmitNewApplicationAsync(Guid userId);
    }
}
