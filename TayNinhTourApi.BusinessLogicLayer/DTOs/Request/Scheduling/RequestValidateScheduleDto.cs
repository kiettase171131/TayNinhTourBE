using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Scheduling
{
    /// <summary>
    /// DTO cho request validate schedule input
    /// </summary>
    public class RequestValidateScheduleDto
    {
        /// <summary>
        /// Năm cần validate
        /// </summary>
        [Required(ErrorMessage = "Năm là bắt buộc")]
        public int Year { get; set; }

        /// <summary>
        /// Tháng cần validate
        /// </summary>
        [Required(ErrorMessage = "Tháng là bắt buộc")]
        public int Month { get; set; }

        /// <summary>
        /// Ngày trong tuần cần validate (optional)
        /// </summary>
        public ScheduleDay? ScheduleDays { get; set; }
    }
}
