using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho vi?c t?o tour template ngày l? v?i ngày c? th?
    /// Khác v?i template th??ng, holiday template ch? t?o 1 slot duy nh?t cho ngày ???c ch?n
    /// </summary>
    public class RequestCreateHolidayTourTemplateDto
    {
        [Required(ErrorMessage = "Vui lòng nh?p tên template")]
        [StringLength(200, ErrorMessage = "Tên template không ???c v??t quá 200 ký t?")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nh?p ?i?m b?t ??u")]
        [StringLength(500, ErrorMessage = "?i?m b?t ??u không ???c v??t quá 500 ký t?")]
        public string StartLocation { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nh?p ?i?m k?t thúc")]
        [StringLength(500, ErrorMessage = "?i?m k?t thúc không ???c v??t quá 500 ký t?")]
        public string EndLocation { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng ch?n th? lo?i tour")]
        public TourTemplateType TemplateType { get; set; }

        [Required(ErrorMessage = "Vui lòng ch?n ngày tour")]
        public DateOnly TourDate { get; set; }

        public List<string> Images { get; set; } = new List<string>();
    }
}