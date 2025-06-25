using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Bảng trung gian cho relationship many-to-many giữa TourDetails và SpecialtyShop
    /// Lưu thông tin về các SpecialtyShop được mời tham gia vào TourDetails
    /// </summary>
    public class TourDetailsSpecialtyShop : BaseEntity
    {
        /// <summary>
        /// ID của TourDetails
        /// </summary>
        [Required]
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// ID của SpecialtyShop được mời
        /// </summary>
        [Required]
        public Guid SpecialtyShopId { get; set; }

        /// <summary>
        /// Thời gian được mời tham gia tour
        /// </summary>
        [Required]
        public DateTime InvitedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Trạng thái phản hồi của shop
        /// </summary>
        [Required]
        public ShopInvitationStatus Status { get; set; } = ShopInvitationStatus.Pending;

        /// <summary>
        /// Thời gian shop phản hồi (accept/decline)
        /// </summary>
        public DateTime? RespondedAt { get; set; }

        /// <summary>
        /// Ghi chú từ shop khi phản hồi
        /// </summary>
        [StringLength(500)]
        public string? ResponseNote { get; set; }

        /// <summary>
        /// Thời gian hết hạn invitation (mặc định 7 ngày)
        /// </summary>
        [Required]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

        /// <summary>
        /// Ưu tiên hiển thị trong timeline (1 = cao nhất)
        /// </summary>
        public int? Priority { get; set; }

        // Navigation Properties

        /// <summary>
        /// TourDetails mà invitation này thuộc về
        /// </summary>
        public virtual TourDetails TourDetails { get; set; } = null!;

        /// <summary>
        /// SpecialtyShop được mời
        /// </summary>
        public virtual SpecialtyShop SpecialtyShop { get; set; } = null!;

        /// <summary>
        /// User đã tạo invitation này (thường là Tour Company)
        /// </summary>
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// User đã cập nhật invitation này (nếu có)
        /// </summary>
        public virtual User? UpdatedBy { get; set; }
    }
}
