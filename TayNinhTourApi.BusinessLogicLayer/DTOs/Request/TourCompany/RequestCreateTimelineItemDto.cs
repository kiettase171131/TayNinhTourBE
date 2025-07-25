using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request tạo mới timeline item
    /// </summary>
    public class RequestCreateTimelineItemDto
    {
        /// <summary>
        /// ID của tour details mà timeline item này thuộc về
        /// </summary>
        [Required(ErrorMessage = "TourDetailsId là bắt buộc")]
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// Thời gian check-in cho hoạt động này (giờ:phút)
        /// Ví dụ: 05:00, 07:00, 09:00, 10:00
        /// </summary>
        [Required(ErrorMessage = "CheckInTime là bắt buộc")]
        public string CheckInTime { get; set; } = string.Empty;

        /// <summary>
        /// Tên hoạt động hoặc mô tả ngắn
        /// Ví dụ: "Khởi hành", "Ăn sáng", "Ghé shop bánh tráng", "Tới Núi Bà"
        /// </summary>
        [Required(ErrorMessage = "Activity là bắt buộc")]
        [StringLength(255, ErrorMessage = "Activity không được vượt quá 255 ký tự")]
        public string Activity { get; set; } = string.Empty;

        /// <summary>
        /// ID của specialty shop liên quan (nếu có)
        /// Nullable - chỉ có giá trị khi hoạt động liên quan đến một specialty shop cụ thể
        /// </summary>
        public Guid? SpecialtyShopId { get; set; }

        /// <summary>
        /// Thứ tự sắp xếp trong timeline (tùy chọn, sẽ tự động assign nếu không có)
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "SortOrder phải lớn hơn 0")]
        public int? SortOrder { get; set; }
    }
}
