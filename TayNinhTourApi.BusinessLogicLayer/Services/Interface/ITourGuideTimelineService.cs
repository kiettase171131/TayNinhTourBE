using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourGuide;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourGuide;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Interface for tour guide timeline progress management
    /// </summary>
    public interface ITourGuideTimelineService
    {
        /// <summary>
        /// Get timeline with progress for a specific tour slot
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="userId">Current user ID (tour guide)</param>
        /// <param name="includeInactive">Whether to include inactive timeline items</param>
        /// <param name="includeShopInfo">Whether to include specialty shop information</param>
        /// <returns>Timeline with progress information</returns>
        Task<TimelineProgressResponse> GetTimelineWithProgressAsync(
            Guid tourSlotId,
            Guid? userId,
            bool includeInactive = false,
            bool includeShopInfo = true);

        /// <summary>
        /// Complete a timeline item for a specific tour slot
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="timelineItemId">Timeline item ID</param>
        /// <param name="request">Completion request details</param>
        /// <param name="userId">Current user ID (tour guide)</param>
        /// <returns>Completion result</returns>
        Task<CompleteTimelineResponse> CompleteTimelineItemAsync(
            Guid tourSlotId, 
            Guid timelineItemId, 
            CompleteTimelineRequest request, 
            Guid userId);

        /// <summary>
        /// Complete multiple timeline items in bulk
        /// </summary>
        /// <param name="request">Bulk completion request</param>
        /// <param name="userId">Current user ID (tour guide)</param>
        /// <returns>Bulk operation result</returns>
        Task<BulkTimelineResponse> BulkCompleteTimelineItemsAsync(
            BulkCompleteTimelineRequest request, 
            Guid userId);

        /// <summary>
        /// Reset completion status of a timeline item
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="timelineItemId">Timeline item ID</param>
        /// <param name="request">Reset request details</param>
        /// <param name="userId">Current user ID (tour guide)</param>
        /// <returns>Reset result</returns>
        Task<CompleteTimelineResponse> ResetTimelineItemAsync(
            Guid tourSlotId, 
            Guid timelineItemId, 
            ResetTimelineRequest request, 
            Guid userId);

        /// <summary>
        /// Get timeline progress summary for a tour slot
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="userId">Current user ID (tour guide)</param>
        /// <returns>Progress summary</returns>
        Task<TimelineProgressSummaryDto> GetProgressSummaryAsync(Guid tourSlotId, Guid userId);

        /// <summary>
        /// Get timeline statistics for analytics
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="userId">Current user ID (tour guide)</param>
        /// <returns>Timeline statistics</returns>
        Task<TimelineStatisticsResponse> GetTimelineStatisticsAsync(Guid tourSlotId, Guid userId);

        /// <summary>
        /// Auto-create progress records for a tour slot when it's assigned to tour details
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="createdById">User ID who triggered the creation</param>
        /// <returns>Number of progress records created</returns>
        Task<int> CreateProgressRecordsForTourSlotAsync(Guid tourSlotId, Guid createdById);

        /// <summary>
        /// Validate if a tour guide can access timeline for a specific tour slot
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="userId">Tour guide user ID</param>
        /// <returns>True if access is allowed</returns>
        Task<bool> ValidateTourGuideAccessAsync(Guid tourSlotId, Guid userId);

        /// <summary>
        /// Get next timeline item that can be completed
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="userId">Current user ID (tour guide)</param>
        /// <returns>Next timeline item or null if none available</returns>
        Task<TimelineWithProgressDto?> GetNextTimelineItemAsync(Guid tourSlotId, Guid userId);

        /// <summary>
        /// Check if a timeline item can be completed (sequential validation)
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="timelineItemId">Timeline item ID</param>
        /// <param name="userId">Current user ID (tour guide)</param>
        /// <returns>True if item can be completed</returns>
        Task<bool> CanCompleteTimelineItemAsync(Guid tourSlotId, Guid timelineItemId, Guid userId);

        /// <summary>
        /// Get timeline completion history for a tour slot
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="userId">Current user ID (tour guide)</param>
        /// <returns>List of completion events</returns>
        Task<List<CompletionTrendPoint>> GetCompletionHistoryAsync(Guid tourSlotId, Guid userId);

        /// <summary>
        /// Send notifications to guests about timeline progress
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="timelineItemId">Completed timeline item ID</param>
        /// <param name="userId">Tour guide user ID</param>
        /// <returns>Number of notifications sent</returns>
        Task<int> NotifyGuestsAboutProgressAsync(Guid tourSlotId, Guid timelineItemId, Guid userId);
    }
}
