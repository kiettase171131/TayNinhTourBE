using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot
{
    /// <summary>
    /// DTO cho request cập nhật tour slot
    /// </summary>
    public class RequestUpdateSlotDto
    {
        /// <summary>
        /// Trạng thái mới của slot
        /// </summary>
        public TourSlotStatus? Status { get; set; }

        /// <summary>
        /// Slot có active không
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>
        /// Ghi chú về việc cập nhật
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? UpdateNote { get; set; }
    }
}
