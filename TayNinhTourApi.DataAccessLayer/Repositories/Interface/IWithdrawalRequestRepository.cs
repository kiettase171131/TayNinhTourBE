using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho WithdrawalRequest entity
    /// Cung cấp các methods để truy cập và thao tác với dữ liệu WithdrawalRequest
    /// </summary>
    public interface IWithdrawalRequestRepository : IGenericRepository<WithdrawalRequest>
    {
        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền của một user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách yêu cầu rút tiền</returns>
        Task<(IEnumerable<WithdrawalRequest> Items, int TotalCount)> GetByUserIdAsync(
            Guid userId, 
            WithdrawalStatus? status = null, 
            int pageNumber = 1, 
            int pageSize = 10);

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền cho admin
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm (tên user, bank name, etc.)</param>
        /// <returns>Danh sách yêu cầu rút tiền với thông tin user và bank</returns>
        Task<(IEnumerable<WithdrawalRequest> Items, int TotalCount)> GetForAdminAsync(
            WithdrawalStatus? status = null,
            int pageNumber = 1,
            int pageSize = 10,
            string? searchTerm = null);

        /// <summary>
        /// Lấy yêu cầu rút tiền theo ID với đầy đủ thông tin
        /// </summary>
        /// <param name="withdrawalRequestId">ID của yêu cầu rút tiền</param>
        /// <returns>Yêu cầu rút tiền với navigation properties</returns>
        Task<WithdrawalRequest?> GetWithDetailsAsync(Guid withdrawalRequestId);

        /// <summary>
        /// Lấy yêu cầu rút tiền theo ID với kiểm tra ownership
        /// </summary>
        /// <param name="withdrawalRequestId">ID của yêu cầu rút tiền</param>
        /// <param name="userId">ID của user (để kiểm tra ownership)</param>
        /// <returns>Yêu cầu rút tiền nếu user là owner, null nếu không</returns>
        Task<WithdrawalRequest?> GetByIdAndUserIdAsync(Guid withdrawalRequestId, Guid userId);

        /// <summary>
        /// Đếm số lượng yêu cầu rút tiền theo trạng thái
        /// </summary>
        /// <param name="status">Trạng thái cần đếm</param>
        /// <returns>Số lượng yêu cầu</returns>
        Task<int> CountByStatusAsync(WithdrawalStatus status);

        /// <summary>
        /// Lấy tổng số tiền đang chờ rút (status = Pending)
        /// </summary>
        /// <returns>Tổng số tiền pending</returns>
        Task<decimal> GetTotalPendingAmountAsync();

        /// <summary>
        /// Lấy tổng số tiền đã rút trong khoảng thời gian
        /// </summary>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <returns>Tổng số tiền đã rút</returns>
        Task<decimal> GetTotalWithdrawnAmountAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Kiểm tra user có yêu cầu rút tiền pending nào không
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>True nếu có yêu cầu pending</returns>
        Task<bool> HasPendingRequestAsync(Guid userId);

        /// <summary>
        /// Lấy yêu cầu rút tiền gần nhất của user
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <returns>Yêu cầu rút tiền gần nhất</returns>
        Task<WithdrawalRequest?> GetLatestByUserIdAsync(Guid userId);

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền theo bank account
        /// </summary>
        /// <param name="bankAccountId">ID của bank account</param>
        /// <returns>Danh sách yêu cầu rút tiền</returns>
        Task<IEnumerable<WithdrawalRequest>> GetByBankAccountIdAsync(Guid bankAccountId);

        /// <summary>
        /// Cập nhật trạng thái yêu cầu rút tiền
        /// </summary>
        /// <param name="withdrawalRequestId">ID của yêu cầu</param>
        /// <param name="status">Trạng thái mới</param>
        /// <param name="processedById">ID của admin xử lý</param>
        /// <param name="adminNotes">Ghi chú từ admin</param>
        /// <param name="transactionReference">Mã tham chiếu giao dịch</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> UpdateStatusAsync(
            Guid withdrawalRequestId,
            WithdrawalStatus status,
            Guid? processedById = null,
            string? adminNotes = null,
            string? transactionReference = null);

        /// <summary>
        /// Lấy thống kê rút tiền theo tháng
        /// </summary>
        /// <param name="year">Năm</param>
        /// <param name="month">Tháng</param>
        /// <returns>Thống kê rút tiền</returns>
        Task<(int TotalRequests, decimal TotalAmount, int ApprovedRequests, decimal ApprovedAmount)> GetMonthlyStatsAsync(int year, int month);

        /// <summary>
        /// Lấy thống kê yêu cầu rút tiền theo role (TourCompany hoặc SpecialtyShop)
        /// </summary>
        /// <param name="roleName">Tên role (TourCompany hoặc SpecialtyShop)</param>
        /// <param name="startDate">Ngày bắt đầu (null = tất cả)</param>
        /// <param name="endDate">Ngày kết thúc (null = tất cả)</param>
        /// <returns>Thống kê yêu cầu rút tiền theo role</returns>
        Task<(int TotalRequests, int PendingRequests, int ApprovedRequests, int RejectedRequests, 
               decimal TotalAmount, decimal PendingAmount, decimal ApprovedAmount, decimal RejectedAmount)> 
               GetStatsByRoleAsync(string roleName, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Lấy thống kê tổng hợp yêu cầu rút tiền với filtering theo ngày
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu (null = tất cả)</param>
        /// <param name="endDate">Ngày kết thúc (null = tất cả)</param>
        /// <returns>Thống kê tổng hợp yêu cầu rút tiền</returns>
        Task<(int TotalRequests, int PendingRequests, int ApprovedRequests, int RejectedRequests, int CancelledRequests,
               decimal TotalAmount, decimal PendingAmount, decimal ApprovedAmount, decimal RejectedAmount, decimal CancelledAmount,
               double AverageProcessingTimeHours, DateTime? LastRequestDate, DateTime? LastApprovalDate)> 
               GetTotalStatsAsync(DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Lấy tất cả yêu cầu rút tiền của user với filtering theo ngày
        /// </summary>
        /// <param name="userId">ID của user</param>
        /// <param name="startDate">Ngày bắt đầu (null = tất cả)</param>
        /// <param name="endDate">Ngày kết thúc (null = tất cả)</param>
        /// <returns>Danh sách yêu cầu rút tiền của user</returns>
        Task<IEnumerable<WithdrawalRequest>> GetUserRequestsWithFilterAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
