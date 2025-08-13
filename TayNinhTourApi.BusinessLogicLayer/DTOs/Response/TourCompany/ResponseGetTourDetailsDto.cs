using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// Response DTO cho việc lấy danh sách tour details
    /// </summary>
    public class ResponseGetTourDetailsDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách tour details
        /// </summary>
        public List<TourDetailDto> Data { get; set; } = new List<TourDetailDto>();

        /// <summary>
        /// Tổng số tour details
        /// </summary>
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc lấy một tour detail
    /// </summary>
    public class ResponseGetTourDetailDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin tour detail
        /// </summary>
        public TourDetailDto? Data { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc tìm kiếm tour details
    /// </summary>
    public class ResponseSearchTourDetailsDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách tour details tìm được
        /// </summary>
        public List<TourDetailDto> Data { get; set; } = new List<TourDetailDto>();

        /// <summary>
        /// Tổng số kết quả
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Từ khóa tìm kiếm
        /// </summary>
        public string SearchKeyword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO cho việc lấy tour details với pagination
    /// </summary>
    public class ResponseGetTourDetailsPaginatedDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách enriched tour details với thông tin đầy đủ (như UserTourSearch)
        /// </summary>
        public List<EnrichedTourDetailDto> Data { get; set; } = new List<EnrichedTourDetailDto>();

        /// <summary>
        /// Tổng số records
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Trang hiện tại
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Số items per page
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Tổng số trang
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Có trang tiếp theo không
        /// </summary>
        public bool HasNextPage { get; set; }

        /// <summary>
        /// Có trang trước đó không
        /// </summary>
        public bool HasPreviousPage { get; set; }
    }

    /// <summary>
    /// DTO cho tour detail với thông tin đầy đủ (tương tự TourSearchResultDto trong UserTourSearch)
    /// </summary>
    public class EnrichedTourDetailDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? SkillsRequired { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Thông tin TourTemplate
        /// </summary>
        public TourTemplateBasicDto TourTemplate { get; set; } = new TourTemplateBasicDto();
        
        /// <summary>
        /// Thông tin TourOperation (nếu có)
        /// </summary>
        public TourOperationBasicDto? TourOperation { get; set; }
        
        /// <summary>
        /// Các slots có sẵn
        /// </summary>
        public List<AvailableSlotDto> AvailableSlots { get; set; } = new List<AvailableSlotDto>();
    }

    /// <summary>
    /// DTO cho thông tin cơ bản TourTemplate
    /// </summary>
    public class TourTemplateBasicDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TemplateType { get; set; } = string.Empty;
        public string ScheduleDays { get; set; } = string.Empty;
        public string ScheduleDaysVietnamese { get; set; } = string.Empty;
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public List<ImageDto> Images { get; set; } = new List<ImageDto>();
        public CreatedByDto CreatedBy { get; set; } = new CreatedByDto();
    }

    /// <summary>
    /// DTO cho thông tin cơ bản TourOperation
    /// </summary>
    public class TourOperationBasicDto
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public int MaxGuests { get; set; }
        public string? Description { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public int CurrentBookings { get; set; }
    }

    /// <summary>
    /// DTO cho slot có sẵn
    /// </summary>
    public class AvailableSlotDto
    {
        public Guid Id { get; set; }
        public DateOnly TourDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int MaxGuests { get; set; }
        public int CurrentBookings { get; set; }
        public int AvailableSpots { get; set; }
    }

    /// <summary>
    /// DTO cho thông tin người tạo
    /// </summary>
    public class CreatedByDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho hình ảnh
    /// </summary>
    public class ImageDto
    {
        public Guid Id { get; set; }
        public string Url { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO cho việc tạo timeline items (bulk)
    /// </summary>
    public class ResponseCreateTimelineItemsDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách timeline items đã tạo
        /// </summary>
        public List<TimelineItemDto> Data { get; set; } = new List<TimelineItemDto>();

        /// <summary>
        /// Số lượng items tạo thành công
        /// </summary>
        public int CreatedCount { get; set; }

        /// <summary>
        /// Số lượng items tạo thất bại
        /// </summary>
        public int FailedCount { get; set; }

        /// <summary>
        /// Danh sách lỗi (nếu có)
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
    }

    /// <summary>
    /// Response DTO cho việc sắp xếp lại timeline
    /// </summary>
    public class ResponseReorderTimelineDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách timeline items sau khi reorder
        /// </summary>
        public List<TimelineItemDto> Data { get; set; } = new List<TimelineItemDto>();

        /// <summary>
        /// Số lượng items được sắp xếp lại
        /// </summary>
        public int ReorderedCount { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc validate timeline
    /// </summary>
    public class ResponseValidateTimelineDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách lỗi validation (nếu có)
        /// </summary>
        public new List<string> ValidationErrors { get; set; } = new List<string>();

        /// <summary>
        /// Số lượng timeline items được kiểm tra
        /// </summary>
        public int TotalItemsChecked { get; set; }

        /// <summary>
        /// Số lượng lỗi tìm thấy
        /// </summary>
        public int ErrorCount { get; set; }

        /// <summary>
        /// Tổng số lỗi
        /// </summary>
        public int TotalErrors { get; set; }
    }

    /// <summary>
    /// Response DTO cho thống kê timeline
    /// </summary>
    public class ResponseTimelineStatisticsDto : BaseResposeDto
    {
        /// <summary>
        /// Tổng số TourDetails
        /// </summary>
        public int TotalTourDetails { get; set; }

        /// <summary>
        /// Tổng số timeline items
        /// </summary>
        public int TotalTimelineItems { get; set; }

        /// <summary>
        /// Số timeline items trung bình mỗi TourDetail
        /// </summary>
        public double AverageItemsPerDetail { get; set; }

        /// <summary>
        /// TourDetail có nhiều timeline items nhất
        /// </summary>
        public string? MostDetailedTour { get; set; }

        /// <summary>
        /// Số items của tour detailed nhất
        /// </summary>
        public int MaxItemsCount { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc lấy timeline
    /// </summary>
    public class ResponseGetTimelineDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin timeline
        /// </summary>
        public TimelineDto? Data { get; set; }
    }
}
