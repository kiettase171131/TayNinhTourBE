using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking
{
    public class UserDashboardSummaryDto
    {
        public int TotalBookings { get; set; }
        public int UpcomingTours { get; set; }
        public int OngoingTours { get; set; }
        public int CompletedTours { get; set; }
        public int CancelledTours { get; set; }
        public int PendingFeedbacks { get; set; }
        public List<TourBookingDto> RecentBookings { get; set; } = new();
        public List<TourBookingDto> UpcomingBookings { get; set; } = new();
    }

    public class UserTourProgressDto
    {
        public Guid TourOperationId { get; set; }
        public string TourTitle { get; set; } = string.Empty;
        public DateTime TourStartDate { get; set; }
        public string? GuideName { get; set; }
        public string? GuidePhone { get; set; }
        public List<TourTimelineProgressItemDto> Timeline { get; set; } = new();
        public TourProgressStatsDto Stats { get; set; } = new();
        public string CurrentStatus { get; set; } = string.Empty;
        public string? CurrentLocation { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
    }

    public class TourTimelineProgressItemDto
    {
        public Guid Id { get; set; }
        public string CheckInTime { get; set; } = string.Empty;
        public string Activity { get; set; } = string.Empty;
        public Guid? SpecialtyShopId { get; set; }
        public string? SpecialtyShopName { get; set; }
        public int SortOrder { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class TourProgressStatsDto
    {
        public int TotalItems { get; set; }
        public int CompletedItems { get; set; }
        public double ProgressPercentage { get; set; }
        public int TotalGuests { get; set; }
        public int CheckedInGuests { get; set; }
        public double CheckInPercentage { get; set; }
    }

    public class ResendQRTicketResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
        public string? Email { get; set; }
    }
}
