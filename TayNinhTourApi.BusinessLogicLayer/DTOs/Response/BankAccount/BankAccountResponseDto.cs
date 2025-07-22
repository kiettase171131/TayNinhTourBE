namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.BankAccount
{
    /// <summary>
    /// DTO cho response thông tin tài khoản ngân hàng
    /// </summary>
    public class BankAccountResponseDto
    {
        /// <summary>
        /// ID của tài khoản ngân hàng
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của user sở hữu tài khoản
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// Số tài khoản ngân hàng (có thể mask một phần cho security)
        /// </summary>
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Số tài khoản đã được mask để hiển thị (VD: ****1234)
        /// </summary>
        public string MaskedAccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Tên chủ tài khoản
        /// </summary>
        public string AccountHolderName { get; set; } = string.Empty;

        /// <summary>
        /// Có phải tài khoản mặc định không
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Ghi chú về tài khoản
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Thời gian xác minh tài khoản (nếu có)
        /// </summary>
        public DateTime? VerifiedAt { get; set; }

        /// <summary>
        /// Tên admin đã xác minh (nếu có)
        /// </summary>
        public string? VerifiedByName { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian cập nhật cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Số lượng yêu cầu rút tiền đã sử dụng tài khoản này
        /// </summary>
        public int WithdrawalRequestCount { get; set; }

        /// <summary>
        /// Có thể xóa tài khoản này không (không có withdrawal request pending)
        /// </summary>
        public bool CanDelete { get; set; }
    }
}
