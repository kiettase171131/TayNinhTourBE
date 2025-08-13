using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đại diện cho chi tiết lịch trình của một tour template
    /// Mỗi TourDetails định nghĩa một lịch trình cụ thể có thể được assign cho các TourSlots
    /// </summary>
    public class TourDetails : BaseEntity
    {
        /// <summary>
        /// ID của tour template mà chi tiết này thuộc về
        /// </summary>
        [Required]
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Tiêu đề của lịch trình
        /// Ví dụ: "Lịch trình VIP", "Lịch trình thường", "Lịch trình tiết kiệm"
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả về lịch trình này
        /// Ví dụ: "Lịch trình cao cấp với các dịch vụ VIP"
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Trạng thái duyệt của tour details
        /// </summary>
        [Required]
        public TourDetailsStatus Status { get; set; } = TourDetailsStatus.Pending;

        /// <summary>
        /// Bình luận từ admin khi duyệt/từ chối tour details
        /// </summary>
        [StringLength(1000)]
        public string? CommentApproved { get; set; }

        /// <summary>
        /// Kỹ năng yêu cầu cho hướng dẫn viên (comma-separated)
        /// </summary>
        [StringLength(500)]
        public string? SkillsRequired { get; set; }

        /// <summary>
        /// Danh sách URL hình ảnh cho tour details này
        /// Lưu dưới dạng JSON array trong database
        /// </summary>
        [Required]
        public List<string> ImageUrls { get; set; } = new List<string>();

        // Navigation Properties

        /// <summary>
        /// Tour template mà chi tiết này thuộc về
        /// </summary>
        public virtual TourTemplate TourTemplate { get; set; } = null!;

        /// <summary>
        /// TourOperation cho lịch trình này (1:1 relationship)
        /// </summary>
        public virtual TourOperation? TourOperation { get; set; }

        /// <summary>
        /// Timeline items thuộc về lịch trình này
        /// </summary>
        public virtual ICollection<TimelineItem> Timeline { get; set; } = new List<TimelineItem>();

        /// <summary>
        /// TourSlots được assign lịch trình này
        /// </summary>
        public virtual ICollection<TourSlot> AssignedSlots { get; set; } = new List<TourSlot>();

        /// <summary>
        /// Danh sách SpecialtyShop được mời tham gia tour này
        /// </summary>
        public virtual ICollection<TourDetailsSpecialtyShop> InvitedSpecialtyShops { get; set; } = new List<TourDetailsSpecialtyShop>();

        /// <summary>
        /// User đã tạo tour detail này
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// User đã cập nhật tour detail này lần cuối
        /// </summary>
        public virtual User? UpdatedBy { get; set; }
    }
}
