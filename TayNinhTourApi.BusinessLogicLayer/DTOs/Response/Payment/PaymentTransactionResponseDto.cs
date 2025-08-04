using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment
{
    /// <summary>
    /// DTO response cho payment transaction
    /// Tương tự như PaymentTransactionResponse trong code mẫu Java
    /// </summary>
    public class PaymentTransactionResponseDto
    {
        /// <summary>
        /// ID của payment transaction
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của Order (nếu có)
        /// </summary>
        public Guid? OrderId { get; set; }

        /// <summary>
        /// ID của TourBooking (nếu có)
        /// </summary>
        public Guid? TourBookingId { get; set; }

        /// <summary>
        /// Số tiền giao dịch
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Trạng thái giao dịch
        /// </summary>
        public PaymentStatus Status { get; set; }

        /// <summary>
        /// Mô tả giao dịch
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Cổng thanh toán
        /// </summary>
        public PaymentGateway Gateway { get; set; }

        /// <summary>
        /// PayOS Order Code
        /// </summary>
        public long? PayOsOrderCode { get; set; }

        /// <summary>
        /// PayOS Transaction ID
        /// </summary>
        public string? PayOsTransactionId { get; set; }

        /// <summary>
        /// URL checkout
        /// </summary>
        public string? CheckoutUrl { get; set; }

        /// <summary>
        /// QR Code
        /// </summary>
        public string? QrCode { get; set; }

        /// <summary>
        /// Lý do thất bại
        /// </summary>
        public string? FailureReason { get; set; }

        /// <summary>
        /// ID transaction cha
        /// </summary>
        public Guid? ParentTransactionId { get; set; }

        /// <summary>
        /// Thời gian hết hạn
        /// </summary>
        public DateTime? ExpiredAt { get; set; }

        /// <summary>
        /// Thời gian tạo
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Thời gian cập nhật
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Loại thanh toán: "ProductPayment" hoặc "TourBookingPayment"
        /// </summary>
        public string PaymentType { get; set; } = string.Empty;

        /// <summary>
        /// Thông tin Order (nếu là Product Payment)
        /// </summary>
        public object? OrderInfo { get; set; }

        /// <summary>
        /// Thông tin TourBooking (nếu là Tour Booking Payment)
        /// </summary>
        public object? TourBookingInfo { get; set; }
    }
}
