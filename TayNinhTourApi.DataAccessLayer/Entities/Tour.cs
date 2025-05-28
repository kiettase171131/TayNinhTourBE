namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class Tour : BaseEntity
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int MaxGuests { get; set; }
        public string TourType { get; set; } = null!;
        public byte Status { get; set; }
        public bool IsApproved { get; set; }
        public string? CommentApproved { get; set; }
        public User CreatedBy { get; set; } = new User();
        public User? UpdatedBy { get; set; }
        public ICollection<Image> Images { get; set; } = new List<Image>();
    }
}
