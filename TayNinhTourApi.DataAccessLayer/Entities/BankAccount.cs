using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Thông tin tài khoản ngân hàng của user để thực hiện rút tiền
    /// Relationship: Many-to-One với User (một user có thể có nhiều tài khoản ngân hàng)
    /// </summary>
    public class BankAccount : BaseEntity
    {
        /// <summary>
        /// Foreign Key đến User
        /// Một user có thể có nhiều tài khoản ngân hàng
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Tên ngân hàng (VD: Vietcombank, Techcombank, BIDV, etc.)
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Tên ngân hàng không được vượt quá 100 ký tự")]
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// Số tài khoản ngân hàng
        /// Lưu ý: Không mã hóa vì cần hiển thị cho admin khi xử lý rút tiền
        /// </summary>
        [Required]
        [StringLength(50, ErrorMessage = "Số tài khoản không được vượt quá 50 ký tự")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Số tài khoản chỉ được chứa số")]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Tên chủ tài khoản (phải khớp với tên trên thẻ ngân hàng)
        /// </summary>
        [Required]
        [StringLength(100, ErrorMessage = "Tên chủ tài khoản không được vượt quá 100 ký tự")]
        public string AccountHolderName { get; set; } = string.Empty;

        /// <summary>
        /// Đánh dấu tài khoản mặc định cho rút tiền
        /// Chỉ có một tài khoản mặc định cho mỗi user
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Ghi chú thêm về tài khoản (tùy chọn)
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Thời gian xác minh tài khoản (nếu có)
        /// </summary>
        public DateTime? VerifiedAt { get; set; }

        /// <summary>
        /// ID của admin xác minh tài khoản (nếu có)
        /// </summary>
        public Guid? VerifiedById { get; set; }

        // Navigation Properties

        /// <summary>
        /// User sở hữu tài khoản ngân hàng này
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Admin đã xác minh tài khoản (nếu có)
        /// </summary>
        public virtual User? VerifiedBy { get; set; }

        /// <summary>
        /// Danh sách các yêu cầu rút tiền sử dụng tài khoản này
        /// </summary>
        public virtual ICollection<WithdrawalRequest> WithdrawalRequests { get; set; } = new List<WithdrawalRequest>();
    }
}
