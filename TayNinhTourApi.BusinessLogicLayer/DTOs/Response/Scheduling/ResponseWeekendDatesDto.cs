using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Scheduling
{
    /// <summary>
    /// Response DTO cho weekend dates
    /// </summary>
    public class ResponseWeekendDatesDto : BaseResposeDto
    {
        /// <summary>
        /// Năm được tính toán
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Tháng được tính toán
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Tên tháng (tiếng Việt)
        /// </summary>
        public string MonthName { get; set; } = string.Empty;

        /// <summary>
        /// Danh sách các ngày weekend
        /// </summary>
        public List<WeekendDateInfo> WeekendDates { get; set; } = new();

        /// <summary>
        /// Tổng số ngày weekend trong tháng
        /// </summary>
        public int TotalWeekendDays { get; set; }

        /// <summary>
        /// Thông tin về ScheduleDay được áp dụng
        /// </summary>
        public string ScheduleDaysApplied { get; set; } = string.Empty;
    }

    /// <summary>
    /// Thông tin chi tiết về một ngày weekend
    /// </summary>
    public class WeekendDateInfo
    {
        /// <summary>
        /// Ngày
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// Tên ngày trong tuần (tiếng Việt)
        /// </summary>
        public string DayName { get; set; } = string.Empty;

        /// <summary>
        /// Ngày trong tuần (enum)
        /// </summary>
        public DayOfWeek DayOfWeek { get; set; }

        /// <summary>
        /// Có phải là ngày trong quá khứ không
        /// </summary>
        public bool IsPastDate { get; set; }

        /// <summary>
        /// Số thứ tự trong tháng (ví dụ: Chủ nhật thứ 2 trong tháng)
        /// </summary>
        public int WeekOfMonth { get; set; }
    }
}
