using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.BankAccount
{
    /// <summary>
    /// DTO cho việc tạo tài khoản ngân hàng mới
    /// </summary>
    [CustomBankNameValidation] // Custom validation attribute
    public class CreateBankAccountDto
    {
        /// <summary>
        /// ID ngân hàng từ enum SupportedBank (null nếu chọn "Other")
        /// </summary>
        public SupportedBank? SupportedBankId { get; set; }

        /// <summary>
        /// Tên ngân hàng (VD: Vietcombank, Techcombank, BIDV, etc.)
        /// - Nếu SupportedBankId có giá trị và != Other: tự động set từ enum
        /// - Nếu SupportedBankId = Other: user nhập tự do
        /// - Nếu SupportedBankId = null: user nhập tự do (backward compatibility)
        /// </summary>
        [Required(ErrorMessage = "Tên ngân hàng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên ngân hàng không được vượt quá 100 ký tự")]
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// Tên ngân hàng tự do khi chọn "Other" (chỉ dùng khi SupportedBankId = Other)
        /// </summary>
        [StringLength(100, ErrorMessage = "Tên ngân hàng tự do không được vượt quá 100 ký tự")]
        public string? CustomBankName { get; set; }

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
        /// Đặt làm tài khoản mặc định
        /// </summary>
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Ghi chú thêm về tài khoản (tùy chọn)
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Custom validation attribute để kiểm tra logic nghiệp vụ của CustomBankName
    /// </summary>
    public class CustomBankNameValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is CreateBankAccountDto dto)
            {
                // Nếu chọn "Other" (999) thì bắt buộc phải có CustomBankName
                if (dto.SupportedBankId == SupportedBank.Other)
                {
                    if (string.IsNullOrWhiteSpace(dto.CustomBankName))
                    {
                        ErrorMessage = "Tên ngân hàng tự do là bắt buộc khi chọn 'Ngân hàng khác'";
                        return false;
                    }
                }

                return true;
            }

            return false;
        }
    }
}
