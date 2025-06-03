using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot
{
    /// <summary>
    /// DTO cho request bulk update status của nhiều slots
    /// </summary>
    public class RequestBulkUpdateSlotStatusDto
    {
        /// <summary>
        /// Danh sách IDs của slots cần update
        /// </summary>
        [Required(ErrorMessage = "Danh sách Slot IDs là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 slot để update")]
        public List<Guid> SlotIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Status mới cho tất cả slots
        /// </summary>
        public TourSlotStatus? NewStatus { get; set; }

        /// <summary>
        /// IsActive mới cho tất cả slots
        /// </summary>
        public bool? NewIsActive { get; set; }

        /// <summary>
        /// Ghi chú về việc bulk update
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? UpdateNote { get; set; }
    }
}
