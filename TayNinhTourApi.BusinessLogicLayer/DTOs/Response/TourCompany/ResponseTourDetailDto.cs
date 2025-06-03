using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// Response DTO cho việc lấy timeline đầy đủ
    /// </summary>
    public class ResponseGetTimelineDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin timeline
        /// </summary>
        public TimelineDto? Data { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc tạo tour detail
    /// </summary>
    public class ResponseCreateTourDetailDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin tour detail vừa tạo
        /// </summary>
        public TourDetailDto? Data { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc cập nhật tour detail
    /// </summary>
    public class ResponseUpdateTourDetailDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin tour detail sau khi cập nhật
        /// </summary>
        public TourDetailDto? Data { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc xóa tour detail
    /// </summary>
    public class ResponseDeleteTourDetailDto : BaseResposeDto
    {
        /// <summary>
        /// Kết quả xóa thành công hay không
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// ID của tour detail đã xóa
        /// </summary>
        public Guid DeletedId { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc sắp xếp lại timeline
    /// </summary>
    public class ResponseReorderTimelineDto : BaseResposeDto
    {
        /// <summary>
        /// Timeline sau khi sắp xếp lại
        /// </summary>
        public TimelineDto? Data { get; set; }

        /// <summary>
        /// Số lượng items đã được reorder
        /// </summary>
        public int ReorderedCount { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc lấy danh sách shops có sẵn
    /// </summary>
    public class ResponseGetAvailableShopsDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách shops có sẵn
        /// </summary>
        public List<ShopSummaryDto> Data { get; set; } = new List<ShopSummaryDto>();

        /// <summary>
        /// Tổng số shops
        /// </summary>
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// DTO tóm tắt thông tin shop cho dropdown
    /// </summary>
    public class ShopSummaryDto
    {
        /// <summary>
        /// ID của shop
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tên của shop
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Địa điểm/vị trí của shop
        /// </summary>
        public string Location { get; set; } = null!;

        /// <summary>
        /// Mô tả ngắn về shop
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Số điện thoại liên hệ của shop
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Trạng thái active của shop
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc validate timeline
    /// </summary>
    public class ResponseValidateTimelineDto : BaseResposeDto
    {
        /// <summary>
        /// Timeline có hợp lệ không
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Danh sách lỗi validation (nếu có)
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new List<string>();

        /// <summary>
        /// Danh sách cảnh báo (nếu có)
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Response DTO cho việc lấy thống kê timeline
    /// </summary>
    public class ResponseTimelineStatisticsDto : BaseResposeDto
    {
        /// <summary>
        /// Thống kê timeline
        /// </summary>
        public TimelineStatistics? Data { get; set; }
    }

    /// <summary>
    /// DTO cho thống kê timeline
    /// </summary>
    public class TimelineStatistics
    {
        /// <summary>
        /// Tổng số timeline items
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Số items có shop
        /// </summary>
        public int ItemsWithShop { get; set; }

        /// <summary>
        /// Số items không có shop
        /// </summary>
        public int ItemsWithoutShop { get; set; }

        /// <summary>
        /// Thời gian bắt đầu sớm nhất
        /// </summary>
        public TimeOnly? EarliestTime { get; set; }

        /// <summary>
        /// Thời gian kết thúc muộn nhất
        /// </summary>
        public TimeOnly? LatestTime { get; set; }

        /// <summary>
        /// Tổng thời gian tour (giờ)
        /// </summary>
        public decimal TotalDuration { get; set; }

        /// <summary>
        /// Danh sách shops được sử dụng
        /// </summary>
        public List<ShopSummaryDto> UsedShops { get; set; } = new List<ShopSummaryDto>();
    }
}
