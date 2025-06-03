namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response thông tin shop
    /// </summary>
    public class ShopDto
    {
        /// <summary>
        /// ID của shop
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên của shop
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Mô tả chi tiết về shop
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Địa điểm/vị trí của shop
        /// </summary>
        public string Location { get; set; } = null!;

        /// <summary>
        /// Số điện thoại liên hệ của shop
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Địa chỉ email liên hệ của shop
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Website của shop
        /// </summary>
        public string? Website { get; set; }

        /// <summary>
        /// Giờ mở cửa của shop
        /// </summary>
        public string? OpeningHours { get; set; }

        /// <summary>
        /// Loại shop (ví dụ: Souvenir, Food, Craft, etc.)
        /// </summary>
        public string? ShopType { get; set; }

        /// <summary>
        /// Đánh giá trung bình của shop (1-5 sao)
        /// </summary>
        public decimal? Rating { get; set; }

        /// <summary>
        /// Ghi chú thêm về shop
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Thời gian tạo shop
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian cập nhật shop lần cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
