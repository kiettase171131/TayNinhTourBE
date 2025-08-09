namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourGuide
{
    /// <summary>
    /// Response DTO for timeline progress operations
    /// </summary>
    public class TimelineProgressResponse
    {
        /// <summary>
        /// Timeline items with progress information
        /// </summary>
        public List<TimelineWithProgressDto> Timeline { get; set; } = new List<TimelineWithProgressDto>();

        /// <summary>
        /// Progress summary
        /// </summary>
        public TimelineProgressSummaryDto Summary { get; set; } = new TimelineProgressSummaryDto();

        /// <summary>
        /// Tour slot information
        /// </summary>
        public TourSlotInfoDto TourSlot { get; set; } = new TourSlotInfoDto();

        /// <summary>
        /// Tour details information
        /// </summary>
        public TourDetailsInfoDto TourDetails { get; set; } = new TourDetailsInfoDto();

        /// <summary>
        /// Whether the tour guide can modify timeline progress
        /// </summary>
        public bool CanModifyProgress { get; set; } = true;

        /// <summary>
        /// Last updated timestamp
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Response DTO for completing timeline items
    /// </summary>
    public class CompleteTimelineResponse
    {
        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Updated timeline item with progress
        /// </summary>
        public TimelineWithProgressDto? CompletedItem { get; set; }

        /// <summary>
        /// Updated progress summary
        /// </summary>
        public TimelineProgressSummaryDto? Summary { get; set; }

        /// <summary>
        /// Next item to be completed (if any)
        /// </summary>
        public TimelineWithProgressDto? NextItem { get; set; }

        /// <summary>
        /// Whether the entire timeline is now completed
        /// </summary>
        public bool IsTimelineCompleted { get; set; }

        /// <summary>
        /// Completion timestamp
        /// </summary>
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Any warnings or additional information
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>
    /// Response DTO for bulk timeline operations
    /// </summary>
    public class BulkTimelineResponse
    {
        /// <summary>
        /// Number of items successfully processed
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// Number of items that failed to process
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// Total number of items attempted
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// List of successfully processed item IDs
        /// </summary>
        public List<Guid> SuccessfulItems { get; set; } = new List<Guid>();

        /// <summary>
        /// List of failed items with error messages
        /// </summary>
        public List<BulkOperationError> FailedItems { get; set; } = new List<BulkOperationError>();

        /// <summary>
        /// Updated progress summary
        /// </summary>
        public TimelineProgressSummaryDto? Summary { get; set; }

        /// <summary>
        /// Overall operation message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Whether the operation was completely successful
        /// </summary>
        public bool IsFullySuccessful => FailureCount == 0;

        /// <summary>
        /// Whether the operation was partially successful
        /// </summary>
        public bool IsPartiallySuccessful => SuccessCount > 0 && FailureCount > 0;
    }

    /// <summary>
    /// Error information for bulk operations
    /// </summary>
    public class BulkOperationError
    {
        /// <summary>
        /// Item ID that failed
        /// </summary>
        public Guid ItemId { get; set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Error code for programmatic handling
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tour slot information for timeline context
    /// </summary>
    public class TourSlotInfoDto
    {
        /// <summary>
        /// Tour slot ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tour date
        /// </summary>
        public DateOnly TourDate { get; set; }

        /// <summary>
        /// Current number of bookings
        /// </summary>
        public int CurrentBookings { get; set; }

        /// <summary>
        /// Maximum number of guests
        /// </summary>
        public int MaxGuests { get; set; }

        /// <summary>
        /// Tour slot status
        /// </summary>
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Tour details information for timeline context
    /// </summary>
    public class TourDetailsInfoDto
    {
        /// <summary>
        /// Tour details ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Tour title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Tour description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tour status
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Tour images
        /// </summary>
        public List<string> ImageUrls { get; set; } = new List<string>();
    }

    /// <summary>
    /// Statistics response for timeline analytics
    /// </summary>
    public class TimelineStatisticsResponse
    {
        /// <summary>
        /// Tour slot ID
        /// </summary>
        public Guid TourSlotId { get; set; }

        /// <summary>
        /// Average completion time per item (in minutes)
        /// </summary>
        public double AverageCompletionTimeMinutes { get; set; }

        /// <summary>
        /// Total time spent on timeline (in minutes)
        /// </summary>
        public double TotalTimeMinutes { get; set; }

        /// <summary>
        /// Completion rate percentage
        /// </summary>
        public double CompletionRate { get; set; }

        /// <summary>
        /// Items completed on time vs overdue
        /// </summary>
        public int OnTimeCompletions { get; set; }

        /// <summary>
        /// Items completed after scheduled time
        /// </summary>
        public int OverdueCompletions { get; set; }

        /// <summary>
        /// Most time-consuming timeline item
        /// </summary>
        public TimelineWithProgressDto? SlowestItem { get; set; }

        /// <summary>
        /// Fastest completed timeline item
        /// </summary>
        public TimelineWithProgressDto? FastestItem { get; set; }

        /// <summary>
        /// Timeline completion trend data
        /// </summary>
        public List<CompletionTrendPoint> CompletionTrend { get; set; } = new List<CompletionTrendPoint>();
    }

    /// <summary>
    /// Data point for completion trend analysis
    /// </summary>
    public class CompletionTrendPoint
    {
        /// <summary>
        /// Time point
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Cumulative completion percentage at this time
        /// </summary>
        public double CompletionPercentage { get; set; }

        /// <summary>
        /// Number of items completed at this time
        /// </summary>
        public int ItemsCompleted { get; set; }
    }
}
