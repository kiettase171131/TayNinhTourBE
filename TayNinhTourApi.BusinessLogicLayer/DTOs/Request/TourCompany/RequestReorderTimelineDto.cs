using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request sắp xếp lại thứ tự timeline
    /// </summary>
    public class RequestReorderTimelineDto
    {
        /// <summary>
        /// ID của tour details mà timeline items thuộc về
        /// </summary>
        [Required(ErrorMessage = "TourDetailsId là bắt buộc")]
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// Danh sách ID timeline items theo thứ tự mới
        /// </summary>
        [Required(ErrorMessage = "TimelineItemIds là bắt buộc")]
        [MinLength(1, ErrorMessage = "TimelineItemIds phải có ít nhất 1 item")]
        public List<Guid> TimelineItemIds { get; set; } = new List<Guid>();
    }

    /// <summary>
    /// DTO cho request sắp xếp lại thứ tự timeline (legacy version)
    /// </summary>
    public class RequestReorderTimelineLegacyDto
    {
        /// <summary>
        /// ID của tour template
        /// </summary>
        [Required(ErrorMessage = "TourTemplateId là bắt buộc")]
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Danh sách các timeline items với thứ tự mới
        /// </summary>
        [Required(ErrorMessage = "TimelineItems là bắt buộc")]
        [MinLength(1, ErrorMessage = "TimelineItems phải có ít nhất 1 item")]
        public List<TimelineOrderItem> TimelineItems { get; set; } = new List<TimelineOrderItem>();

        /// <summary>
        /// Danh sách ID của các detail được sắp xếp lại
        /// </summary>
        [Required(ErrorMessage = "OrderedDetailIds là bắt buộc")]
        public List<Guid> OrderedDetailIds { get; set; } = new List<Guid>();
    }

    /// <summary>
    /// Đại diện cho một item trong timeline với thứ tự mới
    /// </summary>
    public class TimelineOrderItem
    {
        /// <summary>
        /// ID của tour detail
        /// </summary>
        [Required(ErrorMessage = "Id là bắt buộc")]
        public Guid Id { get; set; }

        /// <summary>
        /// Thứ tự mới trong timeline (bắt đầu từ 1)
        /// </summary>
        [Required(ErrorMessage = "SortOrder là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "SortOrder phải lớn hơn 0")]
        public int SortOrder { get; set; }
    }
}
