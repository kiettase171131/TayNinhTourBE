using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBookingRefund;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho quản lý chính sách hoàn tiền tour booking
    /// Cung cấp các methods để tính toán, validate và quản lý refund policies
    /// </summary>
    public interface IRefundPolicyService
    {
        /// <summary>
        /// Tìm policy phù hợp cho việc hoàn tiền
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="daysBeforeEvent">Số ngày trước tour</param>
        /// <param name="effectiveDate">Ngày hiệu lực (mặc định là hiện tại)</param>
        /// <returns>Policy phù hợp nhất</returns>
        Task<RefundPolicy?> GetApplicablePolicyAsync(
            TourRefundType refundType, 
            int daysBeforeEvent, 
            DateTime? effectiveDate = null);

        /// <summary>
        /// Tính toán số tiền hoàn dựa trên policy
        /// </summary>
        /// <param name="originalAmount">Số tiền gốc của booking</param>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="daysBeforeEvent">Số ngày trước tour</param>
        /// <param name="effectiveDate">Ngày hiệu lực</param>
        /// <returns>Thông tin tính toán hoàn tiền</returns>
        Task<RefundCalculationResult> CalculateRefundAmountAsync(
            decimal originalAmount,
            TourRefundType refundType,
            int daysBeforeEvent,
            DateTime? effectiveDate = null);

        /// <summary>
        /// Kiểm tra tour booking có đủ điều kiện hoàn tiền không
        /// </summary>
        /// <param name="tourBookingId">ID của tour booking</param>
        /// <param name="cancellationDate">Ngày dự kiến hủy (mặc định là hiện tại)</param>
        /// <returns>Kết quả kiểm tra eligibility</returns>
        Task<ApiResponse<RefundEligibilityResponseDto>> ValidateRefundEligibilityAsync(
            Guid tourBookingId,
            DateTime? cancellationDate = null);

        /// <summary>
        /// Lấy danh sách policies active theo loại hoàn tiền
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="effectiveDate">Ngày hiệu lực</param>
        /// <returns>Danh sách policies active</returns>
        Task<ApiResponse<List<RefundPolicy>>> GetActivePoliciesByTypeAsync(
            TourRefundType refundType,
            DateTime? effectiveDate = null);

        /// <summary>
        /// Tạo policy mới
        /// </summary>
        /// <param name="policy">Thông tin policy</param>
        /// <param name="createdById">ID của admin tạo</param>
        /// <returns>Policy vừa tạo</returns>
        Task<ApiResponse<RefundPolicy>> CreatePolicyAsync(RefundPolicy policy, Guid createdById);

        /// <summary>
        /// Cập nhật policy
        /// </summary>
        /// <param name="policyId">ID của policy</param>
        /// <param name="policy">Thông tin policy mới</param>
        /// <param name="updatedById">ID của admin cập nhật</param>
        /// <returns>Policy sau khi cập nhật</returns>
        Task<ApiResponse<RefundPolicy>> UpdatePolicyAsync(Guid policyId, RefundPolicy policy, Guid updatedById);

        /// <summary>
        /// Activate/Deactivate policy
        /// </summary>
        /// <param name="policyId">ID của policy</param>
        /// <param name="isActive">Trạng thái active mới</param>
        /// <param name="updatedById">ID của admin thực hiện</param>
        /// <returns>Kết quả cập nhật</returns>
        Task<ApiResponse<bool>> UpdatePolicyStatusAsync(Guid policyId, bool isActive, Guid updatedById);

        /// <summary>
        /// Xóa policy (soft delete)
        /// </summary>
        /// <param name="policyId">ID của policy</param>
        /// <param name="deletedById">ID của admin xóa</param>
        /// <returns>Kết quả xóa</returns>
        Task<ApiResponse<bool>> DeletePolicyAsync(Guid policyId, Guid deletedById);

        /// <summary>
        /// Lấy danh sách policies cho admin management
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Danh sách policies với pagination</returns>
        Task<ApiResponse<PaginatedResponse<RefundPolicy>>> GetPoliciesForAdminAsync(AdminRefundFilterDto filter);

        /// <summary>
        /// Validate policy business rules
        /// </summary>
        /// <param name="policy">Policy cần validate</param>
        /// <param name="excludePolicyId">ID policy cần loại trừ (cho update)</param>
        /// <returns>Danh sách lỗi validation</returns>
        Task<List<string>> ValidatePolicyAsync(RefundPolicy policy, Guid? excludePolicyId = null);

        /// <summary>
        /// Kiểm tra có policy nào conflict với range ngày không
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="minDaysBeforeEvent">Số ngày tối thiểu</param>
        /// <param name="maxDaysBeforeEvent">Số ngày tối đa</param>
        /// <param name="excludePolicyId">ID policy cần loại trừ</param>
        /// <returns>True nếu có conflict</returns>
        Task<bool> HasConflictingPolicyAsync(
            TourRefundType refundType,
            int minDaysBeforeEvent,
            int? maxDaysBeforeEvent,
            Guid? excludePolicyId = null);

        /// <summary>
        /// Lấy next available priority cho loại hoàn tiền
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <returns>Priority tiếp theo có thể sử dụng</returns>
        Task<int> GetNextAvailablePriorityAsync(TourRefundType refundType);

        /// <summary>
        /// Tạo default policies cho hệ thống
        /// </summary>
        /// <param name="createdById">ID của admin tạo</param>
        /// <returns>Kết quả tạo default policies</returns>
        Task<ApiResponse<bool>> CreateDefaultPoliciesAsync(Guid createdById);

        /// <summary>
        /// Lấy policy statistics
        /// </summary>
        /// <returns>Thống kê về policies</returns>
        Task<ApiResponse<PolicyStatistics>> GetPolicyStatisticsAsync();

        /// <summary>
        /// Clone policy với modifications
        /// </summary>
        /// <param name="sourcePolicyId">ID của policy gốc</param>
        /// <param name="modifications">Các thay đổi cần áp dụng</param>
        /// <param name="createdById">ID của admin tạo</param>
        /// <returns>Policy mới được tạo</returns>
        Task<ApiResponse<RefundPolicy>> ClonePolicyAsync(
            Guid sourcePolicyId,
            Action<RefundPolicy> modifications,
            Guid createdById);

        /// <summary>
        /// Bulk update effective dates cho policies
        /// </summary>
        /// <param name="policyIds">Danh sách ID policies</param>
        /// <param name="effectiveFrom">Ngày bắt đầu hiệu lực mới</param>
        /// <param name="effectiveTo">Ngày kết thúc hiệu lực mới</param>
        /// <param name="updatedById">ID của admin thực hiện</param>
        /// <returns>Kết quả cập nhật</returns>
        Task<ApiResponse<bool>> BulkUpdateEffectiveDatesAsync(
            List<Guid> policyIds,
            DateTime? effectiveFrom,
            DateTime? effectiveTo,
            Guid updatedById);

        /// <summary>
        /// Lấy policies sắp hết hạn
        /// </summary>
        /// <param name="daysBeforeExpiry">Số ngày trước khi hết hạn</param>
        /// <returns>Danh sách policies sắp hết hạn</returns>
        Task<ApiResponse<List<RefundPolicy>>> GetExpiringPoliciesAsync(int daysBeforeExpiry = 30);

        /// <summary>
        /// Lấy policy history cho audit
        /// </summary>
        /// <param name="filter">Filter criteria</param>
        /// <returns>Lịch sử thay đổi policies</returns>
        Task<ApiResponse<PaginatedResponse<RefundPolicy>>> GetPolicyHistoryAsync(RefundStatisticsFilterDto filter);

        /// <summary>
        /// Preview refund calculation cho customer
        /// </summary>
        /// <param name="tourBookingId">ID của tour booking</param>
        /// <param name="cancellationDate">Ngày dự kiến hủy</param>
        /// <returns>Preview calculation kết quả</returns>
        Task<ApiResponse<RefundPreviewDto>> PreviewRefundCalculationAsync(
            Guid tourBookingId,
            DateTime? cancellationDate = null);

        /// <summary>
        /// Lấy refund policy text để hiển thị cho customer
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <returns>Policy text formatted</returns>
        Task<ApiResponse<string>> GetRefundPolicyTextAsync(TourRefundType refundType);
    }

    /// <summary>
    /// Kết quả tính toán hoàn tiền
    /// </summary>
    public class RefundCalculationResult
    {
        /// <summary>
        /// Policy được áp dụng
        /// </summary>
        public RefundPolicy? AppliedPolicy { get; set; }

        /// <summary>
        /// Số tiền gốc
        /// </summary>
        public decimal OriginalAmount { get; set; }

        /// <summary>
        /// Phần trăm hoàn tiền
        /// </summary>
        public decimal RefundPercentage { get; set; }

        /// <summary>
        /// Số tiền hoàn trước khi trừ phí
        /// </summary>
        public decimal RefundAmountBeforeFee { get; set; }

        /// <summary>
        /// Phí xử lý cố định
        /// </summary>
        public decimal ProcessingFee { get; set; }

        /// <summary>
        /// Phí xử lý theo phần trăm
        /// </summary>
        public decimal ProcessingFeePercentage { get; set; }

        /// <summary>
        /// Tổng phí xử lý
        /// </summary>
        public decimal TotalProcessingFee { get; set; }

        /// <summary>
        /// Số tiền thực tế customer nhận
        /// </summary>
        public decimal NetRefundAmount { get; set; }

        /// <summary>
        /// Có đủ điều kiện hoàn tiền không
        /// </summary>
        public bool IsEligible { get; set; }

        /// <summary>
        /// Lý do không đủ điều kiện (nếu có)
        /// </summary>
        public string? IneligibilityReason { get; set; }

        /// <summary>
        /// Số ngày trước tour
        /// </summary>
        public int DaysBeforeTour { get; set; }
    }

    /// <summary>
    /// Thống kê policies
    /// </summary>
    public class PolicyStatistics
    {
        /// <summary>
        /// Tổng số policies
        /// </summary>
        public int TotalPolicies { get; set; }

        /// <summary>
        /// Số policies active
        /// </summary>
        public int ActivePolicies { get; set; }

        /// <summary>
        /// Số policies đã hết hạn
        /// </summary>
        public int ExpiredPolicies { get; set; }

        /// <summary>
        /// Số policies sắp hết hạn
        /// </summary>
        public int ExpiringPolicies { get; set; }

        /// <summary>
        /// Breakdown theo refund type
        /// </summary>
        public Dictionary<TourRefundType, int> PoliciesByType { get; set; } = new();
    }

    /// <summary>
    /// Preview refund calculation
    /// </summary>
    public class RefundPreviewDto
    {
        /// <summary>
        /// Có đủ điều kiện hoàn tiền không
        /// </summary>
        public bool IsEligible { get; set; }

        /// <summary>
        /// Lý do không đủ điều kiện
        /// </summary>
        public string? IneligibilityReason { get; set; }

        /// <summary>
        /// Kết quả tính toán
        /// </summary>
        public RefundCalculationResult? Calculation { get; set; }

        /// <summary>
        /// Thông tin policy áp dụng
        /// </summary>
        public string? PolicyDescription { get; set; }

        /// <summary>
        /// Cảnh báo cho customer
        /// </summary>
        public List<string> Warnings { get; set; } = new();

        /// <summary>
        /// Thông tin bổ sung
        /// </summary>
        public List<string> AdditionalInfo { get; set; } = new();

        /// <summary>
        /// Deadline để có policy tốt hơn
        /// </summary>
        public DateTime? NextPolicyDeadline { get; set; }

        /// <summary>
        /// Refund percentage của policy tốt hơn
        /// </summary>
        public decimal? NextPolicyRefundPercentage { get; set; }
    }
}
