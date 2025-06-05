using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đại diện cho thông tin vận hành của một tour slot cụ thể
    /// Mỗi TourOperation chứa thông tin về hướng dẫn viên, giá cả và capacity cho một TourSlot
    /// </summary>
    public class TourOperation : BaseEntity
    {
        /// <summary>
        /// ID của TourSlot mà operation này thuộc về
        /// Relationship: One-to-One với TourSlot
        /// </summary>
        [Required]
        public Guid TourSlotId { get; set; }

        /// <summary>
        /// ID của User làm hướng dẫn viên cho tour này (optional)
        /// Relationship: Many-to-One với User (Guide)
        /// </summary>
        public Guid? GuideId { get; set; }

        /// <summary>
        /// Giá tour cho operation này
        /// Có thể khác với giá gốc trong TourTemplate tùy theo điều kiện thực tế
        /// </summary>
        [Required]
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng khách tối đa cho tour operation này
        /// Có thể khác với MaxGuests trong TourTemplate tùy theo điều kiện thực tế
        /// </summary>
        [Required]
        public int MaxGuests { get; set; }

        /// <summary>
        /// Mô tả bổ sung cho tour operation
        /// Ví dụ: ghi chú về thời tiết, điều kiện đặc biệt, thay đổi lịch trình
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Ghi chú bổ sung cho tour operation
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        /// <summary>
        /// Trạng thái của tour operation
        /// </summary>
        [Required]
        public TourOperationStatus Status { get; set; } = TourOperationStatus.Scheduled;

        /// <summary>
        /// Trạng thái hoạt động của tour operation
        /// Khác với BaseEntity.IsActive (dùng cho soft delete)
        /// - true: Operation đang hoạt động và có thể booking
        /// - false: Operation tạm thời không hoạt động (guide bận, thời tiết xấu, etc.)
        /// </summary>
        public new bool IsActive { get; set; } = true;

        // Navigation Properties

        /// <summary>
        /// TourSlot mà operation này thuộc về
        /// Relationship: One-to-One
        /// </summary>
        public virtual TourSlot TourSlot { get; set; } = null!;

        /// <summary>
        /// User làm hướng dẫn viên cho tour này (optional)
        /// Relationship: Many-to-One
        /// </summary>
        public virtual User? Guide { get; set; }

        /// <summary>
        /// User đã tạo tour operation này
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// User đã cập nhật tour operation này lần cuối
        /// </summary>
        public virtual User? UpdatedBy { get; set; }
    }
}
