using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment
{
    /// <summary>
    /// DTO response cho payment link
    /// Tương tự như PaymentLinkResponseDTO trong code mẫu Java
    /// </summary>
    public class PaymentLinkResponseDto
    {
        /// <summary>
        /// ID của payment transaction
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// PayOS Order Code
        /// </summary>
        public long PayOsOrderCode { get; set; }

        /// <summary>
        /// URL checkout PayOS
        /// </summary>
        public string CheckoutUrl { get; set; } = string.Empty;

        /// <summary>
        /// QR Code data
        /// </summary>
        public string? QrCode { get; set; }

        /// <summary>
        /// Số tiền thanh toán
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Mô tả thanh toán
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Trạng thái giao dịch
        /// </summary>
        public PaymentStatus Status { get; set; }

        /// <summary>
        /// Thời gian hết hạn
        /// </summary>
        public DateTime? ExpiredAt { get; set; }

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
