using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đại diện cho thông tin vận hành của một tour details cụ thể
    /// Mỗi TourOperation chứa thông tin về hướng dẫn viên, giá cả và capacity cho một TourDetails
    /// </summary>
    public class TourOperation : BaseEntity
    {
        /// <summary>
        /// ID của TourDetails mà operation này thuộc về
        /// Relationship: One-to-One với TourDetails
        /// </summary>
        [Required]
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// ID của TourGuide làm hướng dẫn viên cho tour này (optional)
        /// Relationship: Many-to-One với TourGuide
        /// </summary>
        public Guid? TourGuideId { get; set; }

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

        /// <summary>
        /// Số lượng khách đã booking hiện tại (confirmed bookings only)
        /// Được tính toán từ TourBookings với Status = Confirmed
        /// </summary>
        [Required]
        public int CurrentBookings { get; set; } = 0;

        /// <summary>
        /// Row version cho optimistic concurrency control
        /// Được sử dụng để prevent race conditions khi booking
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // Navigation Properties

        /// <summary>
        /// TourDetails mà operation này thuộc về
        /// Relationship: One-to-One
        /// </summary>
        public virtual TourDetails TourDetails { get; set; } = null!;

        /// <summary>
        /// TourGuide làm hướng dẫn viên cho tour này (optional)
        /// Relationship: Many-to-One with TourGuide
        /// </summary>
        public virtual TourGuide? TourGuide { get; set; }

        /// <summary>
        /// User đã tạo tour operation này
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// User đã cập nhật tour operation này lần cuối
        /// </summary>
        public virtual User? UpdatedBy { get; set; }

        /// <summary>
        /// Danh sách các booking cho tour operation này
        /// Relationship: One-to-Many
        /// </summary>
        public virtual ICollection<TourBooking> TourBookings { get; set; } = new List<TourBooking>();
    }
}
