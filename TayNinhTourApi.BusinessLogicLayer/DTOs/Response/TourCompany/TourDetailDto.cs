namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response tour detail (timeline item) với thông tin shop
    /// </summary>
    public class TourDetailDto
    {
        /// <summary>
        /// ID của tour detail
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của tour template mà chi tiết này thuộc về
        /// </summary>
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Thời gian trong ngày cho hoạt động này (giờ:phút)
        /// Ví dụ: 08:30, 14:00, 16:45
        /// </summary>
        public TimeOnly TimeSlot { get; set; }

        /// <summary>
        /// Địa điểm hoặc tên hoạt động
        /// Ví dụ: "Núi Bà Đen", "Chùa Cao Đài", "Nhà hàng ABC"
        /// </summary>
        public string? Location { get; set; }

        /// <summary>
        /// Mô tả chi tiết về hoạt động tại điểm dừng này
        /// Ví dụ: "Tham quan và chụp ảnh tại đỉnh núi", "Dùng bữa trưa đặc sản địa phương"
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// ID của shop liên quan (nếu có)
        /// </summary>
        public Guid? ShopId { get; set; }

        /// <summary>
        /// Thứ tự sắp xếp trong timeline (bắt đầu từ 1)
        /// Dùng để sắp xếp các hoạt động theo đúng trình tự thời gian
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Thông tin shop liên quan (nếu có)
        /// </summary>
        public ShopDto? Shop { get; set; }

        /// <summary>
        /// Thời gian tạo tour detail
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian cập nhật tour detail lần cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
