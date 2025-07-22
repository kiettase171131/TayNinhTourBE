using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho RefundPolicy entity
    /// Cung cấp các methods để truy cập và thao tác với dữ liệu RefundPolicy
    /// </summary>
    public interface IRefundPolicyRepository : IGenericRepository<RefundPolicy>
    {
        /// <summary>
        /// Lấy danh sách policies active theo loại hoàn tiền
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="effectiveDate">Ngày hiệu lực (mặc định là hiện tại)</param>
        /// <returns>Danh sách policies active được sắp xếp theo priority</returns>
        Task<IEnumerable<RefundPolicy>> GetActivePoliciesByTypeAsync(
            TourRefundType refundType, 
            DateTime? effectiveDate = null);

        /// <summary>
        /// Tìm policy phù hợp cho số ngày trước tour
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="daysBeforeEvent">Số ngày trước tour</param>
        /// <param name="effectiveDate">Ngày hiệu lực (mặc định là hiện tại)</param>
        /// <returns>Policy phù hợp nhất (priority cao nhất)</returns>
        Task<RefundPolicy?> GetApplicablePolicyAsync(
            TourRefundType refundType, 
            int daysBeforeEvent, 
            DateTime? effectiveDate = null);

        /// <summary>
        /// Lấy tất cả policies cho admin management
        /// </summary>
        /// <param name="refundType">Lọc theo loại hoàn tiền (null = tất cả)</param>
        /// <param name="isActive">Lọc theo trạng thái active (null = tất cả)</param>
        /// <param name="pageNumber">Số trang</param>
        /// <param name="pageSize">Kích thước trang</param>
        /// <returns>Danh sách policies với pagination</returns>
        Task<(IEnumerable<RefundPolicy> Items, int TotalCount)> GetForAdminAsync(
            TourRefundType? refundType = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10);

        /// <summary>
        /// Kiểm tra có policy nào conflict với range ngày không
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="minDaysBeforeEvent">Số ngày tối thiểu</param>
        /// <param name="maxDaysBeforeEvent">Số ngày tối đa (null = không giới hạn)</param>
        /// <param name="excludePolicyId">ID policy cần loại trừ (cho update)</param>
        /// <returns>True nếu có conflict</returns>
        Task<bool> HasConflictingPolicyAsync(
            TourRefundType refundType,
            int minDaysBeforeEvent,
            int? maxDaysBeforeEvent,
            Guid? excludePolicyId = null);

        /// <summary>
        /// Lấy policies đang overlap với range ngày
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="minDaysBeforeEvent">Số ngày tối thiểu</param>
        /// <param name="maxDaysBeforeEvent">Số ngày tối đa (null = không giới hạn)</param>
        /// <param name="excludePolicyId">ID policy cần loại trừ</param>
        /// <returns>Danh sách policies bị overlap</returns>
        Task<IEnumerable<RefundPolicy>> GetOverlappingPoliciesAsync(
            TourRefundType refundType,
            int minDaysBeforeEvent,
            int? maxDaysBeforeEvent,
            Guid? excludePolicyId = null);

        /// <summary>
        /// Activate/Deactivate policy
        /// </summary>
        /// <param name="policyId">ID của policy</param>
        /// <param name="isActive">Trạng thái active mới</param>
        /// <param name="updatedById">ID của admin thực hiện</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> UpdateActiveStatusAsync(Guid policyId, bool isActive, Guid updatedById);

        /// <summary>
        /// Lấy policy theo ID với validation
        /// </summary>
        /// <param name="policyId">ID của policy</param>
        /// <param name="includeInactive">Có bao gồm policy inactive không</param>
        /// <returns>Policy nếu tìm thấy và hợp lệ</returns>
        Task<RefundPolicy?> GetValidPolicyAsync(Guid policyId, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách policies theo priority range
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="minPriority">Priority tối thiểu</param>
        /// <param name="maxPriority">Priority tối đa</param>
        /// <returns>Danh sách policies trong range priority</returns>
        Task<IEnumerable<RefundPolicy>> GetByPriorityRangeAsync(
            TourRefundType refundType,
            int minPriority,
            int maxPriority);

        /// <summary>
        /// Lấy next available priority cho loại hoàn tiền
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <returns>Priority tiếp theo có thể sử dụng</returns>
        Task<int> GetNextAvailablePriorityAsync(TourRefundType refundType);

        /// <summary>
        /// Kiểm tra priority đã được sử dụng chưa
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="priority">Priority cần kiểm tra</param>
        /// <param name="excludePolicyId">ID policy cần loại trừ</param>
        /// <returns>True nếu priority đã được sử dụng</returns>
        Task<bool> IsPriorityUsedAsync(TourRefundType refundType, int priority, Guid? excludePolicyId = null);

        /// <summary>
        /// Lấy policies sắp hết hạn
        /// </summary>
        /// <param name="daysBeforeExpiry">Số ngày trước khi hết hạn</param>
        /// <returns>Danh sách policies sắp hết hạn</returns>
        Task<IEnumerable<RefundPolicy>> GetExpiringPoliciesAsync(int daysBeforeExpiry = 30);

        /// <summary>
        /// Lấy policies đã hết hạn
        /// </summary>
        /// <param name="asOfDate">Ngày kiểm tra (mặc định là hiện tại)</param>
        /// <returns>Danh sách policies đã hết hạn</returns>
        Task<IEnumerable<RefundPolicy>> GetExpiredPoliciesAsync(DateTime? asOfDate = null);

        /// <summary>
        /// Bulk update effective dates cho policies
        /// </summary>
        /// <param name="policyIds">Danh sách ID policies</param>
        /// <param name="effectiveFrom">Ngày bắt đầu hiệu lực mới</param>
        /// <param name="effectiveTo">Ngày kết thúc hiệu lực mới</param>
        /// <param name="updatedById">ID của admin thực hiện</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> BulkUpdateEffectiveDatesAsync(
            IEnumerable<Guid> policyIds,
            DateTime? effectiveFrom,
            DateTime? effectiveTo,
            Guid updatedById);

        /// <summary>
        /// Lấy policy history cho audit
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <returns>Lịch sử thay đổi policies</returns>
        Task<IEnumerable<RefundPolicy>> GetPolicyHistoryAsync(
            TourRefundType? refundType = null,
            DateTime? fromDate = null,
            DateTime? toDate = null);

        /// <summary>
        /// Validate policy business rules
        /// </summary>
        /// <param name="policy">Policy cần validate</param>
        /// <returns>Danh sách lỗi validation (empty nếu hợp lệ)</returns>
        Task<IEnumerable<string>> ValidatePolicyAsync(RefundPolicy policy);

        /// <summary>
        /// Lấy default policies cho từng loại hoàn tiền
        /// </summary>
        /// <returns>Dictionary mapping RefundType -> Default Policy</returns>
        Task<Dictionary<TourRefundType, RefundPolicy?>> GetDefaultPoliciesAsync();

        /// <summary>
        /// Tạo default policies cho hệ thống
        /// </summary>
        /// <param name="createdById">ID của admin tạo</param>
        /// <returns>True nếu tạo thành công</returns>
        Task<bool> CreateDefaultPoliciesAsync(Guid createdById);

        /// <summary>
        /// Lấy policy statistics
        /// </summary>
        /// <returns>Thống kê về policies</returns>
        Task<(int TotalPolicies, int ActivePolicies, int ExpiredPolicies, int ExpiringPolicies)> GetPolicyStatisticsAsync();

        /// <summary>
        /// Clone policy với modifications
        /// </summary>
        /// <param name="sourcePolicyId">ID của policy gốc</param>
        /// <param name="modifications">Các thay đổi cần áp dụng</param>
        /// <param name="createdById">ID của admin tạo</param>
        /// <returns>Policy mới được tạo</returns>
        Task<RefundPolicy?> ClonePolicyAsync(
            Guid sourcePolicyId,
            Action<RefundPolicy> modifications,
            Guid createdById);
    }
}
