using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot
{
    /// <summary>
    /// DTO cho request h?y tour slot c�ng khai
    /// </summary>
    public class CancelPublicTourSlotDto
    {
        /// <summary>
        /// L� do h?y tour (b?t bu?c)
        /// S? ???c g?i trong email th�ng b�o cho kh�ch h�ng
        /// </summary>
        [Required(ErrorMessage = "L� do h?y tour l� b?t bu?c")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "L� do h?y ph?i t? 10 ??n 1000 k� t?")]
        public string Reason { get; set; } = null!;

        /// <summary>
        /// Th�ng ?i?p b? sung g?i cho kh�ch h�ng (t�y ch?n)
        /// </summary>
        [StringLength(500, ErrorMessage = "Th�ng ?i?p b? sung kh�ng ???c qu� 500 k� t?")]
        public string? AdditionalMessage { get; set; }
    }

    /// <summary>
    /// DTO cho response k?t qu? h?y tour slot
    /// </summary>
    public class CancelTourSlotResultDto
    {
        /// <summary>
        /// K?t qu? th�nh c�ng hay kh�ng
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Th�ng ?i?p k?t qu?
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// S? l??ng kh�ch h�ng ???c th�ng b�o
        /// </summary>
        public int CustomersNotified { get; set; }

        /// <summary>
        /// S? l??ng booking b? ?nh h??ng
        /// </summary>
        public int AffectedBookings { get; set; }

        /// <summary>
        /// T?ng s? ti?n c?n ho�n tr?
        /// </summary>
        public decimal TotalRefundAmount { get; set; }

        /// <summary>
        /// Danh s�ch kh�ch h�ng b? ?nh h??ng
        /// </summary>
        public List<AffectedCustomerInfo> AffectedCustomers { get; set; } = new();
    }

    /// <summary>
    /// DTO th�ng tin kh�ch h�ng b? ?nh h??ng
    /// </summary>
    public class AffectedCustomerInfo
    {
        /// <summary>
        /// ID booking
        /// </summary>
        public Guid BookingId { get; set; }

        /// <summary>
        /// M� booking
        /// </summary>
        public string BookingCode { get; set; } = null!;

        /// <summary>
        /// T�n kh�ch h�ng
        /// </summary>
        public string CustomerName { get; set; } = null!;

        /// <summary>
        /// Email kh�ch h�ng
        /// </summary>
        public string CustomerEmail { get; set; } = null!;

        /// <summary>
        /// S? l??ng kh�ch
        /// </summary>
        public int NumberOfGuests { get; set; }

        /// <summary>
        /// S? ti?n c?n ho�n
        /// </summary>
        public decimal RefundAmount { get; set; }

        /// <summary>
        /// Tr?ng th�i g?i email
        /// </summary>
        public bool EmailSent { get; set; }
    }
}