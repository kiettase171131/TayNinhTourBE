using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBookingRefund
{
    /// <summary>
    /// DTO cho việc tạo yêu cầu hoàn tiền tour booking mới (customer-initiated)
    /// Chỉ dành cho user cancellation, không dùng cho company/auto cancellation
    /// </summary>
    public class CreateTourRefundRequestDto
    {
        /// <summary>
        /// ID của tour booking cần hoàn tiền
        /// </summary>
        [Required(ErrorMessage = "Tour booking ID là bắt buộc")]
        public Guid TourBookingId { get; set; }

        /// <summary>
        /// Lý do hoàn tiền từ customer
        /// </summary>
        [Required(ErrorMessage = "Lý do hoàn tiền là bắt buộc")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Lý do hoàn tiền phải từ 10-1000 ký tự")]
        public string RefundReason { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú thêm từ customer (tùy chọn)
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Thông tin ngân hàng để nhận tiền hoàn
        /// </summary>
        [Required(ErrorMessage = "Thông tin ngân hàng là bắt buộc")]
        public CustomerBankInfoDto BankInfo { get; set; } = null!;
    }

    /// <summary>
    /// Thông tin ngân hàng của customer để nhận tiền hoàn
    /// </summary>
    public class CustomerBankInfoDto
    {
        /// <summary>
        /// Tên ngân hàng
        /// </summary>
        [Required(ErrorMessage = "Tên ngân hàng là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên ngân hàng phải từ 2-100 ký tự")]
        public string BankName { get; set; } = string.Empty;

        /// <summary>
        /// Số tài khoản ngân hàng
        /// </summary>
        [Required(ErrorMessage = "Số tài khoản là bắt buộc")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Số tài khoản phải từ 6-50 ký tự")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Số tài khoản chỉ được chứa số")]
        public string AccountNumber { get; set; } = string.Empty;

        /// <summary>
        /// Tên chủ tài khoản
        /// </summary>
        [Required(ErrorMessage = "Tên chủ tài khoản là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên chủ tài khoản phải từ 2-100 ký tự")]
        public string AccountHolderName { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho việc tạo refund request từ hệ thống (company/auto cancellation)
    /// Chỉ dành cho internal use, không expose qua API
    /// </summary>
    public class CreateSystemRefundRequestDto
    {
        /// <summary>
        /// ID của tour booking cần hoàn tiền
        /// </summary>
        public Guid TourBookingId { get; set; }

        /// <summary>
        /// Loại hoàn tiền (CompanyCancellation hoặc AutoCancellation)
        /// </summary>
        public TourRefundType RefundType { get; set; }

        /// <summary>
        /// Lý do hoàn tiền từ hệ thống
        /// </summary>
        public string RefundReason { get; set; } = string.Empty;

        /// <summary>
        /// Số tiền gốc của booking
        /// </summary>
        public decimal OriginalAmount { get; set; }

        /// <summary>
        /// Số tiền yêu cầu hoàn (thường = OriginalAmount cho system refund)
        /// </summary>
        public decimal RequestedAmount { get; set; }

        /// <summary>
        /// Số ngày trước tour (để tính policy nếu cần)
        /// </summary>
        public int? DaysBeforeTour { get; set; }

        /// <summary>
        /// Phần trăm hoàn tiền theo policy
        /// </summary>
        public decimal? RefundPercentage { get; set; }

        /// <summary>
        /// ID của user tạo request (system user hoặc admin)
        /// </summary>
        public Guid CreatedById { get; set; }
    }

    /// <summary>
    /// DTO cho việc hủy refund request (chỉ khi status = Pending)
    /// </summary>
    public class CancelRefundRequestDto
    {
        /// <summary>
        /// Lý do hủy yêu cầu hoàn tiền
        /// </summary>
        [Required(ErrorMessage = "Lý do hủy là bắt buộc")]
        [StringLength(500, MinimumLength = 5, ErrorMessage = "Lý do hủy phải từ 5-500 ký tự")]
        public string CancellationReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho việc cập nhật thông tin ngân hàng của refund request
    /// </summary>
    public class UpdateRefundBankInfoDto
    {
        /// <summary>
        /// Thông tin ngân hàng mới
        /// </summary>
        [Required(ErrorMessage = "Thông tin ngân hàng là bắt buộc")]
        public CustomerBankInfoDto BankInfo { get; set; } = null!;

        /// <summary>
        /// Lý do thay đổi thông tin ngân hàng
        /// </summary>
        [StringLength(200, ErrorMessage = "Lý do thay đổi không được vượt quá 200 ký tự")]
        public string? ChangeReason { get; set; }
    }
}
