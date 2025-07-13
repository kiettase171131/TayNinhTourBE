namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking
{
    /// <summary>
    /// DTO kết quả tạo booking tour
    /// </summary>
    public class CreateBookingResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? BookingId { get; set; }
        public string? BookingCode { get; set; }
        public string? PaymentUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal FinalPrice { get; set; }
        public string PricingType { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
        public DateTime? TourStartDate { get; set; }
    }
}
