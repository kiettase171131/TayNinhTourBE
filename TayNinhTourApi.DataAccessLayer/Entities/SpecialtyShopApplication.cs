using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đơn đăng ký trở thành Specialty Shop Owner
    /// Thay thế cho ShopApplication với đầy đủ fields theo thiết kế
    /// </summary>
    public class SpecialtyShopApplication : BaseEntity
    {
        /// <summary>
        /// Foreign Key đến User đăng ký
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Tên specialty shop
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ShopName { get; set; } = null!;

        /// <summary>
        /// Mô tả specialty shop
        /// </summary>
        [StringLength(1000)]
        public string? ShopDescription { get; set; }

        /// <summary>
        /// Số giấy phép kinh doanh
        /// </summary>
        [Required]
        [StringLength(100)]
        public string BusinessLicense { get; set; } = null!;

        /// <summary>
        /// Địa chỉ specialty shop
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Location { get; set; } = null!;

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = null!;

        /// <summary>
        /// Email liên hệ
        /// </summary>
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Website specialty shop
        /// </summary>
        [StringLength(200)]
        [Url]
        public string? Website { get; set; }

        /// <summary>
        /// Loại shop (Souvenir, Food, Craft...)
        /// </summary>
        [StringLength(50)]
        public string? ShopType { get; set; }

        /// <summary>
        /// Giờ mở cửa
        /// </summary>
        [StringLength(100)]
        public string? OpeningHours { get; set; }

        /// <summary>
        /// Tên người đại diện
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RepresentativeName { get; set; } = null!;

        /// <summary>
        /// URL đến file giấy phép kinh doanh
        /// </summary>
        [StringLength(500)]
        public string? BusinessLicenseUrl { get; set; }

        /// <summary>
        /// URL đến logo specialty shop
        /// </summary>
        [StringLength(500)]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Trạng thái đơn đăng ký
        /// </summary>
        [Required]
        public SpecialtyShopApplicationStatus Status { get; set; } = SpecialtyShopApplicationStatus.Pending;

        /// <summary>
        /// Lý do từ chối (nếu có)
        /// </summary>
        [StringLength(500)]
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Thời gian nộp đơn
        /// </summary>
        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Thời gian xử lý đơn
        /// </summary>
        public DateTime? ProcessedAt { get; set; }

        /// <summary>
        /// ID của admin xử lý đơn
        /// </summary>
        public Guid? ProcessedById { get; set; }

        // Navigation Properties

        /// <summary>
        /// User đăng ký specialty shop
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Admin xử lý đơn (nếu có)
        /// </summary>
        public virtual User? ProcessedBy { get; set; }
    }
}
