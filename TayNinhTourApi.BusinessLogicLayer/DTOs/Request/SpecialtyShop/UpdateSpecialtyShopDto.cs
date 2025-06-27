using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.BusinessLogicLayer.Attributes;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop
{
    /// <summary>
    /// DTO cho việc cập nhật thông tin SpecialtyShop
    /// Chỉ chứa các fields có thể được cập nhật bởi shop owner
    /// </summary>
    public class UpdateSpecialtyShopDto
    {
        /// <summary>
        /// Tên shop (có thể cập nhật)
        /// </summary>
        [StringLength(200, ErrorMessage = "Shop name cannot exceed 200 characters")]
        public string? ShopName { get; set; }

        /// <summary>
        /// Mô tả shop
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string? Description { get; set; }

        /// <summary>
        /// Địa điểm shop (có thể cập nhật)
        /// </summary>
        [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
        public string? Location { get; set; }

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Website của shop
        /// </summary>
        [StringLength(200, ErrorMessage = "Website URL cannot exceed 200 characters")]
        [Url(ErrorMessage = "Invalid website URL format")]
        public string? Website { get; set; }

        /// <summary>
        /// Loại shop
        /// </summary>
        [StringLength(50, ErrorMessage = "Shop type cannot exceed 50 characters")]
        public string? ShopType { get; set; }

        /// <summary>
        /// Giờ mở cửa (HH:mm format)
        /// </summary>
        [StringLength(10, ErrorMessage = "Opening hours cannot exceed 10 characters")]
        [TimeFormatValidation]
        public string? OpeningHours { get; set; }

        /// <summary>
        /// Giờ đóng cửa (HH:mm format)
        /// </summary>
        [StringLength(10, ErrorMessage = "Closing hours cannot exceed 10 characters")]
        [TimeFormatValidation]
        public string? ClosingHours { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của shop
        /// </summary>
        public bool? IsShopActive { get; set; }
    }
}
