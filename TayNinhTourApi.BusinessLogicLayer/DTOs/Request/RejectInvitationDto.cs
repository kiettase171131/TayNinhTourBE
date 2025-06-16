using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request
{
    /// <summary>
    /// DTO cho request từ chối lời mời tour guide
    /// Sử dụng bởi TourGuide để reject invitation
    /// </summary>
    public class RejectInvitationDto
    {
        /// <summary>
        /// ID của invitation được từ chối
        /// </summary>
        [Required(ErrorMessage = "InvitationId là bắt buộc")]
        public Guid InvitationId { get; set; }

        /// <summary>
        /// Lý do từ chối lời mời (bắt buộc)
        /// Ví dụ: "Tôi đã có lịch trình khác", "Không phù hợp với kỹ năng của tôi"
        /// </summary>
        [Required(ErrorMessage = "Lý do từ chối là bắt buộc")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Lý do từ chối phải từ 10 đến 500 ký tự")]
        public string RejectionReason { get; set; } = string.Empty;

        /// <summary>
        /// Gợi ý cải thiện cho lần mời tiếp theo (tùy chọn)
        /// Ví dụ: "Có thể điều chỉnh thời gian tour", "Cần thêm thông tin về địa điểm"
        /// </summary>
        [StringLength(300, ErrorMessage = "Gợi ý không được vượt quá 300 ký tự")]
        public string? ImprovementSuggestion { get; set; }
    }
}
