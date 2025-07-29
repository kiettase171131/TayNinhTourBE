using AutoMapper;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation chuyên xử lý logic tính toán lịch trình và scheduling algorithm
    /// Kế thừa từ BaseService và implement ISchedulingService
    /// </summary>
    public class SchedulingService : BaseService, ISchedulingService
    {
        private readonly ILogger<SchedulingService> _logger;

        // Business constants for scheduling
        private const int MinYear = 2024;
        private const int MaxYear = 2030;
        private const int MaxSlotsPerMonth = 20;
        private const int DefaultSlotCount = 4;

        public SchedulingService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SchedulingService> logger) : base(mapper, unitOfWork)
        {
            _logger = logger;
        }

        /// <summary>
        /// Tính toán các ngày weekend trong một tháng cụ thể
        /// </summary>
        public List<DateOnly> CalculateWeekendDates(int year, int month, ScheduleDay scheduleDays)
        {
            _logger.LogInformation("Calculating weekend dates for {Year}/{Month} with schedule days: {ScheduleDays}",
                year, month, scheduleDays);

            var weekendDates = new List<DateOnly>();
            var daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateOnly(year, month, day);

                if (IsWeekendDate(date, scheduleDays))
                {
                    weekendDates.Add(date);
                }
            }

            var result = weekendDates.OrderBy(d => d).ToList();
            _logger.LogInformation("Found {Count} weekend dates in {Year}/{Month}", result.Count, year, month);

            return result;
        }

        /// <summary>
        /// Generate một số lượng cụ thể slot dates từ các ngày weekend
        /// </summary>
        public List<DateOnly> GenerateSlotDates(int year, int month, ScheduleDay scheduleDays, int numberOfSlots = 4, bool excludePastDates = true)
        {
            _logger.LogInformation("Generating {NumberOfSlots} slot dates for {Year}/{Month}", numberOfSlots, year, month);

            var allWeekendDates = CalculateWeekendDates(year, month, scheduleDays);

            if (excludePastDates)
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);
                allWeekendDates = allWeekendDates.Where(d => d >= today).ToList();
            }

            // If we have fewer weekend dates than requested slots, return all available
            if (allWeekendDates.Count <= numberOfSlots)
            {
                _logger.LogInformation("Available weekend dates ({Count}) <= requested slots ({NumberOfSlots}), returning all",
                    allWeekendDates.Count, numberOfSlots);
                return allWeekendDates;
            }

            // Use optimal distribution algorithm for better slot spacing
            return CalculateOptimalSlotDistribution(year, month, scheduleDays, numberOfSlots)
                .Where(d => excludePastDates ? d >= DateOnly.FromDateTime(VietnamTimeZoneUtility.GetVietnamNow()) : true)
                .Take(numberOfSlots)
                .ToList();
        }

        /// <summary>
        /// Validate input parameters cho scheduling operations
        /// </summary>
        public ResponseValidationDto ValidateScheduleInput(int year, int month, ScheduleDay? scheduleDays = null)
        {
            _logger.LogInformation("Validating schedule input: Year={Year}, Month={Month}, ScheduleDays={ScheduleDays}",
                year, month, scheduleDays);

            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200,
                ValidationErrors = new List<string>()
            };

            // Validate year
            if (year < MinYear || year > MaxYear)
            {
                result.IsValid = false;
                result.StatusCode = 400;
                result.ValidationErrors.Add($"Năm phải từ {MinYear} đến {MaxYear}");
            }

            // Validate month
            if (month < 1 || month > 12)
            {
                result.IsValid = false;
                result.StatusCode = 400;
                result.ValidationErrors.Add("Tháng phải từ 1 đến 12");
            }

            // Validate schedule days if provided
            if (scheduleDays.HasValue)
            {
                if (!IsValidScheduleDay(scheduleDays.Value))
                {
                    result.IsValid = false;
                    result.StatusCode = 400;
                    result.ValidationErrors.Add("ScheduleDay không hợp lệ");
                }
            }

            // Business rule: Cannot schedule for past months
            var currentDate = DateTime.UtcNow;
            if (year < currentDate.Year || (year == currentDate.Year && month < currentDate.Month))
            {
                result.IsValid = false;
                result.StatusCode = 400;
                result.ValidationErrors.Add("Không thể tạo lịch cho tháng đã qua");
            }

            if (!result.IsValid)
            {
                result.Message = "Dữ liệu đầu vào không hợp lệ";
                _logger.LogWarning("Schedule input validation failed: {Errors}", string.Join(", ", result.ValidationErrors));
            }
            else
            {
                result.Message = "Dữ liệu đầu vào hợp lệ";
                _logger.LogInformation("Schedule input validation passed");
            }

            return result;
        }

        /// <summary>
        /// Lấy danh sách các slot dates có thể booking trong tương lai
        /// </summary>
        public async Task<List<DateOnly>> GetNextAvailableSlots(Guid? tourTemplateId = null, int count = 10, DateOnly? startDate = null)
        {
            _logger.LogInformation("Getting next {Count} available slots for template {TemplateId} from {StartDate}",
                count, tourTemplateId, startDate);

            var searchStartDate = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var availableSlots = new List<DateOnly>();

            // Search through next 6 months to find available slots
            var currentDate = searchStartDate;
            var endSearchDate = currentDate.AddMonths(6);

            while (currentDate <= endSearchDate && availableSlots.Count < count)
            {
                // Get weekend dates for current month
                var weekendDates = CalculateWeekendDates(currentDate.Year, currentDate.Month,
                    ScheduleDay.Saturday | ScheduleDay.Sunday);

                // Filter dates that are >= searchStartDate
                var validDates = weekendDates.Where(d => d >= searchStartDate).ToList();

                if (tourTemplateId.HasValue)
                {
                    // Check existing slots for this template
                    var existingSlots = await _unitOfWork.TourSlotRepository.GetByTourTemplateAsync(tourTemplateId.Value);
                    var existingDates = existingSlots.Select(s => s.TourDate).ToHashSet();

                    // Exclude dates that already have slots
                    validDates = validDates.Where(d => !existingDates.Contains(d)).ToList();
                }

                availableSlots.AddRange(validDates);
                currentDate = new DateOnly(currentDate.Year, currentDate.Month, 1).AddMonths(1);
            }

            var result = availableSlots.Take(count).ToList();
            _logger.LogInformation("Found {Count} available slots", result.Count);

            return result;
        }

        /// <summary>
        /// Tính toán số lượng weekend dates trong một khoảng thời gian
        /// </summary>
        public int CountWeekendDatesInRange(DateOnly startDate, DateOnly endDate, ScheduleDay scheduleDays)
        {
            if (startDate > endDate)
            {
                _logger.LogWarning("Start date {StartDate} is after end date {EndDate}", startDate, endDate);
                return 0;
            }

            int count = 0;
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (IsWeekendDate(currentDate, scheduleDays))
                {
                    count++;
                }
                currentDate = currentDate.AddDays(1);
            }

            _logger.LogInformation("Counted {Count} weekend dates between {StartDate} and {EndDate}",
                count, startDate, endDate);

            return count;
        }

        /// <summary>
        /// Kiểm tra một ngày cụ thể có phải là weekend theo ScheduleDay không
        /// </summary>
        public bool IsWeekendDate(DateOnly date, ScheduleDay scheduleDays)
        {
            var dayOfWeek = date.DayOfWeek;

            // Chỉ check exact match với scheduleDays (không dùng HasFlag vì ScheduleDay không phải Flags enum)
            return (scheduleDays == ScheduleDay.Saturday && dayOfWeek == DayOfWeek.Saturday) ||
                   (scheduleDays == ScheduleDay.Sunday && dayOfWeek == DayOfWeek.Sunday);
        }

        /// <summary>
        /// Lấy ngày weekend tiếp theo từ một ngày cho trước
        /// </summary>
        public DateOnly? GetNextWeekendDate(DateOnly fromDate, ScheduleDay scheduleDays)
        {
            var currentDate = fromDate.AddDays(1); // Start from next day
            var maxSearchDays = 14; // Search up to 2 weeks ahead
            var searchCount = 0;

            while (searchCount < maxSearchDays)
            {
                if (IsWeekendDate(currentDate, scheduleDays))
                {
                    _logger.LogInformation("Next weekend date from {FromDate} is {NextDate}", fromDate, currentDate);
                    return currentDate;
                }

                currentDate = currentDate.AddDays(1);
                searchCount++;
            }

            _logger.LogWarning("No weekend date found within {MaxSearchDays} days from {FromDate}", maxSearchDays, fromDate);
            return null;
        }

        /// <summary>
        /// Tính toán distribution của slots trong tháng để đảm bảo phân bố đều
        /// </summary>
        public List<DateOnly> CalculateOptimalSlotDistribution(int year, int month, ScheduleDay scheduleDays, int targetSlots)
        {
            var allWeekendDates = CalculateWeekendDates(year, month, scheduleDays);

            if (allWeekendDates.Count <= targetSlots)
            {
                return allWeekendDates;
            }

            var result = new List<DateOnly>();
            var step = (double)allWeekendDates.Count / targetSlots;

            for (int i = 0; i < targetSlots; i++)
            {
                var index = (int)Math.Round(i * step);
                if (index >= allWeekendDates.Count)
                {
                    index = allWeekendDates.Count - 1;
                }

                if (!result.Contains(allWeekendDates[index]))
                {
                    result.Add(allWeekendDates[index]);
                }
            }

            _logger.LogInformation("Calculated optimal distribution: {Count} slots from {TotalWeekends} weekend dates",
                result.Count, allWeekendDates.Count);

            return result.OrderBy(d => d).ToList();
        }

        #region Private Helper Methods

        /// <summary>
        /// Kiểm tra ScheduleDay có hợp lệ không
        /// </summary>
        private bool IsValidScheduleDay(ScheduleDay scheduleDay)
        {
            // Check if it's a valid enum value (Sunday = 0 is valid!)
            return Enum.IsDefined(typeof(ScheduleDay), scheduleDay);
        }

        #endregion
    }
}
