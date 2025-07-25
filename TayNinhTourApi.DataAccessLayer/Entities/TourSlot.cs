using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đại diện cho một slot thời gian cụ thể được tạo từ TourTemplate
    /// Mỗi TourSlot là một instance cụ thể của tour với ngày giờ xác định
    /// </summary>
    public class TourSlot : BaseEntity
    {
        /// <summary>
        /// ID của TourTemplate mà slot này được tạo từ
        /// </summary>
        [Required]
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Ngày tour cụ thể sẽ diễn ra
        /// Sử dụng DateOnly để chỉ lưu ngày, không bao gồm thời gian
        /// </summary>
        [Required]
        public DateOnly TourDate { get; set; }

        /// <summary>
        /// Ngày trong tuần của tour (Saturday hoặc Sunday theo yêu cầu hệ thống)
        /// Được tính toán tự động từ TourDate nhưng lưu riêng để query hiệu quả
        /// </summary>
        [Required]
        public ScheduleDay ScheduleDay { get; set; }

        /// <summary>
        /// Trạng thái của tour slot
        /// </summary>
        [Required]
        public TourSlotStatus Status { get; set; } = TourSlotStatus.Available;

        /// <summary>
        /// ID của TourDetails được assign cho slot này (auto-assign khi tạo TourDetails)
        /// Nullable - slot có thể chưa có lịch trình cụ thể
        /// </summary>
        public Guid? TourDetailsId { get; set; }

        /// <summary>
        /// Số lượng khách tối đa cho slot này (lấy từ TourOperation khi được assign)
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "Số lượng khách tối đa phải từ 1 đến 100")]
        public int MaxGuests { get; set; } = 0;

        /// <summary>
        /// Số lượng khách hiện tại đã booking cho slot này
        /// </summary>
        [Required]
        [Range(0, 100, ErrorMessage = "Số lượng khách hiện tại phải từ 0 đến 100")]
        public int CurrentBookings { get; set; } = 0;

        /// <summary>
        /// Số ghế còn lại (computed field)
        /// </summary>
        public int AvailableSpots => MaxGuests - CurrentBookings;

        /// <summary>
        /// Trạng thái slot có sẵn sàng để booking không
        /// Khác với BaseEntity.IsActive (dùng cho soft delete)
        /// - true: Slot có thể được booking
        /// - false: Slot tạm thời không available (do thời tiết, bảo trì, etc.)
        /// </summary>
        public new bool IsActive { get; set; } = true;

        /// <summary>
        /// Row version cho optimistic concurrency control
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // Navigation Properties

        /// <summary>
        /// TourTemplate mà slot này được tạo từ
        /// Relationship: Many TourSlots to One TourTemplate
        /// </summary>
        public virtual TourTemplate TourTemplate { get; set; } = null!;

        /// <summary>
        /// TourDetails được assign cho slot này (nullable)
        /// Relationship: Many TourSlots to One TourDetails
        /// </summary>
        public virtual TourDetails? TourDetails { get; set; }

        /// <summary>
        /// User đã tạo slot này
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// User đã cập nhật slot này lần cuối
        /// </summary>
        public virtual User? UpdatedBy { get; set; }

        /// <summary>
        /// Danh sách các bookings cho slot này
        /// </summary>
        public virtual ICollection<TourBooking> Bookings { get; set; } = new List<TourBooking>();
    }
}
