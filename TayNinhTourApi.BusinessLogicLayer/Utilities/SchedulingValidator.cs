using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Utilities
{
    /// <summary>
    /// Utility class cho validation business logic của Scheduling operations
    /// Cung cấp các static methods để validate input parameters và business rules
    /// </summary>
    public static class SchedulingValidator
    {
        // Business constants
        private const int MinYear = 2024;
        private const int MaxYear = 2030;
        private const int MaxSlotsPerRequest = 50;
        private const int MinSlotsPerRequest = 1;

        /// <summary>
        /// Validate basic scheduling input parameters
        /// </summary>
        /// <param name="year">Năm cần validate</param>
        /// <param name="month">Tháng cần validate</param>
        /// <returns>Kết quả validation</returns>
        public static ResponseValidationDto ValidateBasicScheduleInput(int year, int month)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            // Validate year
            if (year < MinYear || year > MaxYear)
            {
                AddValidationError(result, $"Năm phải từ {MinYear} đến {MaxYear}");
            }

            // Validate month
            if (month < 1 || month > 12)
            {
                AddValidationError(result, "Tháng phải từ 1 đến 12");
            }

            // Business rule: Cannot schedule for past months
            var currentDate = DateTime.UtcNow;
            if (year < currentDate.Year || (year == currentDate.Year && month < currentDate.Month))
            {
                AddValidationError(result, "Không thể tạo lịch cho tháng đã qua");
            }

            SetValidationResult(result);
            return result;
        }

        /// <summary>
        /// Validate schedule days parameter
        /// </summary>
        /// <param name="scheduleDays">ScheduleDay cần validate</param>
        /// <returns>Kết quả validation</returns>
        public static ResponseValidationDto ValidateScheduleDays(ScheduleDay scheduleDays)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            // Check if it's a valid enum value
            if (!Enum.IsDefined(typeof(ScheduleDay), scheduleDays))
            {
                AddValidationError(result, "ScheduleDay không hợp lệ");
            }

            // Check if at least one day is selected
            if (scheduleDays == 0)
            {
                AddValidationError(result, "Phải chọn ít nhất một ngày trong tuần");
            }

            // Business rule: For tour system, prefer weekend days
            if (!scheduleDays.HasFlag(ScheduleDay.Saturday) && !scheduleDays.HasFlag(ScheduleDay.Sunday))
            {
                // This is a warning, not an error
                result.ValidationErrors.Add("Khuyến nghị chọn ngày cuối tuần (Saturday hoặc Sunday) cho tour");
            }

            SetValidationResult(result);
            return result;
        }

        /// <summary>
        /// Validate slot generation parameters
        /// </summary>
        /// <param name="numberOfSlots">Số lượng slots cần generate</param>
        /// <param name="year">Năm</param>
        /// <param name="month">Tháng</param>
        /// <param name="scheduleDays">Ngày trong tuần</param>
        /// <returns>Kết quả validation</returns>
        public static ResponseValidationDto ValidateSlotGenerationRequest(int numberOfSlots, int year, int month, ScheduleDay scheduleDays)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            // Validate basic parameters first
            var basicValidation = ValidateBasicScheduleInput(year, month);
            if (!basicValidation.IsValid)
            {
                return basicValidation;
            }

            var scheduleDaysValidation = ValidateScheduleDays(scheduleDays);
            if (!scheduleDaysValidation.IsValid)
            {
                return scheduleDaysValidation;
            }

            // Validate number of slots
            if (numberOfSlots < MinSlotsPerRequest || numberOfSlots > MaxSlotsPerRequest)
            {
                AddValidationError(result, $"Số lượng slots phải từ {MinSlotsPerRequest} đến {MaxSlotsPerRequest}");
            }

            // Business rule: Check if requested slots is reasonable for the month
            var maxPossibleSlots = CalculateMaxPossibleSlotsInMonth(year, month, scheduleDays);
            if (numberOfSlots > maxPossibleSlots)
            {
                AddValidationError(result, $"Số lượng slots yêu cầu ({numberOfSlots}) vượt quá số ngày có thể trong tháng ({maxPossibleSlots})");
            }

            SetValidationResult(result);
            return result;
        }

        /// <summary>
        /// Validate date range for scheduling operations
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>Kết quả validation</returns>
        public static ResponseValidationDto ValidateDateRange(DateOnly startDate, DateOnly endDate)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            // Check if start date is before end date
            if (startDate > endDate)
            {
                AddValidationError(result, "Ngày bắt đầu phải trước ngày kết thúc");
            }

            // Check if date range is not too far in the future
            var maxFutureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2));
            if (endDate > maxFutureDate)
            {
                AddValidationError(result, "Không thể tạo lịch quá xa trong tương lai (tối đa 2 năm)");
            }

            // Check if date range is not too long
            var daysDifference = endDate.DayNumber - startDate.DayNumber;
            if (daysDifference > 365)
            {
                AddValidationError(result, "Khoảng thời gian không được vượt quá 1 năm");
            }

            SetValidationResult(result);
            return result;
        }

        /// <summary>
        /// Validate tour template ID for scheduling
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <returns>Kết quả validation</returns>
        public static ResponseValidationDto ValidateTourTemplateId(Guid tourTemplateId)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            if (tourTemplateId == Guid.Empty)
            {
                AddValidationError(result, "Tour Template ID không được để trống");
            }

            SetValidationResult(result);
            return result;
        }

        /// <summary>
        /// Comprehensive validation for scheduling request
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="year">Năm</param>
        /// <param name="month">Tháng</param>
        /// <param name="scheduleDays">Ngày trong tuần</param>
        /// <param name="numberOfSlots">Số lượng slots (optional)</param>
        /// <returns>Kết quả validation tổng hợp</returns>
        public static ResponseValidationDto ValidateCompleteSchedulingRequest(
            Guid tourTemplateId, 
            int year, 
            int month, 
            ScheduleDay scheduleDays, 
            int? numberOfSlots = null)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            // Validate tour template ID
            var templateValidation = ValidateTourTemplateId(tourTemplateId);
            if (!templateValidation.IsValid)
            {
                return templateValidation;
            }

            // Validate basic scheduling parameters
            if (numberOfSlots.HasValue)
            {
                var slotValidation = ValidateSlotGenerationRequest(numberOfSlots.Value, year, month, scheduleDays);
                if (!slotValidation.IsValid)
                {
                    return slotValidation;
                }
            }
            else
            {
                var basicValidation = ValidateBasicScheduleInput(year, month);
                if (!basicValidation.IsValid)
                {
                    return basicValidation;
                }

                var scheduleDaysValidation = ValidateScheduleDays(scheduleDays);
                if (!scheduleDaysValidation.IsValid)
                {
                    return scheduleDaysValidation;
                }
            }

            result.Message = "Tất cả tham số hợp lệ";
            return result;
        }

        #region Private Helper Methods

        /// <summary>
        /// Tính toán số lượng slots tối đa có thể trong tháng
        /// </summary>
        private static int CalculateMaxPossibleSlotsInMonth(int year, int month, ScheduleDay scheduleDays)
        {
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var count = 0;

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateOnly(year, month, day);
                var dayOfWeek = date.DayOfWeek;

                if ((scheduleDays.HasFlag(ScheduleDay.Saturday) && dayOfWeek == DayOfWeek.Saturday) ||
                    (scheduleDays.HasFlag(ScheduleDay.Sunday) && dayOfWeek == DayOfWeek.Sunday) ||
                    (scheduleDays.HasFlag(ScheduleDay.Monday) && dayOfWeek == DayOfWeek.Monday) ||
                    (scheduleDays.HasFlag(ScheduleDay.Tuesday) && dayOfWeek == DayOfWeek.Tuesday) ||
                    (scheduleDays.HasFlag(ScheduleDay.Wednesday) && dayOfWeek == DayOfWeek.Wednesday) ||
                    (scheduleDays.HasFlag(ScheduleDay.Thursday) && dayOfWeek == DayOfWeek.Thursday) ||
                    (scheduleDays.HasFlag(ScheduleDay.Friday) && dayOfWeek == DayOfWeek.Friday))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Thêm validation error vào result
        /// </summary>
        private static void AddValidationError(ResponseValidationDto result, string errorMessage)
        {
            result.IsValid = false;
            result.StatusCode = 400;
            result.ValidationErrors.Add(errorMessage);
        }

        /// <summary>
        /// Set final validation result
        /// </summary>
        private static void SetValidationResult(ResponseValidationDto result)
        {
            if (!result.IsValid)
            {
                result.Message = "Dữ liệu đầu vào không hợp lệ";
            }
            else
            {
                result.Message = "Dữ liệu đầu vào hợp lệ";
            }
        }

        #endregion
    }
}
