using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourGuide
{
    /// <summary>
    /// DTO for timeline item with progress information for tour guide
    /// Combines timeline template data with slot-specific progress
    /// </summary>
    public class TimelineWithProgressDto
    {
        /// <summary>
        /// Timeline item ID (template)
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tour slot ID for this progress
        /// </summary>
        public Guid TourSlotId { get; set; }

        /// <summary>
        /// Progress record ID (null if no progress record exists yet)
        /// </summary>
        public Guid? ProgressId { get; set; }

        /// <summary>
        /// Activity description
        /// </summary>
        public string Activity { get; set; } = string.Empty;

        /// <summary>
        /// Check-in time for this activity
        /// </summary>
        public TimeOnly CheckInTime { get; set; }

        /// <summary>
        /// Sort order for timeline sequence
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Whether this timeline item has been completed for this tour slot
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// When this timeline item was completed (null if not completed)
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Notes added when completing this timeline item
        /// </summary>
        public string? CompletionNotes { get; set; }

        /// <summary>
        /// Whether this timeline item can be completed now
        /// Based on sequential completion rules
        /// </summary>
        public bool CanComplete { get; set; }

        /// <summary>
        /// Specialty shop information if this timeline item involves a shop visit
        /// </summary>
        public SpecialtyShopResponseDto? SpecialtyShop { get; set; }

        /// <summary>
        /// User who completed this timeline item
        /// </summary>
        public string? CompletedByName { get; set; }

        /// <summary>
        /// Duration between creation and completion (for analytics)
        /// </summary>
        public TimeSpan? CompletionDuration { get; set; }

        /// <summary>
        /// Status text for display
        /// </summary>
        public string StatusText { get; set; } = string.Empty;

        /// <summary>
        /// Whether this is the next item to be completed
        /// </summary>
        public bool IsNext { get; set; }

        /// <summary>
        /// Position in the timeline (1-based)
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Total number of timeline items
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Progress percentage for this specific item (0 or 100)
        /// </summary>
        public int ProgressPercentage => IsCompleted ? 100 : 0;

        /// <summary>
        /// CSS class for status styling
        /// </summary>
        public string StatusClass => IsCompleted ? "completed" : CanComplete ? "active" : "pending";

        /// <summary>
        /// Icon name for status display
        /// </summary>
        public string StatusIcon => IsCompleted ? "check-circle" : CanComplete ? "play-circle" : "clock-circle";

        /// <summary>
        /// Helper method to get formatted completion time
        /// </summary>
        public string GetFormattedCompletionTime()
        {
            return CompletedAt?.ToString("HH:mm dd/MM/yyyy") ?? "Chưa hoàn thành";
        }

        /// <summary>
        /// Helper method to get time until check-in
        /// </summary>
        public string GetTimeUntilCheckIn()
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            if (CheckInTime > now)
            {
                var diff = CheckInTime - now;
                return $"Còn {diff.Hours}h {diff.Minutes}m";
            }
            return "Đã đến giờ";
        }

        /// <summary>
        /// Helper method to check if this item is overdue
        /// </summary>
        public bool IsOverdue()
        {
            if (IsCompleted) return false;
            var now = TimeOnly.FromDateTime(DateTime.Now);
            return CheckInTime < now;
        }

        /// <summary>
        /// Helper method to get display priority
        /// </summary>
        public int GetDisplayPriority()
        {
            if (IsCompleted) return 3; // Lowest priority
            if (CanComplete) return 1; // Highest priority
            return 2; // Medium priority
        }
    }

    /// <summary>
    /// Summary DTO for timeline progress overview
    /// </summary>
    public class TimelineProgressSummaryDto
    {
        /// <summary>
        /// Tour slot ID
        /// </summary>
        public Guid TourSlotId { get; set; }

        /// <summary>
        /// Total number of timeline items
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// Number of completed items
        /// </summary>
        public int CompletedItems { get; set; }

        /// <summary>
        /// Number of pending items
        /// </summary>
        public int PendingItems => TotalItems - CompletedItems;

        /// <summary>
        /// Overall progress percentage
        /// </summary>
        public int ProgressPercentage => TotalItems > 0 ? (CompletedItems * 100) / TotalItems : 0;

        /// <summary>
        /// Whether all timeline items are completed
        /// </summary>
        public bool IsFullyCompleted => CompletedItems == TotalItems && TotalItems > 0;

        /// <summary>
        /// Next timeline item to be completed
        /// </summary>
        public TimelineWithProgressDto NextItem { get; set; } = new TimelineWithProgressDto();

        /// <summary>
        /// Last completed timeline item
        /// </summary>
        public TimelineWithProgressDto LastCompletedItem { get; set; } = new TimelineWithProgressDto();

        /// <summary>
        /// Estimated completion time based on current progress
        /// </summary>
        public DateTime? EstimatedCompletionTime { get; set; }

        /// <summary>
        /// Status text for overall progress
        /// </summary>
        public string StatusText => IsFullyCompleted ? "Hoàn thành" : $"{CompletedItems}/{TotalItems} hoàn thành";

        /// <summary>
        /// CSS class for progress bar styling
        /// </summary>
        public string ProgressClass => ProgressPercentage switch
        {
            100 => "success",
            >= 75 => "warning",
            >= 50 => "info",
            _ => "default"
        };
    }
}
