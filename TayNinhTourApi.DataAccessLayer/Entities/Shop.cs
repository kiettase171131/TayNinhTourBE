using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Đại diện cho một shop/cửa hàng có thể được ghé thăm trong tour
    /// </summary>
    public class Shop : BaseEntity
    {
        /// <summary>
        /// Tên của shop
        /// </summary>
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = null!;

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
        /// Số điện thoại liên hệ của shop
        /// </summary>
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Địa chỉ email liên hệ của shop
        /// </summary>
        [StringLength(100)]
        [EmailAddress]
        public string? Email { get; set; }

        /// <summary>
        /// Website của shop
        /// </summary>
        [StringLength(200)]
        [Url]
        public string? Website { get; set; }

        /// <summary>
        /// Giờ mở cửa của shop
        /// </summary>
        [StringLength(100)]
        public string? OpeningHours { get; set; }

        /// <summary>
        /// Loại shop (ví dụ: Souvenir, Food, Craft, etc.)
        /// </summary>
        [StringLength(50)]
        public string? ShopType { get; set; }

        /// <summary>
        /// Đánh giá trung bình của shop (1-5 sao)
        /// </summary>
        [Range(0, 5)]
        public decimal? Rating { get; set; }

        /// <summary>
        /// Ghi chú thêm về shop
        /// </summary>
        [StringLength(500)]
        public string? Notes { get; set; }

        // Navigation Properties

        /// <summary>
        /// Danh sách các tour details có ghé thăm shop này
        /// </summary>
        public virtual ICollection<TourDetails> TourDetails { get; set; } = new List<TourDetails>();

        /// <summary>
        /// User đã tạo shop này
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// User đã cập nhật shop này lần cuối
        /// </summary>
        public virtual User? UpdatedBy { get; set; }
    }
}
