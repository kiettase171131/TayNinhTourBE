using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// Request DTO cho việc lấy timeline với filter
    /// </summary>
    public class RequestGetTimelineDto
    {
        /// <summary>
        /// ID của tour template
        /// </summary>
        [Required(ErrorMessage = "TourTemplateId là bắt buộc")]
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Có bao gồm thông tin shop không
        /// </summary>
        public bool IncludeShopInfo { get; set; } = true;

        /// <summary>
        /// Có bao gồm các item không active không
        /// </summary>
        public bool IncludeInactive { get; set; } = false;
    }

    /// <summary>
    /// Request DTO cho việc sắp xếp lại timeline
    /// </summary>
    public class RequestReorderTimelineDto
    {
        /// <summary>
        /// ID của tour template
        /// </summary>
        [Required(ErrorMessage = "TourTemplateId là bắt buộc")]
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Danh sách ID của tour details theo thứ tự mới
        /// </summary>
        [Required(ErrorMessage = "OrderedDetailIds là bắt buộc")]
        [MinLength(1, ErrorMessage = "OrderedDetailIds phải có ít nhất 1 phần tử")]
        public List<Guid> OrderedDetailIds { get; set; } = new List<Guid>();
    }
}
