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
        /// Danh sách các tour details được sắp xếp theo thứ tự timeline
        /// </summary>
        public List<TourDetailDto> TourDetails { get; set; } = new List<TourDetailDto>();

        /// <summary>
        /// Tổng số điểm dừng trong timeline
        /// </summary>
        public int TotalStops => TourDetails.Count;

        /// <summary>
        /// Thời gian bắt đầu sớm nhất trong timeline
        /// </summary>
        public TimeOnly? EarliestTime => TourDetails.Any() ? TourDetails.Min(td => td.TimeSlot) : null;

        /// <summary>
        /// Thời gian kết thúc muộn nhất trong timeline
        /// </summary>
        public TimeOnly? LatestTime => TourDetails.Any() ? TourDetails.Max(td => td.TimeSlot) : null;

        /// <summary>
        /// Số lượng shops được ghé thăm trong tour
        /// </summary>
        public int ShopsCount => TourDetails.Count(td => td.ShopId.HasValue);

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
