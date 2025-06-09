using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request cập nhật timeline item
    /// Tất cả fields đều optional để cho phép partial update
    /// </summary>
    public class RequestUpdateTimelineItemDto
    {
        /// <summary>
        /// Thời gian check-in cho hoạt động này (giờ:phút)
        /// Ví dụ: 05:00, 07:00, 09:00, 10:00
        /// </summary>
        public string? CheckInTime { get; set; }

        /// <summary>
        /// Tên hoạt động hoặc mô tả ngắn
        /// Ví dụ: "Khởi hành", "Ăn sáng", "Ghé shop bánh tráng", "Tới Núi Bà"
        /// </summary>
        [StringLength(255, ErrorMessage = "Activity không được vượt quá 255 ký tự")]
        public string? Activity { get; set; }

        /// <summary>
        /// ID của shop liên quan (nếu có)
        /// Nullable - chỉ có giá trị khi hoạt động liên quan đến một shop cụ thể
        /// </summary>
        public Guid? ShopId { get; set; }

        /// <summary>
        /// Thứ tự sắp xếp trong timeline
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "SortOrder phải lớn hơn 0")]
        public int? SortOrder { get; set; }
    }
}
