using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany
{
    public class TourDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int MaxGuests { get; set; }
        public string TourType { get; set; } = null!;
        public byte Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsApproved { get; set; }
        public string? CommentApproved { get; set; }
        public List<string> Images { get; set; } = new List<string>();
        public UserCmsDto? CreatedBy { get; set; }
        public UserCmsDto? UpdatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
