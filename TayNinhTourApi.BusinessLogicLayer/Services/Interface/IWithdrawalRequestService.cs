using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.WithdrawalRequest;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.WithdrawalRequest;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho quản lý yêu cầu rút tiền
    /// Cung cấp các methods để tạo, xem và xử lý withdrawal requests
    /// </summary>
    public interface IWithdrawalRequestService
    {
        /// <summary>
        /// Tạo yêu cầu rút tiền mới
        /// </summary>
        /// <param name="createDto">Thông tin yêu cầu rút tiền</param>
        /// <param name="userId">ID của user tạo yêu cầu</param>
        /// <returns>Thông tin yêu cầu rút tiền vừa tạo</returns>
        Task<ApiResponse<WithdrawalRequestResponseDto>> CreateRequestAsync(CreateWithdrawalRequestDto createDto, Guid userId);

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền của user hiện tại
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách yêu cầu rút tiền với pagination</returns>
        Task<ApiResponse<PaginatedResponse<WithdrawalRequestResponseDto>>> GetByUserIdAsync(
            Guid userId, 
            WithdrawalStatus? status = null, 
            int pageNumber = 1, 
            int pageSize = 10);

        /// <summary>
        /// Lấy thông tin chi tiết một yêu cầu rút tiền
        /// </summary>
        /// <param name="withdrawalRequestId">ID của yêu cầu rút tiền</param>
        /// <param name="currentUserId">ID của user hiện tại</param>
        /// <returns>Thông tin chi tiết yêu cầu rút tiền</returns>
        Task<ApiResponse<WithdrawalRequestDetailDto>> GetByIdAsync(Guid withdrawalRequestId, Guid currentUserId);

        /// <summary>
        /// Hủy yêu cầu rút tiền (chỉ khi status = Pending)
        /// </summary>
        /// <param name="withdrawalRequestId">ID của yêu cầu rút tiền</param>
        /// <param name="currentUserId">ID của user hiện tại</param>
        /// <param name="reason">Lý do hủy</param>
        /// <returns>Kết quả hủy yêu cầu</returns>
        Task<ApiResponse<bool>> CancelRequestAsync(Guid withdrawalRequestId, Guid currentUserId, string? reason = null);

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền cho admin
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <returns>Danh sách yêu cầu rút tiền cho admin</returns>
        Task<ApiResponse<PaginatedResponse<WithdrawalRequestAdminDto>>> GetForAdminAsync(
            WithdrawalStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null);

        /// <summary>
        /// Admin duyệt yêu cầu rút tiền
        /// </summary>
        /// <param name="withdrawalRequestId">ID của yêu cầu rút tiền</param>
        /// <param name="adminId">ID của admin xử lý</param>
        /// <param name="adminNotes">Ghi chú từ admin</param>
        /// <param name="transactionReference">Mã tham chiếu giao dịch</param>
        /// <returns>Kết quả duyệt yêu cầu</returns>
        Task<ApiResponse<bool>> ApproveRequestAsync(
            Guid withdrawalRequestId, 
            Guid adminId, 
            string? adminNotes = null, 
            string? transactionReference = null);

        /// <summary>
        /// Admin từ chối yêu cầu rút tiền
        /// </summary>
        /// <param name="withdrawalRequestId">ID của yêu cầu rút tiền</param>
        /// <param name="adminId">ID của admin xử lý</param>
        /// <param name="reason">Lý do từ chối</param>
        /// <returns>Kết quả từ chối yêu cầu</returns>
        Task<ApiResponse<bool>> RejectRequestAsync(
            Guid withdrawalRequestId, 
            Guid adminId, 
            string reason);

        /// <summary>
        /// Lấy thống kê yêu cầu rút tiền
        /// </summary>
        /// <param name="userId">ID của user (null = tất cả users)</param>
        /// <returns>Thống kê yêu cầu rút tiền</returns>
        Task<ApiResponse<WithdrawalStatsDto>> GetStatsAsync(Guid? userId = null);

        /// <summary>
        /// Kiểm tra user có thể tạo yêu cầu rút tiền mới không
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>True nếu có thể tạo yêu cầu mới</returns>
        Task<ApiResponse<bool>> CanCreateNewRequestAsync(Guid userId);

        /// <summary>
        /// Lấy yêu cầu rút tiền gần nhất của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Yêu cầu rút tiền gần nhất</returns>
        Task<ApiResponse<WithdrawalRequestResponseDto?>> GetLatestRequestAsync(Guid userId);

        /// <summary>
        /// Validate yêu cầu rút tiền trước khi tạo
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="amount">Số tiền muốn rút</param>
        /// <param name="bankAccountId">ID của tài khoản ngân hàng</param>
        /// <returns>Kết quả validation</returns>
        Task<ApiResponse<bool>> ValidateWithdrawalRequestAsync(Guid userId, decimal amount, Guid bankAccountId);

        /// <summary>
        /// Lấy thống kê yêu cầu rút tiền theo role cho TourCompany và SpecialtyShop
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu lọc (null = tất cả)</param>
        /// <param name="endDate">Ngày kết thúc lọc (null = tất cả)</param>
        /// <returns>Thống kê yêu cầu rút tiền theo role</returns>
        Task<ApiResponse<WithdrawalRoleStatsSummaryDto>> GetRoleStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
}
