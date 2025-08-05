using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot
{
    /// <summary>
    /// DTO cho request h?y tour slot công khai
    /// </summary>
    public class CancelPublicTourSlotDto
    {
        /// <summary>
        /// Lý do h?y tour (b?t bu?c)
        /// S? ???c g?i trong email thông báo cho khách hàng
        /// </summary>
        [Required(ErrorMessage = "Lý do hủy tour là bắt buộc")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Lý do hủy phải từ 10 đến 1000 ký tự")]
        public string Reason { get; set; } = null!;

        /// <summary>
        /// Thông ?i?p b? sung g?i cho khách hàng (tùy ch?n)
        /// </summary>
        [StringLength(500, ErrorMessage = "Thông điệp bổ sung không được quá 500 ký tự")]
        public string? AdditionalMessage { get; set; }
    }

    /// <summary>
    /// DTO cho response k?t qu? h?y tour slot
    /// </summary>
    public class CancelTourSlotResultDto
    {
        /// <summary>
        /// K?t qu? thành công hay không
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Thông ?i?p k?t qu?
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// S? l??ng khách hàng ???c thông báo
        /// </summary>
        public int CustomersNotified { get; set; }

        /// <summary>
        /// S? l??ng booking b? ?nh h??ng
        /// </summary>
        public int AffectedBookings { get; set; }

        /// <summary>
        /// T?ng s? ti?n c?n hoàn tr?
        /// </summary>
        public decimal TotalRefundAmount { get; set; }

        /// <summary>
        /// Danh sách khách hàng b? ?nh h??ng
        /// </summary>
        public List<AffectedCustomerInfo> AffectedCustomers { get; set; } = new();
    }

    /// <summary>
    /// DTO thông tin khách hàng b? ?nh h??ng
    /// </summary>
    public class AffectedCustomerInfo
    {
        /// <summary>
        /// ID booking
        /// </summary>
        public Guid BookingId { get; set; }

        /// <summary>
        /// Mã booking
        /// </summary>
        public string BookingCode { get; set; } = null!;

        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string CustomerName { get; set; } = null!;

        /// <summary>
        /// Email khách hàng
        /// </summary>
        public string CustomerEmail { get; set; } = null!;

        /// <summary>
        /// S? l??ng khách
        /// </summary>
        public int NumberOfGuests { get; set; }

        /// <summary>
        /// S? ti?n c?n hoàn
        /// </summary>
        public decimal RefundAmount { get; set; }

        /// <summary>
        /// Tr?ng thái g?i email
        /// </summary>
        public bool EmailSent { get; set; }
    }
}