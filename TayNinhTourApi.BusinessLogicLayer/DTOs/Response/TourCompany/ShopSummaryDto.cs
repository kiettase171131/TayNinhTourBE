namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response thông tin shop tóm tắt (dùng cho dropdown, select list)
    /// </summary>
    public class ShopSummaryDto
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
        /// Địa điểm/vị trí của shop
        /// </summary>
        public string Location { get; set; } = null!;

        /// <summary>
        /// Mô tả của shop
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Số điện thoại của shop
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Loại shop (ví dụ: Souvenir, Food, Craft, etc.)
        /// </summary>
        public string? ShopType { get; set; }

        /// <summary>
        /// Đánh giá trung bình của shop (1-5 sao)
        /// </summary>
        public decimal? Rating { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của shop
        /// </summary>
        public bool IsActive { get; set; }
    }
}
