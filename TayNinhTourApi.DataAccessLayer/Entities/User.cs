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
        public virtual ICollection<Shop> ShopsCreated { get; set; } = new List<Shop>();
        public virtual ICollection<Shop> ShopsUpdated { get; set; } = new List<Shop>();
        public virtual ICollection<TourSlot> TourSlotsCreated { get; set; } = new List<TourSlot>();
        public virtual ICollection<TourSlot> TourSlotsUpdated { get; set; } = new List<TourSlot>();
        public virtual ICollection<TourDetails> TourDetailsCreated { get; set; } = new List<TourDetails>();
        public virtual ICollection<TourDetails> TourDetailsUpdated { get; set; } = new List<TourDetails>();
        public virtual ICollection<TourOperation> TourOperationsCreated { get; set; } = new List<TourOperation>();
        public virtual ICollection<TourOperation> TourOperationsUpdated { get; set; } = new List<TourOperation>();
        public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
        public virtual ICollection<TourOperation> TourOperationsAsGuide { get; set; } = new List<TourOperation>();
    }
}
