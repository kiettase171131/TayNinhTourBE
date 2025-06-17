namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// DTO cho response timeline đầy đủ của một tour template
    /// </summary>
    public class TimelineDto
    {
        /// <summary>
        /// ID của tour template
        /// </summary>
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Tiêu đề của tour template
        /// </summary>
        public string TourTemplateTitle { get; set; } = null!;

        /// <summary>
        /// Tổng thời lượng tour tính bằng giờ
        /// </summary>
        public decimal Duration { get; set; }

        /// <summary>
        /// Điểm khởi hành của tour
        /// </summary>
        public string StartLocation { get; set; } = null!;

        /// <summary>
        /// Điểm kết thúc của tour
        /// </summary>
        public string EndLocation { get; set; } = null!;

        /// <summary>
        /// Danh sách các timeline items được sắp xếp theo thứ tự timeline
        /// </summary>
        public List<TimelineItemDto> Items { get; set; } = new List<TimelineItemDto>();

        /// <summary>
        /// Tổng số items trong timeline
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Tổng thời lượng timeline tính bằng giờ
        /// </summary>
        public decimal TotalDuration { get; set; }

        /// <summary>
        /// Tổng số điểm dừng trong timeline (computed property)
        /// </summary>
        public int TotalStops => Items.Count;

        /// <summary>
        /// Thời gian bắt đầu sớm nhất trong timeline (computed property)
        /// </summary>
        public string? EarliestTime => Items.Any() ? Items.Min(ti => ti.CheckInTime) : null;

        /// <summary>
        /// Thời gian kết thúc muộn nhất trong timeline (computed property)
        /// </summary>
        public string? LatestTime => Items.Any() ? Items.Max(ti => ti.CheckInTime) : null;

        /// <summary>
        /// Số lượng SpecialtyShops được ghé thăm trong tour (computed property)
        /// </summary>
        public int ShopsCount => Items.Count(ti => ti.SpecialtyShopId.HasValue);

        /// <summary>
        /// Thời gian tạo timeline
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian cập nhật timeline lần cuối
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
    }
}
