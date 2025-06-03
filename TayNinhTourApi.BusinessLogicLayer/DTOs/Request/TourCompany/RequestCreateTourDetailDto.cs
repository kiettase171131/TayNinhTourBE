using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request tạo mới tour detail (timeline item)
    /// </summary>
    public class RequestCreateTourDetailDto
    {
        /// <summary>
        /// ID của tour template mà chi tiết này thuộc về
        /// </summary>
        [Required(ErrorMessage = "TourTemplateId là bắt buộc")]
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Thời gian trong ngày cho hoạt động này (giờ:phút)
        /// Ví dụ: 08:30, 14:00, 16:45
        /// </summary>
        [Required(ErrorMessage = "TimeSlot là bắt buộc")]
        public TimeOnly TimeSlot { get; set; }

        /// <summary>
        /// Địa điểm hoặc tên hoạt động
        /// Ví dụ: "Núi Bà Đen", "Chùa Cao Đài", "Nhà hàng ABC"
        /// </summary>
        [StringLength(500, ErrorMessage = "Location không được vượt quá 500 ký tự")]
        public string? Location { get; set; }

        /// <summary>
        /// Mô tả chi tiết về hoạt động tại điểm dừng này
        /// Ví dụ: "Tham quan và chụp ảnh tại đỉnh núi", "Dùng bữa trưa đặc sản địa phương"
        /// </summary>
        [StringLength(1000, ErrorMessage = "Description không được vượt quá 1000 ký tự")]
        public string? Description { get; set; }

        /// <summary>
        /// ID của shop liên quan (nếu có)
        /// Nullable - chỉ có giá trị khi hoạt động liên quan đến một shop cụ thể
        /// </summary>
        public Guid? ShopId { get; set; }
    }
}
