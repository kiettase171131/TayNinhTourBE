using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs
{
    // Application/Queries/ShopVisitQuery.cs
    public class ShopVisitQuery
    {
        public Guid ShopId { get; set; }

        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerEmail { get; set; }

        public Guid TourSlotId { get; set; }
        public DateOnly TourDate { get; set; }
        public string? TourName { get; set; }

        public Guid TimelineItemId { get; set; }
        public string Activity { get; set; } = string.Empty;
        public int SortOrder { get; set; }

        public TimeSpan PlannedCheckInTime { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? ActualCompletedAt { get; set; }
    }

}
