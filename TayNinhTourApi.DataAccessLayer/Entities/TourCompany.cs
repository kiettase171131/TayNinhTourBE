using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Thông tin công ty tour - mở rộng từ User với role "Tour Company"
    /// Quản lý ví tiền và doanh thu từ tour bookings
    /// </summary>
    public class TourCompany : BaseEntity
    {
        /// <summary>
        /// ID của User có role "Tour Company"
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Tên công ty tour
        /// </summary>
        [Required]
        [StringLength(200, ErrorMessage = "Tên công ty không quá 200 ký tự")]
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Số tiền có thể rút (đã được chuyển từ revenue hold sau 3 ngày)
        /// </summary>
        [Required]
        public decimal Wallet { get; set; } = 0;

        /// <summary>
        /// Số tiền đang hold (chưa thể rút, chờ tour hoàn thành + 3 ngày)
        /// Tiền từ tour bookings sẽ vào đây trước, sau đó chuyển sang Wallet
        /// </summary>
        [Required]
        public decimal RevenueHold { get; set; } = 0;

        /// <summary>
        /// Mô tả về công ty
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Địa chỉ công ty
        /// </summary>
        [StringLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Website công ty
        /// </summary>
        [StringLength(200)]
        public string? Website { get; set; }

        /// <summary>
        /// Số giấy phép kinh doanh
        /// </summary>
        [StringLength(50)]
        public string? BusinessLicense { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của công ty
        /// Khác với BaseEntity.IsActive (dùng cho soft delete)
        /// </summary>
        public new bool IsActive { get; set; } = true;

        // Navigation Properties

        /// <summary>
        /// User account của công ty tour
        /// Relationship: One-to-One
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Các tour templates được tạo bởi công ty này
        /// </summary>
        public virtual ICollection<TourTemplate> TourTemplates { get; set; } = new List<TourTemplate>();

        /// <summary>
        /// Các tour details được tạo bởi công ty này
        /// </summary>
        public virtual ICollection<TourDetails> TourDetails { get; set; } = new List<TourDetails>();

        /// <summary>
        /// Các tour operations được tạo bởi công ty này
        /// </summary>
        public virtual ICollection<TourOperation> TourOperations { get; set; } = new List<TourOperation>();
    }
}
