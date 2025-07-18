using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Common;
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
        /// Ngôn ngữ có thể sử dụng (DEPRECATED)
        /// </summary>
        public string? Languages { get; set; }

        /// <summary>
        /// Kỹ năng của hướng dẫn viên (Enhanced skill system)
        /// </summary>
        public List<TourGuideSkill> Skills { get; set; } = new();

        /// <summary>
        /// Kỹ năng dưới dạng comma-separated string
        /// </summary>
        public string? SkillsString { get; set; }

        /// <summary>
        /// Thông tin chi tiết về kỹ năng với tên hiển thị
        /// </summary>
        public List<SkillInfoDto> SkillsInfo { get; set; } = new();

        /// <summary>
        /// URL đến file CV
        /// </summary>
        public string? CurriculumVitaeUrl { get; set; }

        /// <summary>
        /// Tên file gốc của CV
        /// </summary>
        public string? CvOriginalFileName { get; set; }

        /// <summary>
        /// Kích thước file CV (bytes)
        /// </summary>
        public long? CvFileSize { get; set; }

        /// <summary>
        /// Content type của file CV
        /// </summary>
        public string? CvContentType { get; set; }

        /// <summary>
        /// Đường dẫn file CV trên server (for internal use)
        /// </summary>
        public string? CvFilePath { get; set; }

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
