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
        /// Mô tả chi tiết về tour template
        /// </summary>
        [StringLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// Giá cơ bản của tour (có thể được điều chỉnh cho từng slot cụ thể)
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Giá tour phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        /// <summary>
        /// Số lượng khách tối đa cho mỗi tour slot được tạo từ template này
        /// </summary>
        [Required]
        [Range(1, 1000, ErrorMessage = "Số lượng khách phải từ 1 đến 1000")]
        public int MaxGuests { get; set; }

        /// <summary>
        /// Thời lượng tour tính bằng giờ
        /// </summary>
        [Required]
        [Range(0.5, 720, ErrorMessage = "Thời lượng tour phải từ 0.5 giờ đến 720 giờ (30 ngày)")]
        public decimal Duration { get; set; }

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
        /// Ghi chú đặc biệt hoặc yêu cầu cho tour
        /// </summary>
        [StringLength(1000)]
        public string? SpecialRequirements { get; set; }

        /// <summary>
        /// Số lượng tối thiểu khách để tour có thể được tổ chức
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng khách tối thiểu phải lớn hơn 0")]
        public int MinGuests { get; set; } = 1;

        /// <summary>
        /// Giá cho trẻ em (nếu khác với giá người lớn)
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá trẻ em phải lớn hơn hoặc bằng 0")]
        public decimal? ChildPrice { get; set; }

        /// <summary>
        /// Độ tuổi tối đa được tính là trẻ em
        /// </summary>
        [Range(0, 18, ErrorMessage = "Độ tuổi trẻ em phải từ 0 đến 18")]
        public int? ChildMaxAge { get; set; }

        /// <summary>
        /// Thông tin về phương tiện di chuyển
        /// </summary>
        [StringLength(200)]
        public string? Transportation { get; set; }

        /// <summary>
        /// Thông tin về bữa ăn được bao gồm
        /// </summary>
        [StringLength(500)]
        public string? MealsIncluded { get; set; }

        /// <summary>
        /// Thông tin về chỗ ở (nếu tour qua đêm)
        /// </summary>
        [StringLength(500)]
        public string? AccommodationInfo { get; set; }

        /// <summary>
        /// Thông tin về những gì được bao gồm trong giá tour
        /// </summary>
        [StringLength(1000)]
        public string? IncludedServices { get; set; }

        /// <summary>
        /// Thông tin về những gì không được bao gồm trong giá tour
        /// </summary>
        [StringLength(1000)]
        public string? ExcludedServices { get; set; }

        /// <summary>
        /// Chính sách hủy tour
        /// </summary>
        [StringLength(1000)]
        public string? CancellationPolicy { get; set; }

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
