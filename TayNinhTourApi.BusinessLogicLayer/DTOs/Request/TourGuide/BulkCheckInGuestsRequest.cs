using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourGuide
{
    /// <summary>
    /// Request DTO cho bulk check-in multiple guests
    /// NEW: For efficient mass check-in operations
    /// </summary>
    public class BulkCheckInGuestsRequest
    {
        /// <summary>
        /// Danh sách IDs của guests cần check-in
        /// </summary>
        [Required(ErrorMessage = "Danh sách guest IDs là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải chọn ít nhất 1 guest để check-in")]
        [MaxLength(50, ErrorMessage = "Không thể check-in quá 50 guests cùng lúc")]
        public List<Guid> GuestIds { get; set; } = new();

        /// <summary>
        /// Ghi chú chung cho tất cả guests (tùy chọn)
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        /// <summary>
        /// Thời gian check-in custom (tùy chọn)
        /// Nếu null, sử dụng thời gian hiện tại
        /// </summary>
        public DateTime? CustomCheckInTime { get; set; }

        /// <summary>
        /// Override thời gian check-in (cho phép check-in sớm)
        /// Mặc định: false - chỉ cho phép check-in trong khung thời gian hợp lệ
        /// </summary>
        public bool OverrideTimeRestriction { get; set; } = false;
    }
}
