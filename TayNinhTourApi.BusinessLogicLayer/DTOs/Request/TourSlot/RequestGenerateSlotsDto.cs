using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot
{
    /// <summary>
    /// DTO cho request tạo tour slots tự động
    /// </summary>
    public class RequestGenerateSlotsDto
    {
        /// <summary>
        /// ID của tour template để tạo slots
        /// </summary>
        [Required(ErrorMessage = "Tour Template ID là bắt buộc")]
        public Guid TourTemplateId { get; set; }

        /// <summary>
        /// Tháng cần tạo slots (1-12)
        /// </summary>
        [Required(ErrorMessage = "Tháng là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Month { get; set; }

        /// <summary>
        /// Năm cần tạo slots
        /// </summary>
        [Required(ErrorMessage = "Năm là bắt buộc")]
        [Range(2024, 2030, ErrorMessage = "Năm phải từ 2024 đến 2030")]
        public int Year { get; set; }

        /// <summary>
        /// Các ngày trong tuần muốn tạo slots (Saturday, Sunday hoặc cả hai)
        /// Mặc định là cả Saturday và Sunday
        /// </summary>
        public ScheduleDay ScheduleDays { get; set; } = ScheduleDay.Saturday | ScheduleDay.Sunday;

        /// <summary>
        /// Có ghi đè các slots đã tồn tại không
        /// Mặc định là false - không ghi đè
        /// </summary>
        public bool OverwriteExisting { get; set; } = false;

        /// <summary>
        /// Có tự động activate các slots được tạo không
        /// Mặc định là true
        /// </summary>
        public bool AutoActivate { get; set; } = true;
    }
}
