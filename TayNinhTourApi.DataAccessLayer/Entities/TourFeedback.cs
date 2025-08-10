using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Feedback của user cho một lượt tham gia tour (gắn với Booking và Slot)
    /// </summary>
    public class TourFeedback : BaseEntity
    {
        /// <summary>
        /// Booking mà feedback này thuộc về (bắt buộc).
        /// Đảm bảo user đã thật sự book và tham gia slot đó.
        /// </summary>
        [Required]
        public Guid TourBookingId { get; set; }

        /// <summary>
        /// Slot mà user đã đi và muốn feedback vào (bắt buộc).
        /// </summary>
        [Required]
        public Guid TourSlotId { get; set; }

        /// <summary>
        /// User tạo feedback (bắt buộc) - phải trùng với User của booking.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Rating cho trải nghiệm Tour (1-5).
        /// </summary>
        [Range(1, 5)]
        public int TourRating { get; set; }

        /// <summary>
        /// Nhận xét cho tour (optional).
        /// </summary>
        [StringLength(2000)]
        public string? TourComment { get; set; }

        /// <summary>
        /// Rating cho TourGuide đã dẫn tour (optional 1-5).
        /// Nếu slot đó có guide thì cho phép lưu.
        /// </summary>
        [Range(1, 5)]
        public int? GuideRating { get; set; }

        /// <summary>
        /// Nhận xét riêng cho guide (optional).
        /// </summary>
        [StringLength(2000)]
        public string? GuideComment { get; set; }

        /// <summary>
        /// ID tour guide được chấm (nullable, vì có tour không có guide cố định).
        /// </summary>
        public Guid? TourGuideId { get; set; }

        /// <summary>
        /// RowVersion cho optimistic concurrency
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // Navigation
        public virtual TourBooking TourBooking { get; set; } = null!;
        public virtual TourSlot TourSlot { get; set; } = null!;
        public virtual User User { get; set; } = null!;
        public virtual TourGuide? TourGuide { get; set; }
    }
}
