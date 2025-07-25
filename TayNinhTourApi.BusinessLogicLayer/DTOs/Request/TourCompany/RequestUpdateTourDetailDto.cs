using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request cập nhật tour detail (lịch trình template)
    /// Tất cả fields đều optional để cho phép partial update
    /// </summary>
    public class RequestUpdateTourDetailDto
    {
        /// <summary>
        /// Tiêu đề của lịch trình
        /// Ví dụ: "Lịch trình VIP", "Lịch trình thường", "Lịch trình tiết kiệm"
        /// </summary>
        [StringLength(255, ErrorMessage = "Title không được vượt quá 255 ký tự")]
        public string? Title { get; set; }

        /// <summary>
        /// Mô tả về lịch trình này
        /// Ví dụ: "Lịch trình cao cấp với các dịch vụ VIP"
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        /// <summary>
        /// Danh sách URL hình ảnh cho tour details này
        /// </summary>
        public List<string>? ImageUrls { get; set; }

        /// <summary>
        /// URL hình ảnh đại diện cho tour details này (backward compatibility)
        /// Sẽ được chuyển đổi thành ImageUrls[0] nếu có
        /// </summary>
        [StringLength(500, ErrorMessage = "ImageUrl không được vượt quá 500 ký tự")]
        public string? ImageUrl { get; set; }
    }
}
