using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking
{
    /// <summary>
    /// DTO cho thông tin tour booking
    /// </summary>
    public class TourBookingDto
    {
        public Guid Id { get; set; }
        public Guid TourOperationId { get; set; }
        public Guid UserId { get; set; }
        public int NumberOfGuests { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string BookingCode { get; set; } = string.Empty;
        public string? PayOsOrderCode { get; set; }
        public string? QRCodeData { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime? ConfirmedDate { get; set; }
        public DateTime? CancelledDate { get; set; }
        public string? CancellationReason { get; set; }
        public string? CustomerNotes { get; set; }
        public string? ContactName { get; set; }
        public string? ContactPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? SpecialRequests { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public TourOperationSummaryDto? TourOperation { get; set; }
        public UserSummaryDto? User { get; set; }
    }

    /// <summary>
    /// DTO tóm tắt thông tin TourOperation cho booking
    /// </summary>
    public class TourOperationSummaryDto
    {
        public Guid Id { get; set; }
        public Guid TourDetailsId { get; set; }
        public string TourTitle { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int MaxGuests { get; set; }
        public int CurrentBookings { get; set; }
        public int AvailableSpots => MaxGuests - CurrentBookings;
        public DateTime? TourStartDate { get; set; }
        public string? GuideId { get; set; }
        public string? GuideName { get; set; }
        public string? GuidePhone { get; set; }
    }

    /// <summary>
    /// DTO tóm tắt thông tin User cho booking
    /// </summary>
    public class UserSummaryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
