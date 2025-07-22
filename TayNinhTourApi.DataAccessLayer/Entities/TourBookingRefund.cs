using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Yêu cầu hoàn tiền cho tour booking
    /// Workflow: Customer/System tạo yêu cầu -> Admin xử lý -> Approve/Reject -> Complete
    /// </summary>
    public class TourBookingRefund : BaseEntity
    {
        /// <summary>
        /// Foreign Key đến TourBooking cần hoàn tiền
        /// </summary>
        [Required]
        public Guid TourBookingId { get; set; }

        /// <summary>
        /// Foreign Key đến User yêu cầu hoàn tiền (customer)
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Loại hoàn tiền (Company/Auto/User cancellation)
        /// </summary>
        [Required]
        public TourRefundType RefundType { get; set; }

        /// <summary>
        /// Lý do hoàn tiền
        /// </summary>
        [Required]
        [StringLength(1000, ErrorMessage = "Lý do hoàn tiền không quá 1000 ký tự")]
        public string RefundReason { get; set; } = string.Empty;

        /// <summary>
        /// Số tiền gốc của booking (để tham khảo)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalAmount { get; set; }

        /// <summary>
        /// Số tiền yêu cầu hoàn (có thể khác original do policy)
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền hoàn phải >= 0")]
        public decimal RequestedAmount { get; set; }

        /// <summary>
        /// Số tiền thực tế được duyệt hoàn (admin có thể điều chỉnh)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền duyệt phải >= 0")]
        public decimal? ApprovedAmount { get; set; }

        /// <summary>
        /// Phí xử lý hoàn tiền (nếu có)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Phí xử lý phải >= 0")]
        public decimal ProcessingFee { get; set; } = 0;

        /// <summary>
        /// Số tiền thực tế customer nhận được (ApprovedAmount - ProcessingFee)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetRefundAmount => (ApprovedAmount ?? 0) - ProcessingFee;

        /// <summary>
        /// Trạng thái của yêu cầu hoàn tiền
        /// </summary>
        [Required]
        public TourRefundStatus Status { get; set; } = TourRefundStatus.Pending;

        /// <summary>
        /// Thời gian tạo yêu cầu hoàn tiền
        /// </summary>
        [Required]
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian admin xử lý yêu cầu (approve/reject)
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// ID của admin xử lý yêu cầu
        /// </summary>
        public Guid? ProcessedById { get; set; }

        /// <summary>
        /// Thời gian hoàn thành việc chuyển tiền
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Ghi chú từ admin khi xử lý yêu cầu
        /// </summary>
        [StringLength(1000, ErrorMessage = "Ghi chú admin không quá 1000 ký tự")]
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Ghi chú từ customer khi tạo yêu cầu
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú customer không quá 500 ký tự")]
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Mã tham chiếu giao dịch chuyển tiền (nếu có)
        /// </summary>
        [StringLength(100, ErrorMessage = "Mã giao dịch không quá 100 ký tự")]
        public string? TransactionReference { get; set; }

        // Customer Bank Information for Manual Transfer
        /// <summary>
        /// Tên ngân hàng nhận tiền hoàn
        /// </summary>
        [StringLength(100, ErrorMessage = "Tên ngân hàng không quá 100 ký tự")]
        public string? CustomerBankName { get; set; }

        /// <summary>
        /// Số tài khoản nhận tiền hoàn
        /// </summary>
        [StringLength(50, ErrorMessage = "Số tài khoản không quá 50 ký tự")]
        public string? CustomerAccountNumber { get; set; }

        /// <summary>
        /// Tên chủ tài khoản nhận tiền hoàn
        /// </summary>
        [StringLength(100, ErrorMessage = "Tên chủ tài khoản không quá 100 ký tự")]
        public string? CustomerAccountHolder { get; set; }

        /// <summary>
        /// Số ngày trước tour khi tạo yêu cầu (để tính policy)
        /// </summary>
        public int? DaysBeforeTour { get; set; }

        /// <summary>
        /// Phần trăm hoàn tiền theo policy (0-100)
        /// </summary>
        [Range(0, 100, ErrorMessage = "Phần trăm hoàn tiền phải từ 0-100")]
        public decimal? RefundPercentage { get; set; }

        // Navigation Properties

        /// <summary>
        /// Tour booking được hoàn tiền
        /// </summary>
        public virtual TourBooking TourBooking { get; set; } = null!;

        /// <summary>
        /// Customer yêu cầu hoàn tiền
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Admin xử lý yêu cầu (nếu có)
        /// </summary>
        public virtual User? ProcessedBy { get; set; }
    }
}
