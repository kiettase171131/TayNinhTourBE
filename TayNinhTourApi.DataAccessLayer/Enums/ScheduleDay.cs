namespace TayNinhTourApi.DataAccessLayer.Enums
{
    /// <summary>
    /// Định nghĩa các ngày trong tuần cho việc lập lịch tour
    /// Hệ thống tập trung vào việc tạo tour vào cuối tuần (Saturday, Sunday)
    /// </summary>
    public enum ScheduleDay
    {
        /// <summary>
        /// Chủ nhật - ngày đầu tuần theo chuẩn ISO 8601
        /// </summary>
        Sunday = 0,

        /// <summary>
        /// Thứ hai
        /// </summary>
        Monday = 1,

        /// <summary>
        /// Thứ ba
        /// </summary>
        Tuesday = 2,

        /// <summary>
        /// Thứ tư
        /// </summary>
        Wednesday = 3,

        /// <summary>
        /// Thứ năm
        /// </summary>
        Thursday = 4,

        /// <summary>
        /// Thứ sáu
        /// </summary>
        Friday = 5,

        /// <summary>
        /// Thứ bảy - ngày cuối tuần chính cho tour
        /// </summary>
        Saturday = 6
    }

    /// <summary>
    /// Extension methods cho ScheduleDay enum
    /// </summary>
    public static class ScheduleDayExtensions
    {
        /// <summary>
        /// Kiểm tra xem ngày có phải là cuối tuần không
        /// </summary>
        /// <param name="day">Ngày cần kiểm tra</param>
        /// <returns>True nếu là Saturday hoặc Sunday</returns>
        public static bool IsWeekend(this ScheduleDay day)
        {
            return day == ScheduleDay.Saturday || day == ScheduleDay.Sunday;
        }

        /// <summary>
        /// Kiểm tra xem ngày có phải là ngày trong tuần không
        /// </summary>
        /// <param name="day">Ngày cần kiểm tra</param>
        /// <returns>True nếu là Monday đến Friday</returns>
        public static bool IsWeekday(this ScheduleDay day)
        {
            return !day.IsWeekend();
        }

        /// <summary>
        /// Lấy tên tiếng Việt của ngày
        /// </summary>
        /// <param name="day">Ngày cần lấy tên</param>
        /// <returns>Tên tiếng Việt của ngày</returns>
        public static string GetVietnameseName(this ScheduleDay day)
        {
            return day switch
            {
                ScheduleDay.Sunday => "Chủ nhật",
                ScheduleDay.Monday => "Thứ hai",
                ScheduleDay.Tuesday => "Thứ ba",
                ScheduleDay.Wednesday => "Thứ tư",
                ScheduleDay.Thursday => "Thứ năm",
                ScheduleDay.Friday => "Thứ sáu",
                ScheduleDay.Saturday => "Thứ bảy",
                _ => day.ToString()
            };
        }
    }
}
