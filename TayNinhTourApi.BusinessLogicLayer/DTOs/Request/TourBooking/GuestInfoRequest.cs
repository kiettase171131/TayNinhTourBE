using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBooking
{
    /// <summary>
    /// DTO chứa thông tin của từng khách hàng trong tour booking
    /// Được sử dụng khi user nhập thông tin cho từng vé đã đặt
    /// </summary>
    public class GuestInfoRequest
    {
        /// <summary>
        /// Họ và tên của khách hàng (bắt buộc)
        /// </summary>
        [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên khách hàng phải từ 2-100 ký tự")]
        public string GuestName { get; set; } = null!;

        /// <summary>
        /// Email của khách hàng (bắt buộc, phải unique trong cùng booking)
        /// </summary>
        [Required(ErrorMessage = "Email khách hàng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string GuestEmail { get; set; } = null!;

        /// <summary>
        /// Số điện thoại của khách hàng (tùy chọn)
        /// </summary>
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        [RegularExpression(@"^[\d\s\-\+\(\)]+$", ErrorMessage = "Số điện thoại chỉ được chứa số và các ký tự đặc biệt: +, -, (, ), space")]
        public string? GuestPhone { get; set; }
    }
}
