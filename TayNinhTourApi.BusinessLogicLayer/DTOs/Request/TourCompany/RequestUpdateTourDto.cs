namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    public class RequestUpdateTourDto
    {
        public string? Title { get; set; } = null!;
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? MaxGuests { get; set; }
        public string? TourType { get; set; } = null!;
        public bool? IsActive { get; set; }
        public List<string>? Images { get; set; }
    }
}
