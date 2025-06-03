using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request tạo shop mới
    /// </summary>
    public class RequestCreateShopDto
    {
        /// <summary>
        /// Tên của shop
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập tên shop")]
        [StringLength(200, ErrorMessage = "Tên shop không được vượt quá 200 ký tự")]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mô tả chi tiết về shop
        /// </summary>
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        /// <summary>
        /// Địa điểm/vị trí của shop
        /// </summary>
        [Required(ErrorMessage = "Vui lòng nhập địa điểm shop")]
        [StringLength(500, ErrorMessage = "Địa điểm không được vượt quá 500 ký tự")]
        public string Location { get; set; } = null!;

        /// <summary>
        /// Số điện thoại liên hệ của shop
        /// </summary>
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        [RegularExpression(@"^[0-9+\-\s\(\)]*$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Địa chỉ email liên hệ của shop
        /// </summary>
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
        [EmailAddress(ErrorMessage = "Vui lòng nhập đúng định dạng email")]
        public string? Email { get; set; }

        /// <summary>
        /// Website của shop
        /// </summary>
        [StringLength(200, ErrorMessage = "Website không được vượt quá 200 ký tự")]
        [Url(ErrorMessage = "Vui lòng nhập đúng định dạng URL")]
        public string? Website { get; set; }

        /// <summary>
        /// Giờ mở cửa của shop
        /// </summary>
        [StringLength(100, ErrorMessage = "Giờ mở cửa không được vượt quá 100 ký tự")]
        public string? OpeningHours { get; set; }

        /// <summary>
        /// Loại shop (ví dụ: Souvenir, Food, Craft, etc.)
        /// </summary>
        [StringLength(50, ErrorMessage = "Loại shop không được vượt quá 50 ký tự")]
        public string? ShopType { get; set; }

        /// <summary>
        /// Ghi chú thêm về shop
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }
}
