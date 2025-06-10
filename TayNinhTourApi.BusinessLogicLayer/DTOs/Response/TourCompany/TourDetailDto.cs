namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response tour detail (lịch trình template)
    /// </summary>
    public class TourDetailDto
    {
        /// <summary>
        /// ID của tour detail
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của tour template mà lịch trình này thuộc về
        /// </summary>
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Tên của tour template mà lịch trình này thuộc về
        /// </summary>
        public string TourTemplateName { get; set; } = string.Empty;

        /// <summary>
        /// Tiêu đề của lịch trình
        /// Ví dụ: "Lịch trình VIP", "Lịch trình thường", "Lịch trình tiết kiệm"
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Mô tả về lịch trình này
        /// Ví dụ: "Lịch trình cao cấp với các dịch vụ VIP"
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Danh sách timeline items thuộc về lịch trình này
        /// </summary>
        public List<TimelineItemDto> Timeline { get; set; } = new List<TimelineItemDto>();

        /// <summary>
        /// Thông tin operation cho lịch trình này (nếu có)
        /// </summary>
        public TourOperationDto? TourOperation { get; set; }

        /// <summary>
        /// Số lượng timeline items thuộc về lịch trình này
        /// </summary>
        public int TimelineItemsCount { get; set; }

        /// <summary>
        /// Số lượng slots được assign lịch trình này
        /// </summary>
        public int AssignedSlotsCount { get; set; }

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
