using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// Response DTO cho việc tạo tour detail
    /// Bao gồm thông tin về clone logic cho TourSlots
    /// </summary>
    public class ResponseCreateTourDetailDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin tour detail vừa tạo
        /// </summary>
        public TourDetailDto? Data { get; set; }

        /// <summary>
        /// Số lượng TourSlots đã được clone từ template
        /// </summary>
        public int ClonedSlotsCount { get; set; }

        /// <summary>
        /// Thông tin bổ sung về clone process
        /// </summary>
        public string? CloneInfo { get; set; }
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
        /// ID của tour detail đã xóa
        /// </summary>
        public Guid DeletedTourDetailId { get; set; }

        /// <summary>
        /// Số lượng TourSlots đã được cleanup
        /// </summary>
        public int CleanedSlotsCount { get; set; }

        /// <summary>
        /// Số lượng TimelineItems đã được xóa
        /// </summary>
        public int CleanedTimelineItemsCount { get; set; }

        /// <summary>
        /// Thông tin bổ sung về cleanup process
        /// </summary>
        public string? CleanupInfo { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc lấy danh sách shops có sẵn
    /// </summary>
    public class ResponseGetAvailableShopsDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách SpecialtyShops có sẵn
        /// </summary>
        public List<SpecialtyShopResponseDto> Data { get; set; } = new List<SpecialtyShopResponseDto>();

        /// <summary>
        /// Tổng số shops
        /// </summary>
        public int TotalCount { get; set; }
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
        /// Danh sách SpecialtyShops được sử dụng
        /// </summary>
        public List<SpecialtyShopResponseDto> UsedShops { get; set; } = new List<SpecialtyShopResponseDto>();
    }
}
