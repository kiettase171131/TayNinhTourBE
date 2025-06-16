using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đại diện cho một template tour có thể được sử dụng để tạo ra các tour slots cụ thể
    /// Template định nghĩa cấu trúc và thông tin cơ bản của tour, từ đó có thể tạo ra nhiều tour slots với ngày giờ khác nhau
    /// </summary>
    public class TourTemplate : BaseEntity
    {
        /// <summary>
        /// Tiêu đề của tour template
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = null!;





        /// <summary>
        /// Loại tour template (Standard, Premium, Custom, Group, Private)
        /// </summary>
        [Required]
        public TourTemplateType TemplateType { get; set; }

        /// <summary>
        /// Các ngày trong tuần mà tour này có thể được tổ chức
        /// Hỗ trợ multiple values bằng cách sử dụng bitwise operations
        /// </summary>
        [Required]
        public ScheduleDay ScheduleDays { get; set; }

        /// <summary>
        /// Điểm khởi hành của tour
        /// </summary>
        [Required]
        [StringLength(500)]
        public string StartLocation { get; set; } = null!;

        /// <summary>
        /// Điểm kết thúc của tour
        /// </summary>
        [Required]
        [StringLength(500)]
        public string EndLocation { get; set; } = null!;

        /// <summary>
        /// Tháng áp dụng template (1-12)
        /// </summary>
        [Required]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Month { get; set; }

        /// <summary>
        /// Năm áp dụng template
        /// </summary>
        [Required]
        [Range(2024, 2030, ErrorMessage = "Năm phải từ 2024 đến 2030")]
        public int Year { get; set; }

        // Navigation Properties

        /// <summary>
        /// User đã tạo tour template này
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// User đã cập nhật tour template lần cuối (nullable)
        /// </summary>
        public virtual User? UpdatedBy { get; set; }

        // Collection Navigation Properties

        /// <summary>
        /// Danh sách hình ảnh của tour template
        /// </summary>
        public virtual ICollection<Image> Images { get; set; } = new List<Image>();

        /// <summary>
        /// Danh sách các tour slots được tạo từ template này
        /// </summary>
        public virtual ICollection<TourSlot> TourSlots { get; set; } = new List<TourSlot>();

        /// <summary>
        /// Danh sách chi tiết timeline của tour template
        /// </summary>
        public virtual ICollection<TourDetails> TourDetails { get; set; } = new List<TourDetails>();
    }
}
