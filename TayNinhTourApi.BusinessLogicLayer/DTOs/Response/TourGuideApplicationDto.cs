using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response
{
    /// <summary>
    /// Detailed TourGuide application DTO for admin/user view
    /// </summary>
    public class TourGuideApplicationDto
    {
        /// <summary>
        /// ID của đơn đăng ký
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Họ tên đầy đủ của ứng viên
        /// </summary>
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        public string PhoneNumber { get; set; } = null!;

        /// <summary>
        /// Email liên hệ
        /// </summary>
        public string Email { get; set; } = null!;

        /// <summary>
        /// Mô tả kinh nghiệm (Enhanced version)
        /// </summary>
        public string Experience { get; set; } = null!;

        /// <summary>
        /// Ngôn ngữ có thể sử dụng
        /// </summary>
        public string? Languages { get; set; }

        /// <summary>
        /// URL đến file CV
        /// </summary>
        public string? CurriculumVitaeUrl { get; set; }

        /// <summary>
        /// Trạng thái đơn đăng ký
        /// </summary>
        public TourGuideApplicationStatus Status { get; set; }

        /// <summary>
        /// Lý do từ chối (nếu có)
        /// </summary>
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Thời gian nộp đơn
        /// </summary>
        public DateTime SubmittedAt { get; set; }

        /// <summary>
        /// Thời gian xử lý đơn
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Thông tin user đăng ký
        /// </summary>
        public UserSummaryDto? UserInfo { get; set; }

        /// <summary>
        /// Thông tin admin xử lý (nếu có)
        /// </summary>
        public UserSummaryDto? ProcessedByInfo { get; set; }
    }
}
