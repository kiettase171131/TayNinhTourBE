using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request
{
    /// <summary>
    /// DTO cho request chấp nhận lời mời tour guide
    /// Sử dụng bởi TourGuide để accept invitation
    /// </summary>
    public class AcceptInvitationDto
    {
        /// <summary>
        /// ID của invitation được chấp nhận
        /// </summary>
        [Required(ErrorMessage = "InvitationId là bắt buộc")]
        public Guid InvitationId { get; set; }

        /// <summary>
        /// Ghi chú từ TourGuide khi chấp nhận (tùy chọn)
        /// Ví dụ: "Tôi rất vui được tham gia tour này"
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? AcceptanceMessage { get; set; }

        /// <summary>
        /// Xác nhận TourGuide đã đọc và hiểu yêu cầu tour
        /// </summary>
        [Required(ErrorMessage = "Cần xác nhận đã đọc yêu cầu tour")]
        public bool ConfirmUnderstanding { get; set; }
    }
}
