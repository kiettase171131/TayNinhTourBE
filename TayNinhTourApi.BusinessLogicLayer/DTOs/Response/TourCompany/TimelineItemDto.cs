using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response timeline item với thông tin shop
    /// </summary>
    public class TimelineItemDto
    {
        /// <summary>
        /// ID của timeline item
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của tour details mà timeline item này thuộc về
        /// </summary>
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// Thời gian check-in cho hoạt động này (giờ:phút)
        /// Ví dụ: 05:00, 07:00, 09:00, 10:00
        /// </summary>
        public string CheckInTime { get; set; } = string.Empty;

        /// <summary>
        /// Tên hoạt động hoặc mô tả ngắn
        /// Ví dụ: "Khởi hành", "Ăn sáng", "Ghé shop bánh tráng", "Tới Núi Bà"
        /// </summary>
        public string Activity { get; set; } = string.Empty;

        /// <summary>
        /// ID của SpecialtyShop liên quan (nếu có)
        /// </summary>
        public Guid? SpecialtyShopId { get; set; }

        /// <summary>
        /// Thứ tự sắp xếp trong timeline (bắt đầu từ 1)
        /// Dùng để sắp xếp các hoạt động theo đúng trình tự thời gian
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Thông tin SpecialtyShop liên quan (nếu có)
        /// </summary>
        public SpecialtyShopResponseDto? SpecialtyShop { get; set; }

        /// <summary>
        /// Thời gian tạo timeline item
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian cập nhật timeline item lần cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
