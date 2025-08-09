using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Entity to track timeline completion progress for individual tour slots
    /// Enables each tour slot to have independent timeline progress tracking
    /// </summary>
    [Table("TourSlotTimelineProgress")]
    public class TourSlotTimelineProgress : BaseEntity
    {
        /// <summary>
        /// Reference to the specific tour slot
        /// </summary>
        [Required]
        public Guid TourSlotId { get; set; }

        /// <summary>
        /// Reference to the timeline item template
        /// </summary>
        [Required]
        public Guid TimelineItemId { get; set; }

        /// <summary>
        /// Whether this timeline item has been completed for this tour slot
        /// </summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Timestamp when the timeline item was completed
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Optional notes added when completing the timeline item
        /// </summary>
        [StringLength(500)]
        public string? CompletionNotes { get; set; }

        // Navigation Properties

        /// <summary>
        /// Navigation property to the tour slot
        /// </summary>
        [ForeignKey(nameof(TourSlotId))]
        public virtual TourSlot TourSlot { get; set; } = null!;

        /// <summary>
        /// Navigation property to the timeline item template
        /// </summary>
        [ForeignKey(nameof(TimelineItemId))]
        public virtual TimelineItem TimelineItem { get; set; } = null!;

        /// <summary>
        /// Navigation property to the user who created this progress record
        /// </summary>
        [ForeignKey(nameof(CreatedById))]
        public virtual User CreatedBy { get; set; } = null!;

        /// <summary>
        /// Navigation property to the user who last updated this progress record
        /// </summary>
        [ForeignKey(nameof(UpdatedById))]
        public virtual User? UpdatedBy { get; set; }

        // Helper Methods

        /// <summary>
        /// Mark this timeline item as completed
        /// </summary>
        /// <param name="completedById">ID of the user completing the item</param>
        /// <param name="notes">Optional completion notes</param>
        public void MarkAsCompleted(Guid completedById, string? notes = null)
        {
            IsCompleted = true;
            CompletedAt = DateTime.UtcNow;
            CompletionNotes = notes;
            UpdatedById = completedById;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Reset completion status
        /// </summary>
        /// <param name="resetById">ID of the user resetting the item</param>
        public void ResetCompletion(Guid resetById)
        {
            IsCompleted = false;
            CompletedAt = null;
            CompletionNotes = null;
            UpdatedById = resetById;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Check if this progress record can be completed
        /// </summary>
        /// <returns>True if can be completed, false otherwise</returns>
        public bool CanBeCompleted()
        {
            return !IsCompleted && IsActive;
        }

        /// <summary>
        /// Get completion duration if completed
        /// </summary>
        /// <returns>Duration between creation and completion, or null if not completed</returns>
        public TimeSpan? GetCompletionDuration()
        {
            if (!IsCompleted || CompletedAt == null)
                return null;

            return CompletedAt.Value - CreatedAt;
        }

        /// <summary>
        /// Get display text for completion status
        /// </summary>
        /// <returns>Status text</returns>
        public string GetStatusText()
        {
            if (!IsActive)
                return "Inactive";
            
            if (IsCompleted)
                return $"Completed at {CompletedAt:yyyy-MM-dd HH:mm}";
            
            return "Pending";
        }

        /// <summary>
        /// Validate the progress record
        /// </summary>
        /// <returns>List of validation errors</returns>
        public List<string> Validate()
        {
            var errors = new List<string>();

            if (TourSlotId == Guid.Empty)
                errors.Add("TourSlotId is required");

            if (TimelineItemId == Guid.Empty)
                errors.Add("TimelineItemId is required");

            if (IsCompleted && CompletedAt == null)
                errors.Add("CompletedAt is required when IsCompleted is true");

            if (CompletedAt.HasValue && CompletedAt.Value > DateTime.UtcNow)
                errors.Add("CompletedAt cannot be in the future");

            if (!string.IsNullOrEmpty(CompletionNotes) && CompletionNotes.Length > 500)
                errors.Add("CompletionNotes cannot exceed 500 characters");

            return errors;
        }

        /// <summary>
        /// Create a new progress record
        /// </summary>
        /// <param name="tourSlotId">Tour slot ID</param>
        /// <param name="timelineItemId">Timeline item ID</param>
        /// <param name="createdById">Creator user ID</param>
        /// <returns>New progress record</returns>
        public static TourSlotTimelineProgress Create(Guid tourSlotId, Guid timelineItemId, Guid createdById)
        {
            return new TourSlotTimelineProgress
            {
                Id = Guid.NewGuid(),
                TourSlotId = tourSlotId,
                TimelineItemId = timelineItemId,
                IsCompleted = false,
                CompletedAt = null,
                CompletionNotes = null,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createdById,
                IsActive = true
            };
        }

        /// <summary>
        /// Override ToString for debugging
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return $"TourSlotTimelineProgress: {Id} - TourSlot: {TourSlotId}, TimelineItem: {TimelineItemId}, Completed: {IsCompleted}";
        }
    }
}
