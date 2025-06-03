using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho request cập nhật tour slot
    /// </summary>
    public class RequestUpdateSlotDto
    {
        /// <summary>
        /// ID của tour slot cần cập nhật
        /// </summary>
        [Required(ErrorMessage = "Id là bắt buộc")]
        public Guid Id { get; set; }

        /// <summary>
        /// Trạng thái mới của tour slot
        /// </summary>
        public TourSlotStatus? Status { get; set; }

        /// <summary>
        /// Trạng thái hoạt động của slot
        /// - true: Slot có thể được booking
        /// - false: Slot tạm thời không available
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Ghi chú về việc cập nhật slot
        /// Ví dụ: "Hủy do thời tiết xấu", "Tạm ngưng do bảo trì"
        /// </summary>
        [StringLength(500, ErrorMessage = "Notes không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }
    }
}
