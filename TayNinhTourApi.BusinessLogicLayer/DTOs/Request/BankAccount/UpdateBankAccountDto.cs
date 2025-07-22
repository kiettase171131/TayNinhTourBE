using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.BankAccount
{
    /// <summary>
    /// DTO cho việc cập nhật thông tin tài khoản ngân hàng
    /// </summary>
    public class UpdateBankAccountDto
    {
        /// <summary>
        /// Tên ngân hàng (VD: Vietcombank, Techcombank, BIDV, etc.)
        /// </summary>
        [Required(ErrorMessage = "Tên ngân hàng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên ngân hàng không được vượt quá 100 ký tự")]
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// Số tài khoản ngân hàng
        /// </summary>
        [Required(ErrorMessage = "Số tài khoản là bắt buộc")]
        [StringLength(50, ErrorMessage = "Số tài khoản không được vượt quá 50 ký tự")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Số tài khoản chỉ được chứa số")]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Tên chủ tài khoản (phải khớp với tên trên thẻ ngân hàng)
        /// </summary>
        [Required(ErrorMessage = "Tên chủ tài khoản là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên chủ tài khoản không được vượt quá 100 ký tự")]
        public string AccountHolderName { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú thêm về tài khoản (tùy chọn)
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }
}
