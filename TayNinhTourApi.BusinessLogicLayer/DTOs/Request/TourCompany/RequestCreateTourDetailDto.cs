using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request tạo mới tour detail (lịch trình template)
    /// </summary>
    public class RequestCreateTourDetailDto
    {
        /// <summary>
        /// ID của tour template mà lịch trình này thuộc về
        /// </summary>
        [Required(ErrorMessage = "TourTemplateId là bắt buộc")]
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Tiêu đề của lịch trình
        /// Ví dụ: "Lịch trình VIP", "Lịch trình thường", "Lịch trình tiết kiệm"
        /// </summary>
        [Required(ErrorMessage = "Title là bắt buộc")]
        [StringLength(255, ErrorMessage = "Title không được vượt quá 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả về lịch trình này
        /// Ví dụ: "Lịch trình cao cấp với các dịch vụ VIP"
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }
    }
}
