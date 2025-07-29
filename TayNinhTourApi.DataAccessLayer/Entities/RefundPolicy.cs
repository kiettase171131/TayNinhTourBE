using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Chính sách hoàn tiền cho tour booking
    /// Định nghĩa rules cho việc tính toán số tiền hoàn dựa trên thời gian hủy
    /// </summary>
    public class RefundPolicy : BaseEntity
    {
        /// <summary>
        /// Loại hoàn tiền áp dụng policy này
        /// </summary>
        [Required]
        public TourRefundType RefundType { get; set; }

        /// <summary>
        /// Số ngày tối thiểu trước tour để áp dụng policy này
        /// Ví dụ: 7 nghĩa là áp dụng cho hủy >= 7 ngày trước tour
        /// </summary>
        [Required]
        [Range(0, 365, ErrorMessage = "Số ngày phải từ 0 đến 365")]
        public int MinDaysBeforeEvent { get; set; }

        /// <summary>
        /// Số ngày tối đa trước tour để áp dụng policy này
        /// Ví dụ: 30 nghĩa là áp dụng cho hủy <= 30 ngày trước tour
        /// Null nghĩa là không giới hạn trên
        /// </summary>
        [Range(0, 365, ErrorMessage = "Số ngày phải từ 0 đến 365")]
        public int? MaxDaysBeforeEvent { get; set; }

        /// <summary>
        /// Phần trăm hoàn tiền (0-100)
        /// Ví dụ: 90 nghĩa là hoàn 90% giá trị booking
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100, ErrorMessage = "Phần trăm hoàn tiền phải từ 0 đến 100")]
        public decimal RefundPercentage { get; set; }

        /// <summary>
        /// Phí xử lý hoàn tiền cố định (VNĐ)
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Phí xử lý phải >= 0")]
        public decimal ProcessingFee { get; set; } = 0;

        /// <summary>
        /// Phần trăm phí xử lý (% của số tiền booking)
        /// Sẽ cộng thêm vào ProcessingFee
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 100, ErrorMessage = "Phần trăm phí xử lý phải từ 0 đến 100")]
        public decimal ProcessingFeePercentage { get; set; } = 0;

        /// <summary>
        /// Mô tả policy cho admin và customer
        /// </summary>
        [Required]
        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Thứ tự ưu tiên khi có nhiều policy cùng điều kiện
        /// Số nhỏ hơn = ưu tiên cao hơn
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "Thứ tự ưu tiên phải từ 1 đến 100")]
        public int Priority { get; set; } = 1;

        /// <summary>
        /// Policy có đang được áp dụng không
        /// </summary>
        [Required]
        public new bool IsActive { get; set; } = true;

        /// <summary>
        /// Ngày bắt đầu có hiệu lực
        /// </summary>
        [Required]
        public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Ngày hết hiệu lực (null = vô thời hạn)
        /// </summary>
        public DateTime? EffectiveTo { get; set; }

        /// <summary>
        /// Ghi chú nội bộ cho admin
        /// </summary>
        [StringLength(1000, ErrorMessage = "Ghi chú không quá 1000 ký tự")]
        public string? InternalNotes { get; set; }

        /// <summary>
        /// Kiểm tra policy có áp dụng được cho số ngày trước tour không
        /// </summary>
        /// <param name="daysBeforeEvent">Số ngày trước tour</param>
        /// <returns>True nếu áp dụng được</returns>
        public bool IsApplicable(int daysBeforeEvent)
        {
            if (!IsActive) return false;
            if (DateTime.UtcNow < EffectiveFrom) return false;
            if (EffectiveTo.HasValue && DateTime.UtcNow > EffectiveTo.Value) return false;

            if (daysBeforeEvent < MinDaysBeforeEvent) return false;
            if (MaxDaysBeforeEvent.HasValue && daysBeforeEvent > MaxDaysBeforeEvent.Value) return false;

            return true;
        }

        /// <summary>
        /// Tính tổng phí xử lý cho một booking amount
        /// </summary>
        /// <param name="bookingAmount">Số tiền booking</param>
        /// <returns>Tổng phí xử lý</returns>
        public decimal CalculateTotalProcessingFee(decimal bookingAmount)
        {
            var percentageFee = bookingAmount * (ProcessingFeePercentage / 100);
            return ProcessingFee + percentageFee;
        }

        /// <summary>
        /// Tính số tiền hoàn thực tế
        /// </summary>
        /// <param name="bookingAmount">Số tiền booking gốc</param>
        /// <returns>Số tiền hoàn sau khi trừ phí</returns>
        public decimal CalculateRefundAmount(decimal bookingAmount)
        {
            var refundBeforeFee = bookingAmount * (RefundPercentage / 100);
            var totalFee = CalculateTotalProcessingFee(bookingAmount);
            return Math.Max(0, refundBeforeFee - totalFee);
        }
    }
}
