using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    /// <summary>
    /// Response DTO cho việc tạo timeline item
    /// </summary>
    public class ResponseCreateTimelineItemDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin timeline item vừa tạo
        /// </summary>
        public TimelineItemDto? Data { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc cập nhật timeline item
    /// </summary>
    public class ResponseUpdateTimelineItemDto : BaseResposeDto
    {
        /// <summary>
        /// Thông tin timeline item sau khi cập nhật
        /// </summary>
        public TimelineItemDto? Data { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc xóa timeline item
    /// </summary>
    public class ResponseDeleteTimelineItemDto : BaseResposeDto
    {
        /// <summary>
        /// Kết quả xóa thành công hay không
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// ID của timeline item đã xóa
        /// </summary>
        public Guid DeletedId { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc lấy timeline items của một tour details
    /// </summary>
    public class ResponseGetTimelineItemsDto : BaseResposeDto
    {
        /// <summary>
        /// Danh sách timeline items
        /// </summary>
        public List<TimelineItemDto> Data { get; set; } = new List<TimelineItemDto>();

        /// <summary>
        /// Tổng số timeline items
        /// </summary>
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Response DTO cho việc sắp xếp lại timeline items
    /// </summary>
    public class ResponseReorderTimelineItemsDto : BaseResposeDto
    {
        /// <summary>
        /// Timeline items sau khi sắp xếp lại
        /// </summary>
        public List<TimelineItemDto> Data { get; set; } = new List<TimelineItemDto>();

        /// <summary>
        /// Số lượng items đã được reorder
        /// </summary>
        public int ReorderedCount { get; set; }
    }
}
