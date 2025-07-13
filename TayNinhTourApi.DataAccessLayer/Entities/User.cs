namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Avatar { get; set; } = null!;
        public string? TOtpSecret { get; set; }
        public bool IsVerified { get; set; }
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        public virtual ICollection<Tour> ToursCreated { get; set; } = new List<Tour>();
        public virtual ICollection<Tour> ToursUpdated { get; set; } = new List<Tour>();
        public virtual ICollection<TourTemplate> TourTemplatesCreated { get; set; } = new List<TourTemplate>();
        public virtual ICollection<TourTemplate> TourTemplatesUpdated { get; set; } = new List<TourTemplate>();

        public virtual ICollection<TourSlot> TourSlotsCreated { get; set; } = new List<TourSlot>();
        public virtual ICollection<TourSlot> TourSlotsUpdated { get; set; } = new List<TourSlot>();
        public virtual ICollection<TourDetails> TourDetailsCreated { get; set; } = new List<TourDetails>();
        public virtual ICollection<TourDetails> TourDetailsUpdated { get; set; } = new List<TourDetails>();
        public virtual ICollection<TourOperation> TourOperationsCreated { get; set; } = new List<TourOperation>();
        public virtual ICollection<TourOperation> TourOperationsUpdated { get; set; } = new List<TourOperation>();
        public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
        public virtual ICollection<BlogReaction> BlogReactions { get; set; } = new List<BlogReaction>();
        public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();
        public virtual ICollection<SupportTicket> TicketsCreated { get; set; } = new List<SupportTicket>();
        public virtual ICollection<SupportTicket> TicketsAssigned { get; set; } = new List<SupportTicket>();
        public virtual ICollection<SupportTicketComment> TicketComments { get; set; } = new List<SupportTicketComment>();

        public virtual ICollection<TourOperation> TourOperationsAsGuide { get; set; } = new List<TourOperation>();

        /// <summary>
        /// SpecialtyShop information if user has "Specialty Shop" role (1:1 relationship)
        /// </summary>
        public virtual SpecialtyShop? SpecialtyShop { get; set; }

        /// <summary>
        /// TourGuide information if user has "Tour Guide" role (1:1 relationship)
        /// </summary>
        public virtual TourGuide? TourGuide { get; set; }

        /// <summary>
        /// TourCompany information if user has "Tour Company" role (1:1 relationship)
        /// </summary>
        public virtual TourCompany? TourCompany { get; set; }

        /// <summary>
        /// Tour guides approved by this admin user
        /// </summary>
        public virtual ICollection<TourGuide> ApprovedTourGuides { get; set; } = new List<TourGuide>();
    }
}
