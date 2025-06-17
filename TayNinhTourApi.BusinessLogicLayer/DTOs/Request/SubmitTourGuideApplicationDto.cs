using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request
{
    /// <summary>
    /// Enhanced DTO for TourGuide application submission
    /// Simplified version với chỉ các fields cần thiết
    /// </summary>
    public class SubmitTourGuideApplicationDto
    {
        /// <summary>
        /// Họ tên đầy đủ của ứng viên
        /// </summary>
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-100 ký tự")]
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        public string PhoneNumber { get; set; } = null!;

        /// <summary>
        /// Email liên hệ
        /// </summary>
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(100, ErrorMessage = "Email không được quá 100 ký tự")]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Số năm kinh nghiệm làm hướng dẫn viên
        /// </summary>
        [Required(ErrorMessage = "Kinh nghiệm là bắt buộc")]
        [Range(0, 50, ErrorMessage = "Kinh nghiệm phải từ 0-50 năm")]
        public int Experience { get; set; }

        /// <summary>
        /// Ngôn ngữ có thể sử dụng (VN, EN, CN...)
        /// </summary>
        [StringLength(200, ErrorMessage = "Ngôn ngữ không được quá 200 ký tự")]
        public string? Languages { get; set; }

        /// <summary>
        /// File CV (PDF format)
        /// </summary>
        [Required(ErrorMessage = "CV là bắt buộc")]
        public IFormFile CurriculumVitae { get; set; } = null!;
    }
}
