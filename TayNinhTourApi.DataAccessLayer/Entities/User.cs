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
        public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
        public virtual ICollection<BlogReaction> BlogReactions { get; set; } = new List<BlogReaction>();
        public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();

    }
}
