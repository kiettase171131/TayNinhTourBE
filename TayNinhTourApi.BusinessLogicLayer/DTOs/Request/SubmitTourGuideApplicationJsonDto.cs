using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Common;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request
{
    /// <summary>
    /// JSON DTO for TourGuide application submission (API testing friendly)
    /// </summary>
    public class SubmitTourGuideApplicationJsonDto
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
        /// Mô tả kinh nghiệm làm hướng dẫn viên
        /// </summary>
        [Required(ErrorMessage = "Kinh nghiệm là bắt buộc")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Mô tả kinh nghiệm phải từ 10-1000 ký tự")]
        public string Experience { get; set; } = null!;



        /// <summary>
        /// Kỹ năng của hướng dẫn viên (comma-separated TourGuideSkill enum values)
        /// Ví dụ: "Vietnamese,English,History,MountainClimbing"
        /// </summary>
        [Required(ErrorMessage = "Kỹ năng là bắt buộc")]
        [ValidSkillSelection(ErrorMessage = "Ít nhất một kỹ năng hợp lệ phải được chọn")]
        [StringLength(500, ErrorMessage = "Kỹ năng không được quá 500 ký tự")]
        public string? SkillsString { get; set; }

        /// <summary>
        /// URL của file CV (đã upload trước đó)
        /// </summary>
        [Required(ErrorMessage = "URL CV là bắt buộc")]
        [Url(ErrorMessage = "URL CV không hợp lệ")]
        [StringLength(500, ErrorMessage = "URL CV không được quá 500 ký tự")]
        public string CurriculumVitaeUrl { get; set; } = null!;
    }
}
