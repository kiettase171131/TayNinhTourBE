using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho việc cập nhật tour template (đã đơn giản hóa)
    /// Tất cả fields đều optional để cho phép partial update
    /// </summary>
    public class RequestUpdateTourTemplateDto
    {
        [StringLength(200, ErrorMessage = "Tên template không được vượt quá 200 ký tự")]
        public string? Title { get; set; }



        public TourTemplateType? TemplateType { get; set; }

        public ScheduleDay? ScheduleDays { get; set; }

        [StringLength(500, ErrorMessage = "Điểm bắt đầu không được vượt quá 500 ký tự")]
        public string? StartLocation { get; set; }

        [StringLength(500, ErrorMessage = "Điểm kết thúc không được vượt quá 500 ký tự")]
        public string? EndLocation { get; set; }

        public List<string>? Images { get; set; }
    }
}
