using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho vi?c t?o tour template ng�y l? v?i ng�y c? th?
    /// Kh�c v?i template th??ng, holiday template ch? t?o 1 slot duy nh?t cho ng�y ???c ch?n
    /// </summary>
    public class RequestCreateHolidayTourTemplateDto
    {
        [Required(ErrorMessage = "Vui l�ng nh?p t�n template")]
        [StringLength(200, ErrorMessage = "T�n template kh�ng ???c v??t qu� 200 k� t?")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Vui l�ng nh?p ?i?m b?t ??u")]
        [StringLength(500, ErrorMessage = "?i?m b?t ??u kh�ng ???c v??t qu� 500 k� t?")]
        public string StartLocation { get; set; } = null!;

        [Required(ErrorMessage = "Vui l�ng nh?p ?i?m k?t th�c")]
        [StringLength(500, ErrorMessage = "?i?m k?t th�c kh�ng ???c v??t qu� 500 k� t?")]
        public string EndLocation { get; set; } = null!;

        [Required(ErrorMessage = "Vui l�ng ch?n th? lo?i tour")]
        public TourTemplateType TemplateType { get; set; }

        [Required(ErrorMessage = "Vui l�ng ch?n ng�y tour")]
        public DateOnly TourDate { get; set; }

        public List<string> Images { get; set; } = new List<string>();
    }
}