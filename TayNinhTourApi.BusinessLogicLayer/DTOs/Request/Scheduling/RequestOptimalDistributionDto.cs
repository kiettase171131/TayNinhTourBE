using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Scheduling
{
    /// <summary>
    /// DTO cho request tính optimal distribution
    /// </summary>
    public class RequestOptimalDistributionDto
    {
        /// <summary>
        /// Năm
        /// </summary>
        [Required(ErrorMessage = "Năm là bắt buộc")]
        [Range(2024, 2030, ErrorMessage = "Năm phải từ 2024 đến 2030")]
        public int Year { get; set; }

        /// <summary>
        /// Tháng
        /// </summary>
        [Required(ErrorMessage = "Tháng là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Month { get; set; }

        /// <summary>
        /// Ngày trong tuần
        /// </summary>
        [Required(ErrorMessage = "ScheduleDays là bắt buộc")]
        public ScheduleDay ScheduleDays { get; set; }

        /// <summary>
        /// Số slots mục tiêu
        /// </summary>
        [Required(ErrorMessage = "TargetSlots là bắt buộc")]
        [Range(1, 20, ErrorMessage = "Số slots mục tiêu phải từ 1 đến 20")]
        public int TargetSlots { get; set; }
    }
}
