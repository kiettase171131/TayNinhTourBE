using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBookingRefund
{
    /// <summary>
    /// DTO cho việc admin approve yêu cầu hoàn tiền
    /// </summary>
    public class ApproveRefundDto
    {
        /// <summary>
        /// Số tiền được duyệt hoàn (có thể khác với số tiền yêu cầu)
        /// </summary>
        [Required(ErrorMessage = "Số tiền duyệt là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền duyệt phải >= 0")]
        public decimal ApprovedAmount { get; set; }

        /// <summary>
        /// Ghi chú từ admin khi approve
        /// </summary>
        [StringLength(1000, ErrorMessage = "Ghi chú admin không được vượt quá 1000 ký tự")]
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Mã tham chiếu giao dịch chuyển tiền (nếu có)
        /// </summary>
        [StringLength(100, ErrorMessage = "Mã giao dịch không được vượt quá 100 ký tự")]
        public string? TransactionReference { get; set; }
    }

    /// <summary>
    /// DTO cho việc admin reject yêu cầu hoàn tiền
    /// </summary>
    public class RejectRefundDto
    {
        /// <summary>
        /// Lý do từ chối hoàn tiền
        /// </summary>
        [Required(ErrorMessage = "Lý do từ chối là bắt buộc")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Lý do từ chối phải từ 10-1000 ký tự")]
        public string RejectionReason { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú thêm từ admin
        /// </summary>
        [StringLength(1000, ErrorMessage = "Ghi chú admin không được vượt quá 1000 ký tự")]
        public string? AdminNotes { get; set; }
    }

    /// <summary>
    /// DTO cho việc admin confirm đã chuyển tiền thủ công
    /// </summary>
    public class ConfirmTransferDto
    {
        /// <summary>
        /// Mã tham chiếu giao dịch chuyển tiền
        /// </summary>
        [Required(ErrorMessage = "Mã giao dịch là bắt buộc")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Mã giao dịch phải từ 3-100 ký tự")]
        public string TransactionReference { get; set; } = string.Empty;

        /// <summary>
        /// Số tiền thực tế đã chuyển
        /// </summary>
        [Required(ErrorMessage = "Số tiền chuyển là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền chuyển phải >= 0")]
        public decimal TransferredAmount { get; set; }

        /// <summary>
        /// Ngày giờ chuyển tiền thực tế
        /// </summary>
        [Required(ErrorMessage = "Thời gian chuyển tiền là bắt buộc")]
        public DateTime TransferredAt { get; set; }

        /// <summary>
        /// Ghi chú về việc chuyển tiền
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? TransferNotes { get; set; }
    }

    /// <summary>
    /// DTO cho việc admin bulk approve/reject nhiều refund requests
    /// </summary>
    public class BulkRefundActionDto
    {
        /// <summary>
        /// Danh sách ID của refund requests cần xử lý
        /// </summary>
        [Required(ErrorMessage = "Danh sách refund request là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 refund request")]
        public List<Guid> RefundRequestIds { get; set; } = new();

        /// <summary>
        /// Action cần thực hiện (Approve/Reject)
        /// </summary>
        [Required(ErrorMessage = "Action là bắt buộc")]
        public BulkRefundAction Action { get; set; }

        /// <summary>
        /// Ghi chú chung cho tất cả requests
        /// </summary>
        [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Lý do từ chối (bắt buộc nếu Action = Reject)
        /// </summary>
        [StringLength(1000, ErrorMessage = "Lý do từ chối không được vượt quá 1000 ký tự")]
        public string? RejectionReason { get; set; }
    }

    /// <summary>
    /// Enum cho bulk refund actions
    /// </summary>
    public enum BulkRefundAction
    {
        Approve = 1,
        Reject = 2
    }

    /// <summary>
    /// DTO cho việc admin điều chỉnh số tiền hoàn
    /// </summary>
    public class AdjustRefundAmountDto
    {
        /// <summary>
        /// Số tiền hoàn mới
        /// </summary>
        [Required(ErrorMessage = "Số tiền hoàn mới là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền hoàn phải >= 0")]
        public decimal NewRefundAmount { get; set; }

        /// <summary>
        /// Lý do điều chỉnh
        /// </summary>
        [Required(ErrorMessage = "Lý do điều chỉnh là bắt buộc")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Lý do điều chỉnh phải từ 10-500 ký tự")]
        public string AdjustmentReason { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú thêm từ admin
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? AdminNotes { get; set; }
    }

    /// <summary>
    /// DTO cho việc admin reassign refund request cho admin khác
    /// </summary>
    public class ReassignRefundDto
    {
        /// <summary>
        /// ID của admin mới sẽ xử lý
        /// </summary>
        [Required(ErrorMessage = "Admin mới là bắt buộc")]
        public Guid NewAssigneeId { get; set; }

        /// <summary>
        /// Lý do reassign
        /// </summary>
        [Required(ErrorMessage = "Lý do reassign là bắt buộc")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Lý do reassign phải từ 5-500 ký tự")]
        public string ReassignReason { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú thêm
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }
}
