using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Entity lưu trữ thông tin giao dịch thanh toán
    /// Tương tự như PaymentTransaction trong code mẫu Java
    /// </summary>
    public class PaymentTransaction : BaseEntity
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
        /// Số tiền giao dịch
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Trạng thái giao dịch
        /// </summary>
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        /// <summary>
        /// Mô tả giao dịch
        /// </summary>
        [StringLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Thời gian hết hạn giao dịch
        /// </summary>
        public DateTime? ExpiredAt { get; set; }

        /// <summary>
        /// Cổng thanh toán sử dụng
        /// </summary>
        public PaymentGateway Gateway { get; set; } = PaymentGateway.PayOS;

        /// <summary>
        /// PayOS Order Code with TNDT prefix (e.g., "TNDT1754325287517")
        /// </summary>
        [StringLength(20)]
        public string? PayOsOrderCode { get; set; }

        /// <summary>
        /// PayOS Transaction ID
        /// </summary>
        [StringLength(100)]
        public string? PayOsTransactionId { get; set; }

        /// <summary>
        /// URL checkout PayOS
        /// </summary>
        [StringLength(1000)]
        public string? CheckoutUrl { get; set; }

        /// <summary>
        /// QR Code data từ PayOS
        /// </summary>
        [StringLength(1000)]
        public string? QrCode { get; set; }

        /// <summary>
        /// Lý do thất bại (nếu có)
        /// </summary>
        [StringLength(500)]
        public string? FailureReason { get; set; }

        /// <summary>
        /// ID của transaction cha (cho retry chain)
        /// </summary>
        public Guid? ParentTransactionId { get; set; }

        /// <summary>
        /// Webhook payload từ PayOS (JSON)
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? WebhookPayload { get; set; }

        // Navigation Properties
        public virtual Order? Order { get; set; }
        public virtual TourBooking? TourBooking { get; set; }
        public virtual PaymentTransaction? ParentTransaction { get; set; }
        public virtual ICollection<PaymentTransaction> ChildTransactions { get; set; } = new List<PaymentTransaction>();
    }
}
