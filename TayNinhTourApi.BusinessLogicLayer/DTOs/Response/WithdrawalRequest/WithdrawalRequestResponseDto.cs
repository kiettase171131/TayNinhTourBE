using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.BankAccount;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.WithdrawalRequest
{
    /// <summary>
    /// DTO cho response thông tin yêu cầu rút tiền
    /// </summary>
    public class WithdrawalRequestResponseDto
    {
        /// <summary>
        /// ID của yêu cầu rút tiền
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của user tạo yêu cầu
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// ID của tài khoản ngân hàng
        /// </summary>
        public Guid BankAccountId { get; set; }

        /// <summary>
        /// Thông tin tài khoản ngân hàng
        /// </summary>
        public BankAccountInfo BankAccount { get; set; } = null!;

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
        /// Có thể hủy yêu cầu không (chỉ khi status = Pending)
        /// </summary>
        public bool CanCancel { get; set; }

        /// <summary>
        /// Số dư ví tại thời điểm tạo yêu cầu
        /// </summary>
        public decimal WalletBalanceAtRequest { get; set; }
    }

    /// <summary>
    /// Thông tin tài khoản ngân hàng trong withdrawal request
    /// </summary>
    public class BankAccountInfo
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
        /// Số tài khoản đã được mask
        /// </summary>
        public string MaskedAccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Tên chủ tài khoản
        /// </summary>
        public string AccountHolderName { get; set; } = string.Empty;
    }
}
