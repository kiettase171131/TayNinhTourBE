using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đại diện cho một mục trong timeline của TourDetails
    /// Mỗi TimelineItem định nghĩa một điểm dừng/hoạt động cụ thể trong lịch trình tour
    /// </summary>
    public class TimelineItem : BaseEntity
    {
        /// <summary>
        /// ID của TourDetails mà timeline item này thuộc về
        /// </summary>
        [Required]
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// Thời gian check-in cho hoạt động này (giờ:phút)
        /// Ví dụ: 05:00, 07:00, 09:00, 10:00
        /// </summary>
        [Required]
        public TimeSpan CheckInTime { get; set; }

        /// <summary>
        /// Tên hoạt động hoặc mô tả ngắn
        /// Ví dụ: "Khởi hành", "Ăn sáng", "Ghé shop bánh tráng", "Tới Núi Bà"
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Activity { get; set; } = string.Empty;

        /// <summary>
        /// ID của specialty shop liên quan (nếu có)
        /// Nullable - chỉ có giá trị khi hoạt động liên quan đến một specialty shop cụ thể
        /// </summary>
        public Guid? SpecialtyShopId { get; set; }

        /// <summary>
        /// Thứ tự sắp xếp trong timeline (bắt đầu từ 1)
        /// Dùng để sắp xếp các hoạt động theo đúng trình tự thời gian
        /// </summary>
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "SortOrder phải lớn hơn 0")]
        public int SortOrder { get; set; }

        /// <summary>
        /// Trạng thái hoàn thành của timeline item (để HDV tick khi đã hoàn thành)
        /// </summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Thời gian hoàn thành thực tế của timeline item
        /// Được set khi HDV tick "Đã hoàn thành"
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Ghi chú bổ sung khi hoàn thành timeline item (optional)
        /// Ví dụ: ghi chú về tình hình thực tế, thay đổi so với kế hoạch
        /// </summary>
        [StringLength(500)]
        public string? CompletionNotes { get; set; }

        // Navigation Properties

        /// <summary>
        /// TourDetails mà timeline item này thuộc về
        /// </summary>
        public virtual TourDetails TourDetails { get; set; } = null!;

        /// <summary>
        /// SpecialtyShop liên quan đến hoạt động này (nếu có)
        /// </summary>
        public virtual SpecialtyShop? SpecialtyShop { get; set; }

        /// <summary>
        /// User đã tạo timeline item này
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// User đã cập nhật timeline item này lần cuối
        /// </summary>
        public virtual User? UpdatedBy { get; set; }
    }
}
