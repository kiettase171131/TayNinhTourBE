using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đại diện cho một sự cố được báo cáo bởi HDV trong quá trình tour
    /// Cho phép HDV báo cáo các vấn đề phát sinh và thông báo cho admin
    /// </summary>
    public class TourIncident : BaseEntity
    {
        /// <summary>
        /// ID của TourOperation mà sự cố này xảy ra
        /// </summary>
        [Required]
        public Guid TourOperationId { get; set; }

        /// <summary>
        /// ID của TourGuide báo cáo sự cố này
        /// </summary>
        [Required]
        public Guid ReportedByGuideId { get; set; }

        /// <summary>
        /// Tiêu đề ngắn gọn của sự cố
        /// Ví dụ: "Xe bị hỏng", "Khách bị ốm", "Thời tiết xấu"
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả chi tiết về sự cố
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Mức độ nghiêm trọng của sự cố
        /// "Low", "Medium", "High", "Critical"
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Severity { get; set; } = "Medium";

        /// <summary>
        /// Trạng thái xử lý sự cố
        /// "Reported", "InProgress", "Resolved", "Closed"
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Reported";

        /// <summary>
        /// URLs của các hình ảnh liên quan (JSON array)
        /// Ví dụ: ["url1", "url2", "url3"]
        /// </summary>
        public string? ImageUrls { get; set; }

        /// <summary>
        /// Thời gian báo cáo sự cố
        /// </summary>
        [Required]
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ghi chú bổ sung từ admin khi xử lý
        /// </summary>
        [StringLength(500)]
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Thời gian admin bắt đầu xử lý
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// Thời gian hoàn thành xử lý sự cố
        /// </summary>
        public DateTime? ResolvedAt { get; set; }

        // Navigation Properties

        /// <summary>
        /// TourOperation mà sự cố này thuộc về
        /// </summary>
        public virtual TourOperation TourOperation { get; set; } = null!;

        /// <summary>
        /// TourGuide đã báo cáo sự cố này
        /// </summary>
        public virtual TourGuide ReportedByGuide { get; set; } = null!;

        /// <summary>
        /// User đã tạo incident này (thường là TourGuide)
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// User đã cập nhật incident này lần cuối (có thể là Admin)
        /// </summary>
        public virtual User? UpdatedBy { get; set; }
    }
}
