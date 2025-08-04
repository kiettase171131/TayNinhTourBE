using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment
{
    /// <summary>
    /// DTO cho việc retry payment
    /// Tương tự như RetryPaymentRequestDTO trong code mẫu Java
    /// </summary>
    public class RetryPaymentRequestDto
    {
        /// <summary>
        /// ID của Order (cho product payment)
        /// </summary>
        public Guid? OrderId { get; set; }

        /// <summary>
        /// ID của TourBooking (cho tour booking payment)
        /// </summary>
        public Guid? TourBookingId { get; set; }
    }
}
