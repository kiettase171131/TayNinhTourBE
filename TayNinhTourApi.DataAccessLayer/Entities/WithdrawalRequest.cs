using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Yêu cầu rút tiền từ ví của user
    /// Workflow: User tạo yêu cầu -> Admin xử lý -> Approve/Reject
    /// </summary>
    public class WithdrawalRequest : BaseEntity
    {
        /// <summary>
        /// Foreign Key đến User tạo yêu cầu rút tiền
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Foreign Key đến BankAccount được chọn để nhận tiền
        /// </summary>
        [Required]
        public Guid BankAccountId { get; set; }

        /// <summary>
        /// Số tiền yêu cầu rút (VNĐ)
        /// Phải > 0 và <= số dư hiện tại trong ví
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(1000, double.MaxValue, ErrorMessage = "Số tiền rút tối thiểu là 1,000 VNĐ")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Trạng thái của yêu cầu rút tiền
        /// </summary>
        [Required]
        public WithdrawalStatus Status { get; set; } = WithdrawalStatus.Pending;

        /// <summary>
        /// Thời gian tạo yêu cầu
        /// Tự động set khi tạo record
        /// </summary>
        [Required]
        public DateTime RequestedAt { get; set; } = VietnamTimeZoneUtility.GetVietnamNow();

        /// <summary>
        /// Thời gian admin xử lý yêu cầu (approve/reject)
        /// Null nếu chưa được xử lý
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// ID của admin xử lý yêu cầu
        /// Null nếu chưa được xử lý
        /// </summary>
        public Guid? ProcessedById { get; set; }

        /// <summary>
        /// Ghi chú từ admin khi xử lý yêu cầu
        /// VD: lý do từ chối, thông tin chuyển khoản, etc.
        /// </summary>
        [StringLength(1000)]
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Ghi chú từ user khi tạo yêu cầu (tùy chọn)
        /// </summary>
        [StringLength(500)]
        public string? UserNotes { get; set; }

        /// <summary>
        /// Mã tham chiếu giao dịch (nếu có)
        /// Dùng để tracking khi chuyển tiền thực tế
        /// </summary>
        [StringLength(100)]
        public string? TransactionReference { get; set; }

        /// <summary>
        /// Số dư ví tại thời điểm tạo yêu cầu
        /// Để audit và kiểm tra tính hợp lệ
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal WalletBalanceAtRequest { get; set; }

        /// <summary>
        /// Phí rút tiền (nếu có)
        /// Mặc định = 0, có thể config sau
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal WithdrawalFee { get; set; } = 0;

        /// <summary>
        /// Số tiền thực tế user nhận được (Amount - WithdrawalFee)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal NetAmount => Amount - WithdrawalFee;

        // Navigation Properties

        /// <summary>
        /// User tạo yêu cầu rút tiền
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Tài khoản ngân hàng nhận tiền
        /// </summary>
        public virtual BankAccount BankAccount { get; set; } = null!;

        /// <summary>
        /// Admin xử lý yêu cầu (nếu có)
        /// </summary>
        public virtual User? ProcessedBy { get; set; }
    }
}
