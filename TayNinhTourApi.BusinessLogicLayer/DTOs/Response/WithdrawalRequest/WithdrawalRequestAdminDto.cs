using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.WithdrawalRequest
{
    /// <summary>
    /// DTO cho admin xem danh sách yêu cầu rút tiền
    /// </summary>
    public class WithdrawalRequestAdminDto
    {
        /// <summary>
        /// ID của yêu cầu rút tiền
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Thông tin user tạo yêu cầu
        /// </summary>
        public UserSummary User { get; set; } = null!;

        /// <summary>
        /// Thông tin tài khoản ngân hàng
        /// </summary>
        public BankAccountSummary BankAccount { get; set; } = null!;

        /// <summary>
        /// Số tiền yêu cầu rút (VNĐ)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Phí rút tiền (nếu có)
        /// </summary>
        public decimal WithdrawalFee { get; set; }

        /// <summary>
        /// Số tiền thực tế nhận được
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Trạng thái của yêu cầu
        /// </summary>
        public WithdrawalStatus Status { get; set; }

        /// <summary>
        /// Tên trạng thái (để hiển thị)
        /// </summary>
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian tạo yêu cầu
        /// </summary>
        public DateTime RequestedAt { get; set; }

        /// <summary>
        /// Thời gian xử lý (nếu có)
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Tên admin xử lý (nếu có)
        /// </summary>
        public string? ProcessedByName { get; set; }

        /// <summary>
        /// Ghi chú từ user
        /// </summary>
        public string? UserNotes { get; set; }

        /// <summary>
        /// Ghi chú từ admin (nếu có)
        /// </summary>
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Mã tham chiếu giao dịch (nếu có)
        /// </summary>
        public string? TransactionReference { get; set; }

        /// <summary>
        /// Số dư ví tại thời điểm tạo yêu cầu
        /// </summary>
        public decimal WalletBalanceAtRequest { get; set; }

        /// <summary>
        /// Số ngày đã chờ xử lý
        /// </summary>
        public int DaysPending { get; set; }

        /// <summary>
        /// Độ ưu tiên (dựa trên thời gian chờ, số tiền, etc.)
        /// </summary>
        public string Priority { get; set; } = "Normal";
    }

    /// <summary>
    /// Thông tin tóm tắt user
    /// </summary>
    public class UserSummary
    {
        /// <summary>
        /// ID của user
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên đầy đủ
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Tên shop (nếu có)
        /// </summary>
        public string? ShopName { get; set; }
    }

    /// <summary>
    /// Thông tin tóm tắt tài khoản ngân hàng
    /// </summary>
    public class BankAccountSummary
    {
        /// <summary>
        /// ID của tài khoản ngân hàng
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// Số tài khoản (full cho admin)
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Tên chủ tài khoản
        /// </summary>
        public string AccountHolderName { get; set; } = string.Empty;

        /// <summary>
        /// Đã được xác minh chưa
        /// </summary>
        public bool IsVerified { get; set; }
    }
}
