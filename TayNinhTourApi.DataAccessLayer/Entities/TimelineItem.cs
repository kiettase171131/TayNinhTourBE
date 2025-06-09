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
        /// TourDetails mà timeline item này thuộc về
        /// </summary>
        public virtual TourDetails TourDetails { get; set; } = null!;

        /// <summary>
        /// Shop liên quan đến hoạt động này (nếu có)
        /// </summary>
        public virtual Shop? Shop { get; set; }

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
