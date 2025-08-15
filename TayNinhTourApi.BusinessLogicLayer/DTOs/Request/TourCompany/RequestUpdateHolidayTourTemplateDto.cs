using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho vi?c c?p nh?t tour template ng�y l?
    /// Cho ph�p c?p nh?t m?t s? fields c?a holiday template ?� t?o
    /// </summary>
    public class RequestUpdateHolidayTourTemplateDto
    {
        [StringLength(200, ErrorMessage = "T�n template kh�ng ???c v??t qu� 200 k� t?")]
        public string? Title { get; set; }

        [StringLength(500, ErrorMessage = "?i?m b?t ??u kh�ng ???c v??t qu� 500 k� t?")]
        public string? StartLocation { get; set; }

        [StringLength(500, ErrorMessage = "?i?m k?t th�c kh�ng ???c v??t qu� 500 k� t?")]
        public string? EndLocation { get; set; }

        public TourTemplateType? TemplateType { get; set; }

        /// <summary>
        /// Ng�y tour m?i - n?u mu?n thay ??i ng�y
        /// Ph?i tu�n th? quy t?c 30 ng�y gi?ng nh? khi t?o m?i
        /// </summary>
        public DateOnly? TourDate { get; set; }

        public List<string>? Images { get; set; }
    }
}