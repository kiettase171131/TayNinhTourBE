using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Entity đại diện cho booking của khách hàng cho một tour operation
    /// </summary>
    public class TourBooking : BaseEntity
    {
        /// <summary>
        /// ID của TourOperation được booking
        /// </summary>
        [Required]
        public Guid TourOperationId { get; set; }

        /// <summary>
        /// ID của TourSlot cụ thể được booking (optional)
        /// Nếu có, booking sẽ được gắn với ngày cụ thể của slot này
        /// </summary>
        public Guid? TourSlotId { get; set; }

        /// <summary>
        /// ID của User thực hiện booking
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Số lượng khách trong booking này
        /// </summary>
        [Required]
        [Range(1, 50, ErrorMessage = "Số lượng khách phải từ 1 đến 50")]
        public int NumberOfGuests { get; set; }

        /// <summary>
        /// Giá gốc của tour (trước khi áp dụng discount)
        /// </summary>
        [Required]
        public decimal OriginalPrice { get; set; }

        /// <summary>
        /// Phần trăm giảm giá được áp dụng (0-100)
        /// </summary>
        [Range(0, 100)]
        public decimal DiscountPercent { get; set; } = 0;

        /// <summary>
        /// Tổng giá tiền của booking (sau khi áp dụng discount)
        /// </summary>
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng giá phải >= 0")]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Số tiền giữ lại (revenue hold) của booking này
        /// Được cộng khi khách thanh toán thành công (90% của TotalPrice sau khi trừ 10% commission)
        /// Sẽ được chuyển vào ví của TourCompany sau 3 ngày từ khi tour hoàn thành
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal RevenueHold { get; set; } = 0;

        /// <summary>
        /// Ngày chuyển tiền từ revenue hold sang wallet của TourCompany
        /// Null nếu chưa chuyển tiền
        /// </summary>
        public DateTime? RevenueTransferredDate { get; set; }

        /// <summary>
        /// Trạng thái của booking
        /// </summary>
        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        /// <summary>
        /// Ngày booking được tạo
        /// </summary>
        [Required]
        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày xác nhận booking (nếu có)
        /// </summary>
        public DateTime? ConfirmedDate { get; set; }

        /// <summary>
        /// Ngày hủy booking (nếu có)
        /// </summary>
        public DateTime? CancelledDate { get; set; }

        /// <summary>
        /// Lý do hủy booking
        /// </summary>
        [StringLength(500, ErrorMessage = "Lý do hủy không quá 500 ký tự")]
        public string? CancellationReason { get; set; }

        /// <summary>
        /// Ghi chú từ khách hàng
        /// </summary>
        [StringLength(1000, ErrorMessage = "Ghi chú không quá 1000 ký tự")]
        public string? CustomerNotes { get; set; }

        /// <summary>
        /// Thông tin liên hệ khách hàng
        /// </summary>
        [StringLength(100, ErrorMessage = "Tên liên hệ không quá 100 ký tự")]
        public string? ContactName { get; set; }

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        [StringLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự")]
        public string? ContactPhone { get; set; }

        /// <summary>
        /// Email liên hệ
        /// </summary>
        [StringLength(100, ErrorMessage = "Email không quá 100 ký tự")]
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Mã booking duy nhất cho khách hàng
        /// Format: TB + YYYYMMDD + 6 số random
        /// </summary>
        [Required]
        [StringLength(20, ErrorMessage = "Mã booking không quá 20 ký tự")]
        public string BookingCode { get; set; } = string.Empty;

        /// <summary>
        /// PayOS order code cho thanh toán
        /// Format: TNDT + 10 digits
        /// </summary>
        [StringLength(20)]
        public string? PayOsOrderCode { get; set; }

        /// <summary>
        /// QR code data cho khách hàng (để HDV quét ngày tour)
        /// </summary>
        public string? QRCodeData { get; set; }

        /// <summary>
        /// Thời gian hết hạn reservation (để tự động release slot nếu không thanh toán)
        /// Null nếu booking đã được confirm hoặc cancel
        /// </summary>
        public DateTime? ReservedUntil { get; set; }

        /// <summary>
        /// Trạng thái check-in của khách hàng (để HDV quét QR khi tour bắt đầu)
        /// </summary>
        public bool IsCheckedIn { get; set; } = false;

        /// <summary>
        /// Thời gian check-in thực tế của khách hàng
        /// Được set khi HDV quét QR code thành công
        /// </summary>
        public DateTime? CheckInTime { get; set; }

        /// <summary>
        /// Ghi chú bổ sung khi check-in (optional)
        /// Ví dụ: số người thực tế, ghi chú đặc biệt
        /// </summary>
        [StringLength(500)]
        public string? CheckInNotes { get; set; }

        /// <summary>
        /// Row version cho optimistic concurrency control
        /// </summary>
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        // Navigation Properties

        /// <summary>
        /// TourOperation được booking
        /// </summary>
        public virtual TourOperation TourOperation { get; set; } = null!;

        /// <summary>
        /// TourSlot cụ thể được booking (optional)
        /// </summary>
        public virtual TourSlot? TourSlot { get; set; }

        /// <summary>
        /// User thực hiện booking
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Refund request for this booking (1:1 relationship, optional)
        /// </summary>
        public virtual TourBookingRefund? RefundRequest { get; set; }
    }
}
