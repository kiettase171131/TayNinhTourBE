using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request
{
    /// <summary>
    /// DTO for rejecting TourGuide application
    /// </summary>
    public class RejectTourGuideApplicationDto
    {
        /// <summary>
        /// Lý do từ chối đơn đăng ký
        /// </summary>
        [Required(ErrorMessage = "Lý do từ chối là bắt buộc")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Lý do từ chối phải từ 10-500 ký tự")]
        public string Reason { get; set; } = null!;
    }
}
