using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request cập nhật shop
    /// Tất cả fields đều optional để cho phép partial update
    /// </summary>
    public class RequestUpdateShopDto
    {
        /// <summary>
        /// Tên của shop
        /// </summary>
        [StringLength(200, ErrorMessage = "Tên shop không được vượt quá 200 ký tự")]
        public string? Name { get; set; }

        /// <summary>
        /// Mô tả chi tiết về shop
        /// </summary>
        [StringLength(1000, ErrorMessage = "Mô tả không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        /// <summary>
        /// Địa điểm/vị trí của shop
        /// </summary>
        [StringLength(500, ErrorMessage = "Địa điểm không được vượt quá 500 ký tự")]
        public string? Location { get; set; }

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
        /// Đánh giá trung bình của shop (1-5 sao)
        /// </summary>
        [Range(0, 5, ErrorMessage = "Đánh giá phải từ 0 đến 5")]
        public decimal? Rating { get; set; }

        /// <summary>
        /// Ghi chú thêm về shop
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của shop
        /// </summary>
        public bool? IsActive { get; set; }
    }
}
