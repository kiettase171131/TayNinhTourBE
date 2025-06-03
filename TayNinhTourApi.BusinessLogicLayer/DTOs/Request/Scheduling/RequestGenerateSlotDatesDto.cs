using System.ComponentModel.DataAnnotations;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Scheduling
{
    /// <summary>
    /// DTO cho request generate slot dates từ SchedulingService
    /// </summary>
    public class RequestGenerateSlotDatesDto
    {
        /// <summary>
        /// Năm cần generate slots (2024-2030)
        /// </summary>
        [Required(ErrorMessage = "Năm là bắt buộc")]
        [Range(2024, 2030, ErrorMessage = "Năm phải từ 2024 đến 2030")]
        public int Year { get; set; }

        /// <summary>
        /// Tháng cần generate slots (1-12)
        /// </summary>
        [Required(ErrorMessage = "Tháng là bắt buộc")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Month { get; set; }

        /// <summary>
        /// Các ngày trong tuần muốn generate (Saturday, Sunday hoặc cả hai)
        /// </summary>
        [Required(ErrorMessage = "ScheduleDays là bắt buộc")]
        public ScheduleDay ScheduleDays { get; set; }

        /// <summary>
        /// Số lượng slots cần generate (mặc định 4)
        /// </summary>
        [Range(1, 20, ErrorMessage = "Số lượng slots phải từ 1 đến 20")]
        public int NumberOfSlots { get; set; } = 4;

        /// <summary>
        /// Có loại bỏ các ngày đã qua không (mặc định true)
        /// </summary>
        public bool ExcludePastDates { get; set; } = true;
    }
}
