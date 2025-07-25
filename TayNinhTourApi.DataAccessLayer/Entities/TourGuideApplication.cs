﻿using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Enhanced TourGuide Application entity
    /// Nâng cấp từ version cũ với đầy đủ fields cần thiết
    /// </summary>
    public class TourGuideApplication : BaseEntity
    {
        /// <summary>
        /// Foreign Key đến User đăng ký
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Họ tên đầy đủ của ứng viên
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = null!;

        /// <summary>
        /// Email liên hệ
        /// </summary>
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Mô tả kinh nghiệm làm hướng dẫn viên (Enhanced version)
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Experience { get; set; } = null!;

        /// <summary>
        /// Ngôn ngữ có thể sử dụng (VN, EN, CN...)
        /// DEPRECATED: Sử dụng Skills field thay thế
        /// </summary>
        [StringLength(200)]
        public string? Languages { get; set; }

        /// <summary>
        /// Kỹ năng của hướng dẫn viên (comma-separated TourGuideSkill enum values)
        /// Thay thế cho Languages field với hệ thống skill toàn diện
        /// Ví dụ: "Vietnamese,English,History,MountainClimbing"
        /// </summary>
        [StringLength(500)]
        public string? Skills { get; set; }

        /// <summary>
        /// URL đến file CV
        /// </summary>
        [StringLength(500)]
        public string? CurriculumVitae { get; set; }

        /// <summary>
        /// Tên file gốc của CV
        /// </summary>
        [StringLength(255)]
        public string? CvOriginalFileName { get; set; }

        /// <summary>
        /// Kích thước file CV (bytes)
        /// </summary>
        public long? CvFileSize { get; set; }

        /// <summary>
        /// Content type của file CV
        /// </summary>
        [StringLength(100)]
        public string? CvContentType { get; set; }

        /// <summary>
        /// Đường dẫn file CV trên server (relative path)
        /// </summary>
        [StringLength(500)]
        public string? CvFilePath { get; set; }

        /// <summary>
        /// Trạng thái đơn đăng ký
        /// </summary>
        [Required]
        public TourGuideApplicationStatus Status { get; set; } = TourGuideApplicationStatus.Pending;

        /// <summary>
        /// Lý do từ chối (nếu có)
        /// </summary>
        [StringLength(500)]
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Thời gian nộp đơn
        /// </summary>
        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian xử lý đơn
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// ID của admin xử lý đơn
        /// </summary>
        public Guid? ProcessedById { get; set; }

        // Navigation Properties

        /// <summary>
        /// User đăng ký TourGuide
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Admin xử lý đơn (nếu có)
        /// </summary>
        public virtual User? ProcessedBy { get; set; }

        /// <summary>
        /// Approved TourGuide record (if application is approved)
        /// One-to-One relationship
        /// </summary>
        public virtual TourGuide? ApprovedTourGuide { get; set; }
    }
}
