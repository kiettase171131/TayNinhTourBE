namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking
{
    /// <summary>
    /// DTO kết quả tính giá tour
    /// </summary>
    public class PriceCalculationDto
    {
        public Guid TourOperationId { get; set; }
        public string TourTitle { get; set; } = string.Empty;
        public int NumberOfGuests { get; set; }
        public decimal OriginalPricePerGuest { get; set; }
        public decimal TotalOriginalPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public bool IsEarlyBird { get; set; }
        public string PricingType { get; set; } = string.Empty;
        public int DaysSinceCreated { get; set; }
        public int DaysUntilTour { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime? TourStartDate { get; set; }
        public DateTime TourDetailsCreatedAt { get; set; }
        public bool IsAvailable { get; set; }
        public int AvailableSpots { get; set; }
        public string? UnavailableReason { get; set; }
    }
}
