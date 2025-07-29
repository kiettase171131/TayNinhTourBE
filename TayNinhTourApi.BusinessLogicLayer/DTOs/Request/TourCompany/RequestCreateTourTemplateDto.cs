using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO đơn giản cho việc tạo tour template mới
    /// Chỉ bao gồm các field cần thiết, các thông tin chi tiết sẽ được quản lý ở TourDetails
    /// </summary>
    public class RequestCreateTourTemplateDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên template")]
        [StringLength(200, ErrorMessage = "Tên template không được vượt quá 200 ký tự")]
        public string Title { get; set; } = null!;



        [Required(ErrorMessage = "Vui lòng nhập điểm bắt đầu")]
        [StringLength(500, ErrorMessage = "Điểm bắt đầu không được vượt quá 500 ký tự")]
        public string StartLocation { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập điểm kết thúc")]
        [StringLength(500, ErrorMessage = "Điểm kết thúc không được vượt quá 500 ký tự")]
        public string EndLocation { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng chọn thể loại tour")]
        public TourTemplateType TemplateType { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn thứ")]
        public ScheduleDay ScheduleDays { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tháng")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Month { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn năm")]
        [Range(2024, 2030, ErrorMessage = "Năm phải từ 2024 đến 2030")]
        public int Year { get; set; } = VietnamTimeZoneUtility.GetVietnamNow().Year;

        public List<string> Images { get; set; } = new List<string>();
    }
}
