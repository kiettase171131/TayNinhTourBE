using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Utilities
{
    /// <summary>
    /// Validator cho schedule constraints của TourTemplate
    /// Đảm bảo chỉ cho phép Saturday OR Sunday, không cả hai
    /// </summary>
    public static class TourTemplateScheduleValidator
    {
        /// <summary>
        /// Validate schedule day constraint - chỉ cho phép Saturday OR Sunday
        /// </summary>
        /// <param name="scheduleDay">Ngày được chọn</param>
        /// <returns>Kết quả validation</returns>
        public static ScheduleValidationResult ValidateScheduleDay(ScheduleDay scheduleDay)
        {
            // Kiểm tra xem có phải là Saturday hoặc Sunday không
            if (scheduleDay != ScheduleDay.Saturday && scheduleDay != ScheduleDay.Sunday)
            {
                return new ScheduleValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Chỉ được chọn Thứ 7 (Saturday) hoặc Chủ nhật (Sunday) cho tour template",
                    ErrorCode = "INVALID_SCHEDULE_DAY"
                };
            }

            // Kiểm tra xem có phải là bitwise combination không (cả Saturday và Sunday)
            if (HasMultipleDays(scheduleDay))
            {
                return new ScheduleValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Chỉ được chọn một ngày duy nhất: Thứ 7 HOẶC Chủ nhật, không được chọn cả hai",
                    ErrorCode = "MULTIPLE_DAYS_NOT_ALLOWED"
                };
            }

            return new ScheduleValidationResult
            {
                IsValid = true,
                ErrorMessage = null,
                ErrorCode = null
            };
        }

        /// <summary>
        /// Kiểm tra xem ScheduleDay có chứa nhiều ngày không (bitwise combination)
        /// </summary>
        /// <param name="scheduleDay">Ngày cần kiểm tra</param>
        /// <returns>True nếu chứa nhiều ngày</returns>
        private static bool HasMultipleDays(ScheduleDay scheduleDay)
        {
            // Convert to int để kiểm tra bitwise
            int dayValue = (int)scheduleDay;
            
            // Kiểm tra xem có nhiều hơn 1 bit được set không
            return (dayValue & (dayValue - 1)) != 0;
        }

        /// <summary>
        /// Validate schedule day cho request tạo tour template
        /// </summary>
        /// <param name="scheduleDay">Ngày được chọn</param>
        /// <param name="templateType">Loại tour template</param>
        /// <returns>Kết quả validation với thông tin chi tiết</returns>
        public static ScheduleValidationResult ValidateScheduleDayForTemplate(ScheduleDay scheduleDay, TourTemplateType templateType)
        {
            // Validate basic schedule day constraint
            var basicValidation = ValidateScheduleDay(scheduleDay);
            if (!basicValidation.IsValid)
            {
                return basicValidation;
            }

            // Additional validation based on template type if needed
            // Hiện tại cả FreeScenic và PaidAttraction đều có cùng constraint
            // Có thể mở rộng trong tương lai nếu cần

            return new ScheduleValidationResult
            {
                IsValid = true,
                ErrorMessage = null,
                ErrorCode = null,
                ValidatedDay = scheduleDay,
                ValidatedDayName = scheduleDay.GetVietnameseName()
            };
        }

        /// <summary>
        /// Validate schedule day cho request generate slots
        /// </summary>
        /// <param name="scheduleDay">Ngày được chọn</param>
        /// <param name="month">Tháng</param>
        /// <param name="year">Năm</param>
        /// <returns>Kết quả validation với thông tin về số slots có thể tạo</returns>
        public static ScheduleValidationResult ValidateScheduleDayForSlotGeneration(ScheduleDay scheduleDay, int month, int year)
        {
            // Validate basic schedule day constraint
            var basicValidation = ValidateScheduleDay(scheduleDay);
            if (!basicValidation.IsValid)
            {
                return basicValidation;
            }

            // Tính số ngày weekend trong tháng cho ngày được chọn
            var weekendDatesCount = CountWeekendDatesInMonth(year, month, scheduleDay);
            
            if (weekendDatesCount == 0)
            {
                return new ScheduleValidationResult
                {
                    IsValid = false,
                    ErrorMessage = $"Không có ngày {scheduleDay.GetVietnameseName()} nào trong tháng {month}/{year}",
                    ErrorCode = "NO_WEEKEND_DATES_IN_MONTH"
                };
            }

            return new ScheduleValidationResult
            {
                IsValid = true,
                ErrorMessage = null,
                ErrorCode = null,
                ValidatedDay = scheduleDay,
                ValidatedDayName = scheduleDay.GetVietnameseName(),
                PossibleSlotsCount = Math.Min(weekendDatesCount, 4) // Tối đa 4 slots per month
            };
        }

        /// <summary>
        /// Đếm số ngày weekend trong một tháng cho ngày cụ thể
        /// </summary>
        /// <param name="year">Năm</param>
        /// <param name="month">Tháng</param>
        /// <param name="scheduleDay">Ngày cần đếm</param>
        /// <returns>Số ngày weekend</returns>
        private static int CountWeekendDatesInMonth(int year, int month, ScheduleDay scheduleDay)
        {
            var count = 0;
            var daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var dayOfWeek = date.DayOfWeek;

                // Convert DayOfWeek to ScheduleDay
                var currentScheduleDay = dayOfWeek switch
                {
                    DayOfWeek.Sunday => ScheduleDay.Sunday,
                    DayOfWeek.Monday => ScheduleDay.Monday,
                    DayOfWeek.Tuesday => ScheduleDay.Tuesday,
                    DayOfWeek.Wednesday => ScheduleDay.Wednesday,
                    DayOfWeek.Thursday => ScheduleDay.Thursday,
                    DayOfWeek.Friday => ScheduleDay.Friday,
                    DayOfWeek.Saturday => ScheduleDay.Saturday,
                    _ => ScheduleDay.Sunday
                };

                if (currentScheduleDay == scheduleDay)
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Lấy danh sách các ngày hợp lệ cho tour template
        /// </summary>
        /// <returns>Danh sách các ngày hợp lệ</returns>
        public static List<ScheduleDay> GetValidScheduleDays()
        {
            return new List<ScheduleDay> { ScheduleDay.Saturday, ScheduleDay.Sunday };
        }

        /// <summary>
        /// Lấy danh sách các ngày hợp lệ với tên tiếng Việt
        /// </summary>
        /// <returns>Dictionary với key là ScheduleDay và value là tên tiếng Việt</returns>
        public static Dictionary<ScheduleDay, string> GetValidScheduleDaysWithNames()
        {
            return GetValidScheduleDays().ToDictionary(day => day, day => day.GetVietnameseName());
        }
    }

    /// <summary>
    /// Kết quả validation cho schedule day
    /// </summary>
    public class ScheduleValidationResult
    {
        /// <summary>
        /// Có hợp lệ không
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Thông báo lỗi nếu không hợp lệ
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Mã lỗi
        /// </summary>
        public string? ErrorCode { get; set; }

        /// <summary>
        /// Ngày đã được validate (nếu hợp lệ)
        /// </summary>
        public ScheduleDay? ValidatedDay { get; set; }

        /// <summary>
        /// Tên tiếng Việt của ngày đã validate
        /// </summary>
        public string? ValidatedDayName { get; set; }

        /// <summary>
        /// Số slots có thể tạo (cho slot generation validation)
        /// </summary>
        public int? PossibleSlotsCount { get; set; }
    }
}
