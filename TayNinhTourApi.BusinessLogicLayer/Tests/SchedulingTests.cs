using TayNinhTourApi.BusinessLogicLayer.Services;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Tests
{
    /// <summary>
    /// Unit tests cho SchedulingService
    /// Test các chức năng date calculations và calendar edge cases
    /// </summary>
    public class SchedulingTests
    {
        private readonly SchedulingService _schedulingService;

        public SchedulingTests()
        {
            // Tạo SchedulingService với null dependencies cho testing
            _schedulingService = new SchedulingService(null, null, null);
        }

        #region CalculateWeekendDates Tests

        /// <summary>
        /// Test tính weekend dates cho tháng bình thường
        /// </summary>
        public void TestCalculateWeekendDates_NormalMonth_ReturnsCorrectDates()
        {
            // Arrange
            int year = 2025;
            int month = 1; // January 2025
            var scheduleDays = ScheduleDay.Saturday | ScheduleDay.Sunday;

            // Act
            var result = _schedulingService.CalculateWeekendDates(year, month, scheduleDays);

            // Assert
            var expectedDates = new List<DateOnly>
            {
                new DateOnly(2025, 1, 4),  // Saturday
                new DateOnly(2025, 1, 5),  // Sunday
                new DateOnly(2025, 1, 11), // Saturday
                new DateOnly(2025, 1, 12), // Sunday
                new DateOnly(2025, 1, 18), // Saturday
                new DateOnly(2025, 1, 19), // Sunday
                new DateOnly(2025, 1, 25), // Saturday
                new DateOnly(2025, 1, 26)  // Sunday
            };

            AssertDatesEqual(expectedDates, result);
        }

        /// <summary>
        /// Test tính weekend dates chỉ cho Saturday
        /// </summary>
        public void TestCalculateWeekendDates_SaturdayOnly_ReturnsOnlySaturdays()
        {
            // Arrange
            int year = 2025;
            int month = 1;
            var scheduleDays = ScheduleDay.Saturday;

            // Act
            var result = _schedulingService.CalculateWeekendDates(year, month, scheduleDays);

            // Assert
            var expectedDates = new List<DateOnly>
            {
                new DateOnly(2025, 1, 4),  // Saturday
                new DateOnly(2025, 1, 11), // Saturday
                new DateOnly(2025, 1, 18), // Saturday
                new DateOnly(2025, 1, 25)  // Saturday
            };

            AssertDatesEqual(expectedDates, result);
            AssertAllDatesAreSaturday(result);
        }

        /// <summary>
        /// Test tính weekend dates chỉ cho Sunday
        /// </summary>
        public void TestCalculateWeekendDates_SundayOnly_ReturnsOnlySundays()
        {
            // Arrange
            int year = 2025;
            int month = 1;
            var scheduleDays = ScheduleDay.Sunday;

            // Act
            var result = _schedulingService.CalculateWeekendDates(year, month, scheduleDays);

            // Assert
            var expectedDates = new List<DateOnly>
            {
                new DateOnly(2025, 1, 5),  // Sunday
                new DateOnly(2025, 1, 12), // Sunday
                new DateOnly(2025, 1, 19), // Sunday
                new DateOnly(2025, 1, 26)  // Sunday
            };

            AssertDatesEqual(expectedDates, result);
            AssertAllDatesAreSunday(result);
        }

        /// <summary>
        /// Test tính weekend dates cho tháng 2 năm nhuận
        /// </summary>
        public void TestCalculateWeekendDates_LeapYearFebruary_ReturnsCorrectDates()
        {
            // Arrange
            int year = 2024; // Leap year
            int month = 2;   // February
            var scheduleDays = ScheduleDay.Saturday | ScheduleDay.Sunday;

            // Act
            var result = _schedulingService.CalculateWeekendDates(year, month, scheduleDays);

            // Assert
            // February 2024 has 29 days
            AssertAllDatesInMonth(result, 2);
            AssertMaxDate(result, new DateOnly(2024, 2, 29));
        }

        #endregion

        #region GenerateSlotDates Tests

        /// <summary>
        /// Test generate slot dates với số lượng ít hơn weekend dates
        /// </summary>
        public void TestGenerateSlotDates_FewerSlotsThanWeekends_ReturnsOptimalDistribution()
        {
            // Arrange
            int year = 2025;
            int month = 1;
            var scheduleDays = ScheduleDay.Saturday | ScheduleDay.Sunday;
            int numberOfSlots = 4;

            // Act
            var result = _schedulingService.GenerateSlotDates(year, month, scheduleDays, numberOfSlots, false);

            // Assert
            AssertSlotCount(result, numberOfSlots);
            AssertDatesAreOrdered(result);
            AssertAllDatesInMonth(result, month);
        }

        /// <summary>
        /// Test generate slot dates với số lượng bằng weekend dates
        /// </summary>
        public void TestGenerateSlotDates_EqualSlotsToWeekends_ReturnsAllWeekends()
        {
            // Arrange
            int year = 2025;
            int month = 1;
            var scheduleDays = ScheduleDay.Saturday;
            int numberOfSlots = 4; // January 2025 has exactly 4 Saturdays

            // Act
            var result = _schedulingService.GenerateSlotDates(year, month, scheduleDays, numberOfSlots, false);

            // Assert
            AssertSlotCount(result, numberOfSlots);
            AssertAllDatesAreSaturday(result);
        }

        #endregion

        #region ValidateScheduleInput Tests

        /// <summary>
        /// Test validate với input hợp lệ
        /// </summary>
        public void TestValidateScheduleInput_ValidInput_ReturnsValid()
        {
            // Arrange
            int year = 2025;
            int month = 6;
            var scheduleDays = ScheduleDay.Saturday | ScheduleDay.Sunday;

            // Act
            var result = _schedulingService.ValidateScheduleInput(year, month, scheduleDays);

            // Assert
            AssertValidationSuccess(result);
        }

        /// <summary>
        /// Test validate với năm không hợp lệ
        /// </summary>
        public void TestValidateScheduleInput_InvalidYear_ReturnsInvalid()
        {
            // Arrange
            int year = 2023; // Too early
            int month = 6;

            // Act
            var result = _schedulingService.ValidateScheduleInput(year, month);

            // Assert
            AssertValidationFailure(result);
            AssertContainsValidationError(result, "Năm phải từ");
        }

        /// <summary>
        /// Test validate với tháng không hợp lệ
        /// </summary>
        public void TestValidateScheduleInput_InvalidMonth_ReturnsInvalid()
        {
            // Arrange
            int year = 2025;
            int month = 13; // Invalid month

            // Act
            var result = _schedulingService.ValidateScheduleInput(year, month);

            // Assert
            AssertValidationFailure(result);
            AssertContainsValidationError(result, "Tháng phải từ 1 đến 12");
        }

        #endregion

        #region Helper Methods

        private void AssertDatesEqual(List<DateOnly> expected, List<DateOnly> actual)
        {
            if (expected.Count != actual.Count)
                throw new Exception($"Expected {expected.Count} dates, but got {actual.Count}");

            for (int i = 0; i < expected.Count; i++)
            {
                if (expected[i] != actual[i])
                    throw new Exception($"Expected date {expected[i]} at index {i}, but got {actual[i]}");
            }
        }

        private void AssertAllDatesAreSaturday(List<DateOnly> dates)
        {
            foreach (var date in dates)
            {
                if (date.DayOfWeek != DayOfWeek.Saturday)
                    throw new Exception($"Expected Saturday, but {date} is {date.DayOfWeek}");
            }
        }

        private void AssertAllDatesAreSunday(List<DateOnly> dates)
        {
            foreach (var date in dates)
            {
                if (date.DayOfWeek != DayOfWeek.Sunday)
                    throw new Exception($"Expected Sunday, but {date} is {date.DayOfWeek}");
            }
        }

        private void AssertAllDatesInMonth(List<DateOnly> dates, int expectedMonth)
        {
            foreach (var date in dates)
            {
                if (date.Month != expectedMonth)
                    throw new Exception($"Expected month {expectedMonth}, but {date} is in month {date.Month}");
            }
        }

        private void AssertMaxDate(List<DateOnly> dates, DateOnly maxDate)
        {
            foreach (var date in dates)
            {
                if (date > maxDate)
                    throw new Exception($"Date {date} exceeds maximum allowed date {maxDate}");
            }
        }

        private void AssertSlotCount(List<DateOnly> dates, int expectedCount)
        {
            if (dates.Count != expectedCount)
                throw new Exception($"Expected {expectedCount} slots, but got {dates.Count}");
        }

        private void AssertDatesAreOrdered(List<DateOnly> dates)
        {
            for (int i = 1; i < dates.Count; i++)
            {
                if (dates[i] <= dates[i - 1])
                    throw new Exception($"Dates are not properly ordered: {dates[i - 1]} should be before {dates[i]}");
            }
        }

        private void AssertValidationSuccess(dynamic result)
        {
            if (!result.IsValid)
                throw new Exception($"Expected validation to succeed, but it failed: {result.Message}");
        }

        private void AssertValidationFailure(dynamic result)
        {
            if (result.IsValid)
                throw new Exception("Expected validation to fail, but it succeeded");
        }

        private void AssertContainsValidationError(dynamic result, string expectedError)
        {
            var errors = result.ValidationErrors as List<string>;
            if (errors == null || !errors.Any(e => e.Contains(expectedError)))
                throw new Exception($"Expected validation error containing '{expectedError}', but not found");
        }

        #endregion
    }
}
