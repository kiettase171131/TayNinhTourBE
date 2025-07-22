using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBookingRefund;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBookingRefund;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho quản lý yêu cầu hoàn tiền tour booking
    /// Cung cấp các methods để tạo, xử lý và quản lý refund requests
    /// </summary>
    public interface ITourBookingRefundService
    {
        #region Customer Operations

        /// <summary>
        /// Tạo yêu cầu hoàn tiền mới từ customer (user cancellation)
        /// </summary>
        /// <param name="createDto">Thông tin yêu cầu hoàn tiền</param>
        /// <param name="customerId">ID của customer tạo yêu cầu</param>
        /// <returns>Thông tin yêu cầu hoàn tiền vừa tạo</returns>
        Task<ApiResponse<TourRefundRequestDto>> CreateRefundRequestAsync(
            CreateTourRefundRequestDto createDto, 
            Guid customerId);

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền của customer
        /// </summary>
        /// <param name="customerId">ID của customer</param>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Danh sách yêu cầu hoàn tiền với pagination</returns>
        Task<ApiResponse<PaginatedResponse<TourRefundRequestDto>>> GetCustomerRefundRequestsAsync(
            Guid customerId, 
            CustomerRefundFilterDto filter);

        /// <summary>
        /// Lấy thông tin chi tiết yêu cầu hoàn tiền của customer
        /// </summary>
        /// <param name="refundRequestId">ID của yêu cầu hoàn tiền</param>
        /// <param name="customerId">ID của customer</param>
        /// <returns>Thông tin chi tiết yêu cầu hoàn tiền</returns>
        Task<ApiResponse<TourRefundRequestDto>> GetCustomerRefundRequestByIdAsync(
            Guid refundRequestId, 
            Guid customerId);

        /// <summary>
        /// Hủy yêu cầu hoàn tiền (chỉ khi status = Pending)
        /// </summary>
        /// <param name="refundRequestId">ID của yêu cầu hoàn tiền</param>
        /// <param name="customerId">ID của customer</param>
        /// <param name="cancelDto">Thông tin hủy yêu cầu</param>
        /// <returns>Kết quả hủy yêu cầu</returns>
        Task<ApiResponse<bool>> CancelRefundRequestAsync(
            Guid refundRequestId, 
            Guid customerId, 
            CancelRefundRequestDto cancelDto);

        /// <summary>
        /// Cập nhật thông tin ngân hàng của yêu cầu hoàn tiền
        /// </summary>
        /// <param name="refundRequestId">ID của yêu cầu hoàn tiền</param>
        /// <param name="customerId">ID của customer</param>
        /// <param name="updateDto">Thông tin ngân hàng mới</param>
        /// <returns>Kết quả cập nhật</returns>
        Task<ApiResponse<bool>> UpdateRefundBankInfoAsync(
            Guid refundRequestId, 
            Guid customerId, 
            UpdateRefundBankInfoDto updateDto);

        #endregion

        #region Admin Operations

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền cho admin
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Danh sách yêu cầu hoàn tiền với pagination</returns>
        Task<ApiResponse<PaginatedResponse<AdminTourRefundDto>>> GetAdminRefundRequestsAsync(
            AdminRefundFilterDto filter);

        /// <summary>
        /// Lấy thông tin chi tiết yêu cầu hoàn tiền cho admin
        /// </summary>
        /// <param name="refundRequestId">ID của yêu cầu hoàn tiền</param>
        /// <returns>Thông tin chi tiết yêu cầu hoàn tiền</returns>
        Task<ApiResponse<AdminTourRefundDto>> GetAdminRefundRequestByIdAsync(Guid refundRequestId);

        /// <summary>
        /// Admin approve yêu cầu hoàn tiền
        /// </summary>
        /// <param name="refundRequestId">ID của yêu cầu hoàn tiền</param>
        /// <param name="adminId">ID của admin xử lý</param>
        /// <param name="approveDto">Thông tin approve</param>
        /// <returns>Kết quả approve</returns>
        Task<ApiResponse<bool>> ApproveRefundAsync(
            Guid refundRequestId, 
            Guid adminId, 
            ApproveRefundDto approveDto);

        /// <summary>
        /// Admin reject yêu cầu hoàn tiền
        /// </summary>
        /// <param name="refundRequestId">ID của yêu cầu hoàn tiền</param>
        /// <param name="adminId">ID của admin xử lý</param>
        /// <param name="rejectDto">Thông tin reject</param>
        /// <returns>Kết quả reject</returns>
        Task<ApiResponse<bool>> RejectRefundAsync(
            Guid refundRequestId, 
            Guid adminId, 
            RejectRefundDto rejectDto);

        /// <summary>
        /// Admin confirm đã chuyển tiền thủ công
        /// </summary>
        /// <param name="refundRequestId">ID của yêu cầu hoàn tiền</param>
        /// <param name="adminId">ID của admin xử lý</param>
        /// <param name="confirmDto">Thông tin confirm transfer</param>
        /// <returns>Kết quả confirm</returns>
        Task<ApiResponse<bool>> ConfirmTransferAsync(
            Guid refundRequestId, 
            Guid adminId, 
            ConfirmTransferDto confirmDto);

        /// <summary>
        /// Bulk approve/reject nhiều refund requests
        /// </summary>
        /// <param name="adminId">ID của admin xử lý</param>
        /// <param name="bulkActionDto">Thông tin bulk action</param>
        /// <returns>Kết quả bulk action</returns>
        Task<ApiResponse<BulkRefundActionResult>> BulkProcessRefundRequestsAsync(
            Guid adminId, 
            BulkRefundActionDto bulkActionDto);

        /// <summary>
        /// Điều chỉnh số tiền hoàn
        /// </summary>
        /// <param name="refundRequestId">ID của yêu cầu hoàn tiền</param>
        /// <param name="adminId">ID của admin xử lý</param>
        /// <param name="adjustDto">Thông tin điều chỉnh</param>
        /// <returns>Kết quả điều chỉnh</returns>
        Task<ApiResponse<bool>> AdjustRefundAmountAsync(
            Guid refundRequestId, 
            Guid adminId, 
            AdjustRefundAmountDto adjustDto);

        /// <summary>
        /// Reassign refund request cho admin khác
        /// </summary>
        /// <param name="refundRequestId">ID của yêu cầu hoàn tiền</param>
        /// <param name="currentAdminId">ID của admin hiện tại</param>
        /// <param name="reassignDto">Thông tin reassign</param>
        /// <returns>Kết quả reassign</returns>
        Task<ApiResponse<bool>> ReassignRefundRequestAsync(
            Guid refundRequestId, 
            Guid currentAdminId, 
            ReassignRefundDto reassignDto);

        #endregion

        #region System Operations

        /// <summary>
        /// Xử lý hoàn tiền cho company cancellation
        /// </summary>
        /// <param name="tourBookingIds">Danh sách ID của tour bookings bị hủy</param>
        /// <param name="cancellationReason">Lý do hủy từ company</param>
        /// <param name="processedById">ID của admin/system xử lý</param>
        /// <returns>Kết quả xử lý company cancellation</returns>
        Task<ApiResponse<CompanyCancellationResult>> ProcessCompanyCancellationAsync(
            List<Guid> tourBookingIds, 
            string cancellationReason, 
            Guid processedById);

        /// <summary>
        /// Xử lý hoàn tiền cho auto cancellation
        /// </summary>
        /// <param name="tourOperationIds">Danh sách ID của tour operations bị auto cancel</param>
        /// <param name="cancellationReason">Lý do auto cancel</param>
        /// <returns>Kết quả xử lý auto cancellation</returns>
        Task<ApiResponse<AutoCancellationResult>> ProcessAutoCancellationAsync(
            List<Guid> tourOperationIds, 
            string cancellationReason);

        /// <summary>
        /// Tính toán số tiền hoàn cho một booking
        /// </summary>
        /// <param name="tourBookingId">ID của tour booking</param>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="cancellationDate">Ngày hủy (mặc định là hiện tại)</param>
        /// <returns>Kết quả tính toán hoàn tiền</returns>
        Task<ApiResponse<RefundCalculationResult>> CalculateRefundAmountAsync(
            Guid tourBookingId, 
            TourRefundType refundType, 
            DateTime? cancellationDate = null);

        #endregion

        #region Statistics and Reporting

        /// <summary>
        /// Lấy dashboard thống kê refund cho admin
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Dashboard thống kê</returns>
        Task<ApiResponse<AdminRefundDashboardDto>> GetRefundDashboardAsync(RefundStatisticsFilterDto filter);

        /// <summary>
        /// Lấy thống kê refund theo tháng
        /// </summary>
        /// <param name="year">Năm</param>
        /// <param name="month">Tháng</param>
        /// <param name="refundType">Loại hoàn tiền (null = tất cả)</param>
        /// <returns>Thống kê theo tháng</returns>
        Task<ApiResponse<MonthlyRefundStats>> GetMonthlyRefundStatsAsync(
            int year, 
            int month, 
            TourRefundType? refundType = null);

        /// <summary>
        /// Export refund data
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>File data để download</returns>
        Task<ApiResponse<ExportFileResult>> ExportRefundDataAsync(ExportRefundFilterDto filter);

        #endregion

        #region Validation and Utilities

        /// <summary>
        /// Kiểm tra tour booking có đủ điều kiện hoàn tiền không
        /// </summary>
        /// <param name="tourBookingId">ID của tour booking</param>
        /// <param name="cancellationDate">Ngày dự kiến hủy</param>
        /// <returns>Kết quả kiểm tra eligibility</returns>
        Task<ApiResponse<RefundEligibilityResponseDto>> CheckRefundEligibilityAsync(
            Guid tourBookingId, 
            DateTime? cancellationDate = null);

        /// <summary>
        /// Lấy refund request theo tour booking ID
        /// </summary>
        /// <param name="tourBookingId">ID của tour booking</param>
        /// <returns>Refund request nếu có</returns>
        Task<ApiResponse<TourRefundRequestDto?>> GetRefundRequestByBookingIdAsync(Guid tourBookingId);

        /// <summary>
        /// Kiểm tra customer có refund request pending nào không
        /// </summary>
        /// <param name="customerId">ID của customer</param>
        /// <returns>True nếu có pending request</returns>
        Task<ApiResponse<bool>> HasPendingRefundRequestAsync(Guid customerId);

        #endregion
    }

    #region Result Classes

    /// <summary>
    /// Kết quả xử lý company cancellation
    /// </summary>
    public class CompanyCancellationResult
    {
        /// <summary>
        /// Số lượng bookings được xử lý
        /// </summary>
        public int ProcessedBookings { get; set; }

        /// <summary>
        /// Số lượng refund requests được tạo
        /// </summary>
        public int CreatedRefundRequests { get; set; }

        /// <summary>
        /// Tổng số tiền hoàn
        /// </summary>
        public decimal TotalRefundAmount { get; set; }

        /// <summary>
        /// Danh sách booking IDs thành công
        /// </summary>
        public List<Guid> SuccessfulBookingIds { get; set; } = new();

        /// <summary>
        /// Danh sách booking IDs thất bại
        /// </summary>
        public List<Guid> FailedBookingIds { get; set; } = new();

        /// <summary>
        /// Danh sách lỗi
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Kết quả xử lý auto cancellation
    /// </summary>
    public class AutoCancellationResult
    {
        /// <summary>
        /// Số lượng tour operations được xử lý
        /// </summary>
        public int ProcessedTourOperations { get; set; }

        /// <summary>
        /// Số lượng bookings bị ảnh hưởng
        /// </summary>
        public int AffectedBookings { get; set; }

        /// <summary>
        /// Số lượng refund requests được tạo
        /// </summary>
        public int CreatedRefundRequests { get; set; }

        /// <summary>
        /// Tổng số tiền hoàn
        /// </summary>
        public decimal TotalRefundAmount { get; set; }

        /// <summary>
        /// Danh sách tour operation IDs thành công
        /// </summary>
        public List<Guid> SuccessfulTourOperationIds { get; set; } = new();

        /// <summary>
        /// Danh sách lỗi
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Kết quả bulk refund action
    /// </summary>
    public class BulkRefundActionResult
    {
        /// <summary>
        /// Số lượng requests được xử lý thành công
        /// </summary>
        public int SuccessfulCount { get; set; }

        /// <summary>
        /// Số lượng requests thất bại
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// Danh sách refund request IDs thành công
        /// </summary>
        public List<Guid> SuccessfulRequestIds { get; set; } = new();

        /// <summary>
        /// Danh sách refund request IDs thất bại
        /// </summary>
        public List<Guid> FailedRequestIds { get; set; } = new();

        /// <summary>
        /// Danh sách lỗi
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Kết quả export file
    /// </summary>
    public class ExportFileResult
    {
        /// <summary>
        /// Tên file
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// Content type
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// File data
        /// </summary>
        public byte[] FileData { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Kích thước file (bytes)
        /// </summary>
        public long FileSize { get; set; }
    }

    #endregion
}
