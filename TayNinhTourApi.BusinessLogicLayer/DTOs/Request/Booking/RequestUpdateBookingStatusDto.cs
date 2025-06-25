using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Booking
{
    /// <summary>
    /// DTO cho request cập nhật trạng thái booking
    /// </summary>
    public class RequestUpdateBookingStatusDto
    {
        /// <summary>
        /// Trạng thái mới của booking
        /// </summary>
        [Required(ErrorMessage = "Trạng thái mới là bắt buộc")]
        public BookingStatus NewStatus { get; set; }

        /// <summary>
        /// Lý do thay đổi trạng thái (bắt buộc khi hủy)
        /// </summary>
        [StringLength(500, ErrorMessage = "Lý do không quá 500 ký tự")]
        public string? Reason { get; set; }
    }
}
