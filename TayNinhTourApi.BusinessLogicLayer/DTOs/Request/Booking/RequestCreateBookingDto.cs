using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Booking
{
    /// <summary>
    /// DTO cho request tạo booking mới
    /// </summary>
    public class RequestCreateBookingDto
    {
        /// <summary>
        /// ID của TourOperation muốn booking
        /// </summary>
        [Required(ErrorMessage = "Vui lòng chọn tour operation")]
        public Guid TourOperationId { get; set; }

        /// <summary>
        /// Số lượng khách
        /// </summary>
        [Required(ErrorMessage = "Số lượng khách là bắt buộc")]
        [Range(1, 50, ErrorMessage = "Số lượng khách phải từ 1 đến 50")]
        public int NumberOfGuests { get; set; }

        /// <summary>
        /// Ghi chú từ khách hàng
        /// </summary>
        [StringLength(1000, ErrorMessage = "Ghi chú không quá 1000 ký tự")]
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Tên người liên hệ
        /// </summary>
        [Required(ErrorMessage = "Tên người liên hệ là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên liên hệ không quá 100 ký tự")]
        public string ContactName { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        [Required(ErrorMessage = "Số điện thoại liên hệ là bắt buộc")]
        [StringLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự")]
        [RegularExpression(@"^[0-9+\-\s\(\)]+$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string ContactPhone { get; set; } = string.Empty;

        /// <summary>
        /// Email liên hệ
        /// </summary>
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? ContactEmail { get; set; }
    }
}
