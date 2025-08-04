using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment
{
    /// <summary>
    /// DTO cho việc cancel payment
    /// Tương tự như CancelPaymentRequestDTO trong code mẫu Java
    /// </summary>
    public class CancelPaymentRequestDto
    {
        /// <summary>
        /// ID của Order (cho product payment)
        /// </summary>
        public Guid? OrderId { get; set; }

        /// <summary>
        /// ID của TourBooking (cho tour booking payment)
        /// </summary>
        public Guid? TourBookingId { get; set; }

        /// <summary>
        /// Lý do hủy thanh toán
        /// </summary>
        [StringLength(500)]
        public string? CancellationReason { get; set; }
    }
}
