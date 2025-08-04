using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment
{
    /// <summary>
    /// DTO cho việc tạo payment link
    /// Tương tự như CreatePaymentRequestDTO trong code mẫu Java
    /// </summary>
    public class CreatePaymentRequestDto
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
        /// Số tiền thanh toán
        /// </summary>
        [Required]
        [Range(1000, double.MaxValue, ErrorMessage = "Số tiền phải lớn hơn 1000 VNĐ")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Mô tả thanh toán
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }
    }
}
