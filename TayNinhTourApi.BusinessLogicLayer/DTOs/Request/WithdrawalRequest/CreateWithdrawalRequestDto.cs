using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.WithdrawalRequest
{
    /// <summary>
    /// DTO cho việc tạo yêu cầu rút tiền mới
    /// </summary>
    public class CreateWithdrawalRequestDto
    {
        /// <summary>
        /// ID của tài khoản ngân hàng nhận tiền
        /// </summary>
        [Required(ErrorMessage = "Tài khoản ngân hàng là bắt buộc")]
        public Guid BankAccountId { get; set; }

        /// <summary>
        /// Số tiền yêu cầu rút (VNĐ)
        /// </summary>
        [Required(ErrorMessage = "Số tiền rút là bắt buộc")]
        [Range(1000, double.MaxValue, ErrorMessage = "Số tiền rút tối thiểu là 1,000 VNĐ")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Ghi chú từ user (tùy chọn)
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? UserNotes { get; set; }
    }
}
