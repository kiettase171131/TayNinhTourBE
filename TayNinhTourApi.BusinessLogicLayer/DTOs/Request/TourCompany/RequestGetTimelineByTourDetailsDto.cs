using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request lấy timeline của một TourDetails cụ thể
    /// </summary>
    public class RequestGetTimelineByTourDetailsDto
    {
        /// <summary>
        /// ID của TourDetails cần lấy timeline
        /// </summary>
        [Required(ErrorMessage = "TourDetailsId là bắt buộc")]
        public Guid TourDetailsId { get; set; }

        /// <summary>
        /// Có bao gồm các timeline items không active không
        /// Default: false
        /// </summary>
        public bool IncludeInactive { get; set; } = false;

        /// <summary>
        /// Có bao gồm thông tin shop không
        /// Default: true
        /// </summary>
        public bool IncludeShopInfo { get; set; } = true;
    }
} 