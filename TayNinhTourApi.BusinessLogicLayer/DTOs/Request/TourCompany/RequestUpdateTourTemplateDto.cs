using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany
{
    /// <summary>
    /// DTO cho việc cập nhật tour template
    /// Tất cả fields đều optional để cho phép partial update
    /// </summary>
    public class RequestUpdateTourTemplateDto
    {
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string? Title { get; set; }

        [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá tour phải lớn hơn hoặc bằng 0")]
        public decimal? Price { get; set; }

        [Range(1, 1000, ErrorMessage = "Số lượng khách phải từ 1 đến 1000")]
        public int? MaxGuests { get; set; }

        [Range(0.5, 720, ErrorMessage = "Thời lượng tour phải từ 0.5 giờ đến 720 giờ")]
        public decimal? Duration { get; set; }

        public TourTemplateType? TemplateType { get; set; }

        public ScheduleDay? ScheduleDays { get; set; }

        [StringLength(500, ErrorMessage = "Điểm khởi hành không được vượt quá 500 ký tự")]
        public string? StartLocation { get; set; }

        [StringLength(500, ErrorMessage = "Điểm kết thúc không được vượt quá 500 ký tự")]
        public string? EndLocation { get; set; }

        [StringLength(1000, ErrorMessage = "Yêu cầu đặc biệt không được vượt quá 1000 ký tự")]
        public string? SpecialRequirements { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng khách tối thiểu phải lớn hơn 0")]
        public int? MinGuests { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Giá trẻ em phải lớn hơn hoặc bằng 0")]
        public decimal? ChildPrice { get; set; }

        [Range(0, 18, ErrorMessage = "Độ tuổi trẻ em phải từ 0 đến 18")]
        public int? ChildMaxAge { get; set; }

        [StringLength(200, ErrorMessage = "Thông tin phương tiện không được vượt quá 200 ký tự")]
        public string? Transportation { get; set; }

        [StringLength(500, ErrorMessage = "Thông tin bữa ăn không được vượt quá 500 ký tự")]
        public string? MealsIncluded { get; set; }

        [StringLength(500, ErrorMessage = "Thông tin chỗ ở không được vượt quá 500 ký tự")]
        public string? AccommodationInfo { get; set; }

        [StringLength(1000, ErrorMessage = "Dịch vụ bao gồm không được vượt quá 1000 ký tự")]
        public string? IncludedServices { get; set; }

        [StringLength(1000, ErrorMessage = "Dịch vụ không bao gồm không được vượt quá 1000 ký tự")]
        public string? ExcludedServices { get; set; }

        [StringLength(1000, ErrorMessage = "Chính sách hủy không được vượt quá 1000 ký tự")]
        public string? CancellationPolicy { get; set; }

        public List<string>? Images { get; set; }
    }
}
