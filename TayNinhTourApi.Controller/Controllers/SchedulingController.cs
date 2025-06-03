using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Scheduling;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Scheduling;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Tests;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho Scheduling Algorithm Service
    /// Cung cấp API endpoints để test và sử dụng các chức năng scheduling
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulingController : ControllerBase
    {
        private readonly ISchedulingService _schedulingService;
        private readonly ILogger<SchedulingController> _logger;

        public SchedulingController(
            ISchedulingService schedulingService,
            ILogger<SchedulingController> logger)
        {
            _schedulingService = schedulingService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách các ngày weekend trong một tháng cụ thể
        /// </summary>
        /// <param name="year">Năm (2024-2030)</param>
        /// <param name="month">Tháng (1-12)</param>
        /// <param name="scheduleDays">Các ngày trong tuần (Saturday=1, Sunday=2, Both=3)</param>
        /// <returns>Danh sách weekend dates với thông tin chi tiết</returns>
        [HttpGet("weekend-dates/{year}/{month}")]
        public ActionResult<ResponseWeekendDatesDto> GetWeekendDates(
            int year,
            int month,
            [FromQuery] ScheduleDay scheduleDays = ScheduleDay.Saturday | ScheduleDay.Sunday)
        {
            try
            {
                _logger.LogInformation("Getting weekend dates for {Year}/{Month} with schedule days: {ScheduleDays}",
                    year, month, scheduleDays);

                // Validate input
                var validation = _schedulingService.ValidateScheduleInput(year, month, scheduleDays);
                if (!validation.IsValid)
                {
                    return BadRequest(new ResponseWeekendDatesDto
                    {
                        IsSuccess = false,
                        Message = validation.Message,
                        StatusCode = validation.StatusCode
                    });
                }

                // Get weekend dates
                var weekendDates = _schedulingService.CalculateWeekendDates(year, month, scheduleDays);
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                // Build detailed response
                var weekendDateInfos = weekendDates.Select((date, index) => new WeekendDateInfo
                {
                    Date = date,
                    DayName = GetVietnameseDayName(date.DayOfWeek),
                    DayOfWeek = date.DayOfWeek,
                    IsPastDate = date < today,
                    WeekOfMonth = GetWeekOfMonth(date)
                }).ToList();

                var response = new ResponseWeekendDatesDto
                {
                    IsSuccess = true,
                    Message = $"Tìm thấy {weekendDates.Count} ngày weekend trong tháng {month}/{year}",
                    StatusCode = 200,
                    Year = year,
                    Month = month,
                    MonthName = CultureInfo.GetCultureInfo("vi-VN").DateTimeFormat.GetMonthName(month),
                    WeekendDates = weekendDateInfos,
                    TotalWeekendDays = weekendDates.Count,
                    ScheduleDaysApplied = GetScheduleDaysDescription(scheduleDays)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting weekend dates for {Year}/{Month}", year, month);
                return StatusCode(500, new ResponseWeekendDatesDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách ngày weekend",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Generate slot dates từ các ngày weekend trong tháng
        /// </summary>
        /// <param name="request">Thông tin để generate slots</param>
        /// <returns>Danh sách slot dates được chọn theo algorithm</returns>
        [HttpPost("generate-slot-dates")]
        public ActionResult<ResponseGenerateSlotDatesDto> GenerateSlotDates([FromBody] RequestGenerateSlotDatesDto request)
        {
            try
            {
                _logger.LogInformation("Generating {NumberOfSlots} slot dates for {Year}/{Month}",
                    request.NumberOfSlots, request.Year, request.Month);

                // Validate input
                var validation = _schedulingService.ValidateScheduleInput(request.Year, request.Month, request.ScheduleDays);
                if (!validation.IsValid)
                {
                    return BadRequest(new ResponseGenerateSlotDatesDto
                    {
                        IsSuccess = false,
                        Message = validation.Message,
                        StatusCode = validation.StatusCode
                    });
                }

                // Get all weekend dates first for comparison
                var allWeekendDates = _schedulingService.CalculateWeekendDates(request.Year, request.Month, request.ScheduleDays);

                // Generate slot dates
                var slotDates = _schedulingService.GenerateSlotDates(
                    request.Year,
                    request.Month,
                    request.ScheduleDays,
                    request.NumberOfSlots,
                    request.ExcludePastDates);

                // Build detailed response
                var slotDateInfos = slotDates.Select((date, index) => new SlotDateInfo
                {
                    Date = date,
                    DayName = GetVietnameseDayName(date.DayOfWeek),
                    DayOfWeek = date.DayOfWeek,
                    SlotOrder = index + 1,
                    DaysFromPreviousSlot = index > 0 ? (date.DayNumber - slotDates[index - 1].DayNumber) : null,
                    IsOptimallyDistributed = slotDates.Count < allWeekendDates.Count
                }).ToList();

                var algorithmUsed = slotDates.Count == allWeekendDates.Count
                    ? "All Available Dates"
                    : "Optimal Distribution Algorithm";

                var response = new ResponseGenerateSlotDatesDto
                {
                    IsSuccess = true,
                    Message = $"Đã generate thành công {slotDates.Count} slot dates",
                    StatusCode = 200,
                    Year = request.Year,
                    Month = request.Month,
                    RequestedSlots = request.NumberOfSlots,
                    GeneratedSlots = slotDates.Count,
                    SlotDates = slotDateInfos,
                    ExcludedPastDates = request.ExcludePastDates,
                    AlgorithmUsed = algorithmUsed,
                    TotalAvailableWeekendDates = allWeekendDates.Count
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating slot dates");
                return StatusCode(500, new ResponseGenerateSlotDatesDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi generate slot dates",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Validate input parameters cho scheduling operations
        /// </summary>
        /// <param name="request">Thông tin cần validate</param>
        /// <returns>Kết quả validation với chi tiết lỗi nếu có</returns>
        [HttpPost("validate")]
        public ActionResult<ResponseValidationDto> ValidateScheduleInput([FromBody] RequestValidateScheduleDto request)
        {
            try
            {
                _logger.LogInformation("Validating schedule input: Year={Year}, Month={Month}, ScheduleDays={ScheduleDays}",
                    request.Year, request.Month, request.ScheduleDays);

                var validation = _schedulingService.ValidateScheduleInput(request.Year, request.Month, request.ScheduleDays);

                return StatusCode(validation.StatusCode, validation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating schedule input");
                return StatusCode(500, new ResponseValidationDto
                {
                    IsValid = false,
                    Message = "Có lỗi xảy ra khi validate input",
                    StatusCode = 500,
                    ValidationErrors = new List<string> { ex.Message }
                });
            }
        }

        /// <summary>
        /// Lấy danh sách các slot dates có thể booking trong tương lai
        /// </summary>
        /// <param name="request">Thông tin để tìm available slots</param>
        /// <returns>Danh sách slot dates available cho booking</returns>
        [HttpPost("next-available-slots")]
        public async Task<ActionResult<ResponseNextAvailableSlotsDto>> GetNextAvailableSlots([FromBody] RequestNextAvailableSlotsDto request)
        {
            try
            {
                var startTime = DateTime.UtcNow;
                _logger.LogInformation("Getting next {Count} available slots for template {TemplateId} from {StartDate}",
                    request.Count, request.TourTemplateId, request.StartDate);

                var availableSlots = await _schedulingService.GetNextAvailableSlots(
                    request.TourTemplateId,
                    request.Count,
                    request.StartDate);

                var searchEndDate = (request.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow)).AddMonths(6);
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                // Build detailed response
                var availableSlotInfos = availableSlots.Select(date => new AvailableSlotInfo
                {
                    Date = date,
                    DayName = GetVietnameseDayName(date.DayOfWeek),
                    DayOfWeek = date.DayOfWeek,
                    Month = date.Month,
                    Year = date.Year,
                    DaysFromToday = date.DayNumber - today.DayNumber,
                    HasExistingSlot = false // This would need additional logic to check
                }).ToList();

                var searchTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

                var response = new ResponseNextAvailableSlotsDto
                {
                    IsSuccess = true,
                    Message = $"Tìm thấy {availableSlots.Count} available slots",
                    StatusCode = 200,
                    TourTemplateId = request.TourTemplateId,
                    RequestedCount = request.Count,
                    FoundCount = availableSlots.Count,
                    SearchStartDate = request.StartDate ?? today,
                    SearchEndDate = searchEndDate,
                    AvailableSlots = availableSlotInfos,
                    HasEnoughSlots = availableSlots.Count >= request.Count,
                    SearchInfo = new SearchInfo
                    {
                        MonthsSearched = 6,
                        TotalWeekendDatesChecked = 0, // Would need calculation
                        ExcludedExistingSlots = 0, // Would need calculation
                        SearchTimeMs = (long)searchTime
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next available slots");
                return StatusCode(500, new ResponseNextAvailableSlotsDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy available slots",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Tính toán optimal distribution của slots trong tháng
        /// </summary>
        /// <param name="request">Thông tin để tính optimal distribution</param>
        /// <returns>Danh sách ngày được phân bố optimal</returns>
        [HttpPost("optimal-distribution")]
        public ActionResult<ResponseOptimalDistributionDto> CalculateOptimalDistribution([FromBody] RequestOptimalDistributionDto request)
        {
            try
            {
                _logger.LogInformation("Calculating optimal distribution for {Year}/{Month} with {TargetSlots} target slots",
                    request.Year, request.Month, request.TargetSlots);

                // Validate input
                var validation = _schedulingService.ValidateScheduleInput(request.Year, request.Month, request.ScheduleDays);
                if (!validation.IsValid)
                {
                    return BadRequest(new ResponseOptimalDistributionDto
                    {
                        IsSuccess = false,
                        Message = validation.Message,
                        StatusCode = validation.StatusCode
                    });
                }

                // Get all weekend dates and optimal distribution
                var allWeekendDates = _schedulingService.CalculateWeekendDates(request.Year, request.Month, request.ScheduleDays);
                var optimalSlots = _schedulingService.CalculateOptimalSlotDistribution(request.Year, request.Month, request.ScheduleDays, request.TargetSlots);

                var usedOptimalDistribution = optimalSlots.Count < allWeekendDates.Count;

                // Build detailed response
                var optimalSlotInfos = optimalSlots.Select((date, index) => new OptimalSlotInfo
                {
                    Date = date,
                    DayName = GetVietnameseDayName(date.DayOfWeek),
                    DayOfWeek = date.DayOfWeek,
                    OptimalOrder = index + 1,
                    OriginalIndex = allWeekendDates.IndexOf(date),
                    DaysFromPreviousSlot = index > 0 ? (date.DayNumber - optimalSlots[index - 1].DayNumber) : null,
                    DaysToNextSlot = index < optimalSlots.Count - 1 ? (optimalSlots[index + 1].DayNumber - date.DayNumber) : null,
                    IsCalculatedStep = usedOptimalDistribution
                }).ToList();

                // Calculate algorithm info
                var distances = optimalSlotInfos.Where(s => s.DaysFromPreviousSlot.HasValue)
                                               .Select(s => s.DaysFromPreviousSlot!.Value)
                                               .ToList();

                var averageDistance = distances.Any() ? distances.Average() : 0;
                var standardDeviation = distances.Any() ? Math.Sqrt(distances.Average(d => Math.Pow(d - averageDistance, 2))) : 0;

                var response = new ResponseOptimalDistributionDto
                {
                    IsSuccess = true,
                    Message = $"Đã tính toán optimal distribution thành công",
                    StatusCode = 200,
                    Year = request.Year,
                    Month = request.Month,
                    TargetSlots = request.TargetSlots,
                    ActualSlots = optimalSlots.Count,
                    TotalAvailableWeekendDates = allWeekendDates.Count,
                    OptimalSlots = optimalSlotInfos,
                    UsedOptimalDistribution = usedOptimalDistribution,
                    AlgorithmInfo = new DistributionAlgorithmInfo
                    {
                        CalculatedStep = usedOptimalDistribution ? (double)allWeekendDates.Count / request.TargetSlots : 1.0,
                        DistributionMethod = usedOptimalDistribution ? "Step-based Optimal Distribution" : "All Available Dates",
                        AverageDistanceBetweenSlots = averageDistance,
                        DistanceStandardDeviation = standardDeviation,
                        IsEvenlyDistributed = standardDeviation <= 2.0 // Arbitrary threshold
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating optimal distribution");
                return StatusCode(500, new ResponseOptimalDistributionDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tính optimal distribution",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Chạy unit tests cho SchedulingService
        /// </summary>
        /// <returns>Kết quả của tất cả unit tests</returns>
        [HttpGet("run-tests")]
        public ActionResult<object> RunTests()
        {
            try
            {
                _logger.LogInformation("Running SchedulingService unit tests");

                var results = TestRunner.RunAllTests();

                var response = new
                {
                    IsSuccess = results.FailedTests == 0,
                    Message = results.FailedTests == 0
                        ? "Tất cả tests đã pass thành công"
                        : $"Có {results.FailedTests} tests failed",
                    StatusCode = results.FailedTests == 0 ? 200 : 500,
                    TestSummary = new
                    {
                        TotalTests = results.TotalTests,
                        PassedTests = results.PassedTests,
                        FailedTests = results.FailedTests,
                        SuccessRate = $"{results.SuccessRate:P2}",
                        TotalExecutionTime = $"{results.TotalExecutionTime.TotalMilliseconds:F2}ms"
                    },
                    PassedTests = results.PassedTestResults.Select(t => new
                    {
                        TestName = t.TestName,
                        ExecutionTime = $"{t.ExecutionTime.TotalMilliseconds:F2}ms"
                    }).ToList(),
                    FailedTests = results.FailedTestResults.Select(t => new
                    {
                        TestName = t.TestName,
                        ErrorMessage = t.ErrorMessage,
                        ExecutionTime = $"{t.ExecutionTime.TotalMilliseconds:F2}ms"
                    }).ToList()
                };

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running SchedulingService tests");
                return StatusCode(500, new
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi chạy tests",
                    StatusCode = 500,
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Chạy một test cụ thể
        /// </summary>
        /// <param name="testName">Tên của test method</param>
        /// <returns>Kết quả của test cụ thể</returns>
        [HttpGet("run-test/{testName}")]
        public ActionResult<object> RunSpecificTest(string testName)
        {
            try
            {
                _logger.LogInformation("Running specific test: {TestName}", testName);

                var result = TestRunner.RunSpecificTest(testName);

                var response = new
                {
                    IsSuccess = result.Passed,
                    Message = result.Passed
                        ? $"Test '{testName}' đã pass thành công"
                        : $"Test '{testName}' failed",
                    StatusCode = result.Passed ? 200 : 500,
                    TestResult = new
                    {
                        TestName = result.TestName,
                        Passed = result.Passed,
                        ErrorMessage = result.ErrorMessage,
                        ExecutionTime = $"{result.ExecutionTime.TotalMilliseconds:F2}ms"
                    }
                };

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running specific test: {TestName}", testName);
                return StatusCode(500, new
                {
                    IsSuccess = false,
                    Message = $"Có lỗi xảy ra khi chạy test '{testName}'",
                    StatusCode = 500,
                    Error = ex.Message
                });
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Lấy tên tiếng Việt của ngày trong tuần
        /// </summary>
        private string GetVietnameseDayName(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ nhật",
                _ => dayOfWeek.ToString()
            };
        }

        /// <summary>
        /// Tính tuần thứ mấy trong tháng
        /// </summary>
        private int GetWeekOfMonth(DateOnly date)
        {
            var firstDayOfMonth = new DateOnly(date.Year, date.Month, 1);
            var firstWeekday = (int)firstDayOfMonth.DayOfWeek;
            return (date.Day + firstWeekday - 1) / 7 + 1;
        }

        /// <summary>
        /// Mô tả ScheduleDays bằng tiếng Việt
        /// </summary>
        private string GetScheduleDaysDescription(ScheduleDay scheduleDays)
        {
            var descriptions = new List<string>();

            if (scheduleDays.HasFlag(ScheduleDay.Saturday))
                descriptions.Add("Thứ 7");

            if (scheduleDays.HasFlag(ScheduleDay.Sunday))
                descriptions.Add("Chủ nhật");

            return string.Join(" và ", descriptions);
        }

        #endregion
    }
}
