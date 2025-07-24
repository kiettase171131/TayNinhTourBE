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
        /// Danh sách tour details
        /// </summary>
        public List<TourDetailDto> Data { get; set; } = new List<TourDetailDto>();

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
