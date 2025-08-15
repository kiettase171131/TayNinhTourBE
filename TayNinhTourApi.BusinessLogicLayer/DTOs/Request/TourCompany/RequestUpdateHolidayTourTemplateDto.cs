using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho vi?c c?p nh?t tour template ngày l?
    /// Cho phép c?p nh?t m?t s? fields c?a holiday template ?ã t?o
    /// </summary>
    public class RequestUpdateHolidayTourTemplateDto
    {
        [StringLength(200, ErrorMessage = "Tên template không ???c v??t quá 200 ký t?")]
        public string? Title { get; set; }

        [StringLength(500, ErrorMessage = "?i?m b?t ??u không ???c v??t quá 500 ký t?")]
        public string? StartLocation { get; set; }

        [StringLength(500, ErrorMessage = "?i?m k?t thúc không ???c v??t quá 500 ký t?")]
        public string? EndLocation { get; set; }

        public TourTemplateType? TemplateType { get; set; }

        /// <summary>
        /// Ngày tour m?i - n?u mu?n thay ??i ngày
        /// Ph?i tuân th? quy t?c 30 ngày gi?ng nh? khi t?o m?i
        /// </summary>
        public DateOnly? TourDate { get; set; }

        public List<string>? Images { get; set; }
    }
}