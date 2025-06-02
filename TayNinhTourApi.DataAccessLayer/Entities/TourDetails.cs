using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đại diện cho chi tiết timeline của một tour template
    /// Mỗi TourDetails định nghĩa một điểm dừng/hoạt động cụ thể trong lịch trình tour
    /// </summary>
    public class TourDetails : BaseEntity
    {
        /// <summary>
        /// ID của tour template mà chi tiết này thuộc về
        /// </summary>
        [Required]
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Thời gian trong ngày cho hoạt động này (giờ:phút)
        /// Ví dụ: 08:30, 14:00, 16:45
        /// </summary>
        [Required]
        public TimeOnly TimeSlot { get; set; }

        /// <summary>
        /// Địa điểm hoặc tên hoạt động
        /// Ví dụ: "Núi Bà Đen", "Chùa Cao Đài", "Nhà hàng ABC"
        /// </summary>
        [StringLength(500)]
        public string? Location { get; set; }

        /// <summary>
        /// Mô tả chi tiết về hoạt động tại điểm dừng này
        /// Ví dụ: "Tham quan và chụp ảnh tại đỉnh núi", "Dùng bữa trưa đặc sản địa phương"
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// ID của shop liên quan (nếu có)
        /// Nullable - chỉ có giá trị khi hoạt động liên quan đến một shop cụ thể
        /// </summary>
        public Guid? ShopId { get; set; }

        /// <summary>
        /// Thứ tự sắp xếp trong timeline (bắt đầu từ 1)
        /// Dùng để sắp xếp các hoạt động theo đúng trình tự thời gian
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "SortOrder phải lớn hơn 0")]
        public int SortOrder { get; set; }

        // Navigation Properties

        /// <summary>
        /// Tour template mà chi tiết này thuộc về
        /// </summary>
        public virtual TourTemplate TourTemplate { get; set; } = null!;

        /// <summary>
        /// Shop liên quan đến hoạt động này (nếu có)
        /// </summary>
        public virtual Shop? Shop { get; set; }
    }
}
