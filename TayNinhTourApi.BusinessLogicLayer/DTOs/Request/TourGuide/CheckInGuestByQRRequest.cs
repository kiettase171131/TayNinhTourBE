using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourGuide
{
    /// <summary>
    /// Request DTO cho check-in individual guest bằng QR code
    /// NEW: For individual guest QR system
    /// </summary>
    public class CheckInGuestByQRRequest
    {
        /// <summary>
        /// QR code data của guest cần check-in
        /// </summary>
        [Required(ErrorMessage = "QR code data là bắt buộc")]
        [StringLength(2000, ErrorMessage = "QR code data không được vượt quá 2000 ký tự")]
        public string QRCodeData { get; set; } = string.Empty;

        /// <summary>
        /// Ghi chú bổ sung khi check-in (tùy chọn)
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        /// <summary>
        /// Override thời gian check-in (cho phép check-in sớm)
        /// Mặc định: false - chỉ cho phép check-in trong khung thời gian hợp lệ
        /// </summary>
        public bool OverrideTimeRestriction { get; set; } = false;

        /// <summary>
        /// Thời gian check-in custom (nếu override = true)
        /// Nếu null, sử dụng thời gian hiện tại
        /// </summary>
        public DateTime? CustomCheckInTime { get; set; }
    }
}
