using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourGuide
{
    /// <summary>
    /// Request DTO for completing a timeline item
    /// </summary>
    public class CompleteTimelineRequest
    {
        /// <summary>
        /// Optional notes to add when completing the timeline item
        /// Maximum 500 characters
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        /// <summary>
        /// Optional completion time override (defaults to current time)
        /// Useful for backdating completion if needed
        /// </summary>
        public DateTime? CompletionTime { get; set; }

        /// <summary>
        /// Validate the request
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (!string.IsNullOrEmpty(Notes) && Notes.Length > 500)
            {
                errors.Add("Ghi chú không được vượt quá 500 ký tự");
            }

            if (CompletionTime.HasValue && CompletionTime.Value > DateTime.UtcNow)
            {
                errors.Add("Thời gian hoàn thành không được trong tương lai");
            }

            return errors;
        }

        /// <summary>
        /// Get the effective completion time (provided time or current time)
        /// </summary>
        /// <returns>Completion time to use</returns>
        public DateTime GetEffectiveCompletionTime()
        {
            return CompletionTime ?? DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Request DTO for bulk completing multiple timeline items
    /// </summary>
    public class BulkCompleteTimelineRequest
    {
        /// <summary>
        /// List of timeline item IDs to complete
        /// </summary>
        [Required(ErrorMessage = "Danh sách timeline items là bắt buộc")]
        [MinLength(1, ErrorMessage = "Phải có ít nhất 1 timeline item")]
        public List<Guid> TimelineItemIds { get; set; } = new List<Guid>();

        /// <summary>
        /// Tour slot ID for the timeline items
        /// </summary>
        [Required(ErrorMessage = "Tour slot ID là bắt buộc")]
        public Guid TourSlotId { get; set; }

        /// <summary>
        /// Optional notes to add to all completed items
        /// </summary>
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        public string? Notes { get; set; }

        /// <summary>
        /// Whether to respect sequential completion rules
        /// If false, allows completing items out of order
        /// </summary>
        public bool RespectSequentialOrder { get; set; } = true;

        /// <summary>
        /// Validate the request
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (TimelineItemIds == null || TimelineItemIds.Count == 0)
            {
                errors.Add("Danh sách timeline items là bắt buộc");
            }

            if (TourSlotId == Guid.Empty)
            {
                errors.Add("Tour slot ID là bắt buộc");
            }

            if (!string.IsNullOrEmpty(Notes) && Notes.Length > 500)
            {
                errors.Add("Ghi chú không được vượt quá 500 ký tự");
            }

            // Check for duplicate IDs
            if (TimelineItemIds != null && TimelineItemIds.Distinct().Count() != TimelineItemIds.Count)
            {
                errors.Add("Danh sách timeline items chứa ID trùng lặp");
            }

            return errors;
        }
    }

    /// <summary>
    /// Request DTO for resetting timeline item completion
    /// </summary>
    public class ResetTimelineRequest
    {
        /// <summary>
        /// Reason for resetting the completion
        /// </summary>
        [Required(ErrorMessage = "Lý do reset là bắt buộc")]
        [StringLength(500, ErrorMessage = "Lý do không được vượt quá 500 ký tự")]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Whether to reset all subsequent items as well
        /// </summary>
        public bool ResetSubsequentItems { get; set; } = true;

        /// <summary>
        /// Validate the request
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Reason))
            {
                errors.Add("Lý do reset là bắt buộc");
            }

            if (!string.IsNullOrEmpty(Reason) && Reason.Length > 500)
            {
                errors.Add("Lý do không được vượt quá 500 ký tự");
            }

            return errors;
        }
    }

    /// <summary>
    /// Request DTO for getting timeline with progress
    /// </summary>
    public class GetTimelineProgressRequest
    {
        /// <summary>
        /// Tour slot ID to get timeline progress for
        /// </summary>
        [Required(ErrorMessage = "Tour slot ID là bắt buộc")]
        public Guid TourSlotId { get; set; }

        /// <summary>
        /// Whether to include inactive timeline items
        /// </summary>
        public bool IncludeInactive { get; set; } = false;

        /// <summary>
        /// Whether to include specialty shop information
        /// </summary>
        public bool IncludeShopInfo { get; set; } = true;

        /// <summary>
        /// Whether to include completion statistics
        /// </summary>
        public bool IncludeStatistics { get; set; } = true;

        /// <summary>
        /// Filter by completion status
        /// null = all, true = completed only, false = pending only
        /// </summary>
        public bool? CompletionFilter { get; set; }

        /// <summary>
        /// Validate the request
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (TourSlotId == Guid.Empty)
            {
                errors.Add("Tour slot ID là bắt buộc");
            }

            return errors;
        }
    }
}
