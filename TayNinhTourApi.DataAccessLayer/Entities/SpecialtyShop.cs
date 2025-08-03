using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đại diện cho thông tin mở rộng của một user có role "Specialty Shop"
    /// Relationship 1:1 với User entity
    /// </summary>
    public class SpecialtyShop : BaseEntity
    {
        /// <summary>
        /// Foreign Key đến User (1:1 relationship)
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Tên của shop
        /// </summary>
        [Required]
        [StringLength(200)]
        public string ShopName { get; set; } = null!;

        /// <summary>
        /// Số tiền trong ví của shop từ việc bán sản phẩm
        /// Được cộng vào khi có đơn hàng thanh toán thành công
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Wallet { get; set; } = 0;

        /// <summary>
        /// Mô tả chi tiết về shop
        /// </summary>
        [StringLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Địa điểm/vị trí của shop
        /// </summary>
        [Required]
        [StringLength(500)]
        public string Location { get; set; } = null!;

        /// <summary>
        /// Tên người đại diện shop
        /// </summary>
        [Required]
        [StringLength(100)]
        public string RepresentativeName { get; set; } = null!;

        /// <summary>
        /// Email liên hệ của shop (có thể khác với User.Email)
        /// </summary>
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Số điện thoại liên hệ của shop
        /// </summary>
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Địa chỉ của shop
        /// </summary>
        [StringLength(500)]
        public string? Address { get; set; }

        /// <summary>
        /// Website của shop
        /// </summary>
        [StringLength(200)]
        [Url]
        public string? Website { get; set; }

        /// <summary>
        /// Số giấy phép kinh doanh
        /// </summary>
        [StringLength(100)]
        public string? BusinessLicense { get; set; }

        /// <summary>
        /// URL đến file giấy phép kinh doanh
        /// </summary>
        [StringLength(500)]
        public string? BusinessLicenseUrl { get; set; }

        /// <summary>
        /// URL đến logo của shop
        /// </summary>
        [StringLength(500)]
        public string? LogoUrl { get; set; }

        /// <summary>
        /// Loại shop (ví dụ: Souvenir, Food, Craft, etc.)
        /// </summary>
        [StringLength(50)]
        public string? ShopType { get; set; }

        /// <summary>
        /// Giờ mở cửa của shop (HH:mm format, ví dụ: "08:00")
        /// </summary>
        [StringLength(10)]
        public string? OpeningHours { get; set; }

        /// <summary>
        /// Giờ đóng cửa của shop (HH:mm format, ví dụ: "18:00")
        /// </summary>          
        [StringLength(10)]
        public string? ClosingHours { get; set; }

        /// <summary>
        /// Đánh giá trung bình của shop (1-5 sao)
        /// </summary>
        [Range(0, 5)]
        public decimal? Rating { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của shop
        /// Khác với BaseEntity.IsActive (dùng cho soft delete)
        /// - true: Shop đang hoạt động và có thể nhận tour
        /// - false: Shop tạm thời đóng cửa
        /// </summary>
        public bool IsShopActive { get; set; } = true;

        /// <summary>
        /// Ghi chú thêm về shop (từ Shop entity)
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation Properties

        /// <summary>
        /// User sở hữu shop này (1:1 relationship)
        /// </summary>  
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Danh sách các timeline items có ghé thăm shop này
        /// Thay thế cho Shop.TourDetails relationship
        /// </summary>
        public virtual ICollection<TimelineItem> TimelineItems { get; set; } = new List<TimelineItem>();

        /// <summary>
        /// Danh sách các tour invitations mà shop này nhận được
        /// </summary>
        public virtual ICollection<TourDetailsSpecialtyShop> TourInvitations { get; set; } = new List<TourDetailsSpecialtyShop>();
    }
}
