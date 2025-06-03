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


}
