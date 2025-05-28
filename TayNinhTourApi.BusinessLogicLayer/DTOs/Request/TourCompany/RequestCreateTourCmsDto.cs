using System.ComponentModel.DataAnnotations;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    public class RequestCreateTourCmsDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng khách tối đa")]
        public int MaxGuests { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập loại tour")]
        public string TourType { get; set; } = null!;

        [Required(ErrorMessage = "Please select images")]
        public List<string> Images { get; set; } = new List<string>();
    }
}
