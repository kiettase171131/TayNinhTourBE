using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    /// <summary>
    /// Operational TourGuide entity - stores active tour guide data after application approval
    /// This table contains tour guide information for day-to-day operations
    /// </summary>
    public class TourGuide : BaseEntity
    {
        /// <summary>
        /// Foreign Key to User table - One-to-One relationship
        /// Links the tour guide to their user account
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Foreign Key to TourGuideApplication table
        /// Links to the original approved application
        /// </summary>
        [Required]
        public Guid ApplicationId { get; set; }

        /// <summary>
        /// Full name of the tour guide
        /// Copied from approved application
        /// </summary>
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = null!;

        /// <summary>
        /// Contact phone number
        /// Copied from approved application
        /// </summary>
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = null!;

        /// <summary>
        /// Contact email address
        /// Copied from approved application
        /// </summary>
        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        /// <summary>
        /// Tour guide experience description
        /// Copied from approved application
        /// </summary>
        [Required]
        [StringLength(1000)]
        public string Experience { get; set; } = null!;

        /// <summary>
        /// Tour guide skills (comma-separated TourGuideSkill enum values)
        /// Copied from approved application
        /// </summary>
        [StringLength(500)]
        public string? Skills { get; set; }

        /// <summary>
        /// Average rating from tour participants
        /// Calculated from tour feedback
        /// </summary>
        public decimal Rating { get; set; } = 0.00m;

        /// <summary>
        /// Total number of tours guided
        /// Incremented when tour operations are completed
        /// </summary>
        public int TotalToursGuided { get; set; } = 0;

        /// <summary>
        /// Whether the tour guide is currently available for new tours
        /// Can be set by the guide or admin
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Additional notes about the tour guide
        /// Can be updated by admin or guide
        /// </summary>
        [StringLength(1000)]
        public string? Notes { get; set; }

        /// <summary>
        /// Tour guide's profile image URL
        /// Optional profile picture
        /// </summary>
        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        /// <summary>
        /// Date when the tour guide was approved and became active
        /// Set when the application is approved
        /// </summary>
        [Required]
        public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID of the admin who approved this tour guide
        /// </summary>
        [Required]
        public Guid ApprovedById { get; set; }
        /// <summary>
        /// Tổng số lượt được chấm sao
        /// </summary>
        public int RatingsCount { get; set; } = 0;


        // Navigation Properties

        /// <summary>
        /// User account associated with this tour guide
        /// One-to-One relationship
        /// </summary>
        public virtual User User { get; set; } = null!;

        /// <summary>
        /// Original approved application
        /// Many-to-One relationship (application can have only one approved guide)
        /// </summary>
        public virtual TourGuideApplication Application { get; set; } = null!;

        /// <summary>
        /// Admin who approved this tour guide
        /// Many-to-One relationship
        /// </summary>
        public virtual User ApprovedBy { get; set; } = null!;

        /// <summary>
        /// Tour operations where this guide is assigned
        /// One-to-Many relationship
        /// </summary>
        public virtual ICollection<TourOperation> TourOperations { get; set; } = new List<TourOperation>();

        /// <summary>
        /// Tour guide invitations for this guide
        /// One-to-Many relationship
        /// </summary>
        public virtual ICollection<TourGuideInvitation> Invitations { get; set; } = new List<TourGuideInvitation>();
        public virtual ICollection<TourFeedback> GuideFeedbacks { get; set; } = new List<TourFeedback>();

    }
}
