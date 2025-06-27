using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Common;
using TayNinhTourApi.BusinessLogicLayer.Attributes;

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
        /// Mô tả kinh nghiệm làm hướng dẫn viên
        /// </summary>
        [Required(ErrorMessage = "Kinh nghiệm là bắt buộc")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Mô tả kinh nghiệm phải từ 10-1000 ký tự")]
        public string Experience { get; set; } = null!;



        /// <summary>
        /// Kỹ năng của hướng dẫn viên (Enhanced skill system)
        /// </summary>
        [Required(ErrorMessage = "Kỹ năng là bắt buộc")]
        [ValidSkillSelection(ErrorMessage = "Ít nhất một kỹ năng hợp lệ phải được chọn")]
        public List<TourGuideSkill> Skills { get; set; } = new();

        /// <summary>
        /// Kỹ năng dưới dạng comma-separated string (for API compatibility)
        /// Ví dụ: "Vietnamese,English,History,MountainClimbing"
        /// </summary>
        [ValidSkillSelection(ErrorMessage = "Ít nhất một kỹ năng hợp lệ phải được chọn")]
        [StringLength(500, ErrorMessage = "Kỹ năng không được quá 500 ký tự")]
        public string? SkillsString { get; set; }

        /// <summary>
        /// File CV (PDF, DOC, DOCX, PNG, JPG, JPEG, WEBP format)
        /// </summary>
        [Required(ErrorMessage = "CV là bắt buộc")]
        [CvFileValidation]
        public IFormFile CurriculumVitae { get; set; } = null!;
    }
}
