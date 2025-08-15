using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Utilities
{
    /// <summary>
    /// Validator ri�ng cho Holiday Tour Template
    /// Kh�c v?i TourTemplateValidator, class n�y cho ph�p ch?n b?t k? ng�y n�o trong tu?n
    /// kh�ng ch? Saturday/Sunday
    /// </summary>
    public static class HolidayTourTemplateValidator
    {
        /// <summary>
        /// Validate holiday tour template creation request
        /// Cho ph�p ch?n b?t k? ng�y n�o trong tu?n (Monday - Sunday)
        /// </summary>
        public static ResponseValidationDto ValidateCreateRequest(RequestCreateHolidayTourTemplateDto request)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200,
                ValidationErrors = new List<string>(),
                FieldErrors = new Dictionary<string, List<string>>()
            };

            // Title validation
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                AddFieldError(result, nameof(request.Title), "T�n template l� b?t bu?c");
            }
            else if (request.Title.Length > 200)
            {
                AddFieldError(result, nameof(request.Title), "T�n template kh�ng ???c v??t qu� 200 k� t?");
            }

            // Location validation
            if (string.IsNullOrWhiteSpace(request.StartLocation))
            {
                AddFieldError(result, nameof(request.StartLocation), "?i?m b?t ??u l� b?t bu?c");
            }
            else if (request.StartLocation.Length > 500)
            {
                AddFieldError(result, nameof(request.StartLocation), "?i?m b?t ??u kh�ng ???c v??t qu� 500 k� t?");
            }

            if (string.IsNullOrWhiteSpace(request.EndLocation))
            {
                AddFieldError(result, nameof(request.EndLocation), "?i?m k?t th�c l� b?t bu?c");
            }
            else if (request.EndLocation.Length > 500)
            {
                AddFieldError(result, nameof(request.EndLocation), "?i?m k?t th�c kh�ng ???c v??t qu� 500 k� t?");
            }

            // Tour date validation - Apply business rules for holiday templates
            var currentTime = VietnamTimeZoneUtility.GetVietnamNow();
            var tourDateTime = request.TourDate.ToDateTime(TimeOnly.MinValue);

            // Rule 1: Tour date must be in the future
            if (request.TourDate <= DateOnly.FromDateTime(currentTime))
            {
                AddFieldError(result, nameof(request.TourDate), "Ng�y tour ph?i l� ng�y trong t??ng lai");
            }

            // Rule 2: Apply the same 30-day rule as regular templates
            var minimumDate = currentTime.AddDays(30);
            if (tourDateTime < minimumDate)
            {
                var suggestedDate = minimumDate.AddDays(7); // Add 7 more days for safety
                AddFieldError(result, nameof(request.TourDate), 
                    $"Ng�y tour ph?i sau �t nh?t 30 ng�y t? ng�y t?o ({currentTime:dd/MM/yyyy}). " +
                    $"Ng�y s?m nh?t c� th?: {minimumDate:dd/MM/yyyy}. " +
                    $"G?i �: Ch?n ng�y {suggestedDate:dd/MM/yyyy} ho?c mu?n h?n. " +
                    $"V� d? JSON h?p l?: \"tourDate\": \"{suggestedDate:yyyy-MM-dd}\"");
            }

            // Rule 3: Tour date should not be too far in the future (2 years max)
            var maxFutureDate = DateOnly.FromDateTime(currentTime.AddYears(2));
            if (request.TourDate > maxFutureDate)
            {
                AddFieldError(result, nameof(request.TourDate), 
                    $"Ng�y tour kh�ng ???c qu� 2 n?m trong t??ng lai. " +
                    $"Ng�y mu?n nh?t c� th?: {maxFutureDate:dd/MM/yyyy}");
            }

            // Rule 4: Validate year range
            if (request.TourDate.Year < 2024 || request.TourDate.Year > 2030)
            {
                AddFieldError(result, nameof(request.TourDate), "N?m c?a ng�y tour ph?i t? 2024 ??n 2030");
            }

            // Template type validation
            if (!Enum.IsDefined(typeof(TourTemplateType), request.TemplateType))
            {
                AddFieldError(result, nameof(request.TemplateType), "Lo?i tour template kh�ng h?p l?");
            }

            // Set validation result
            result.IsValid = !result.FieldErrors.Any();
            if (!result.IsValid)
            {
                result.StatusCode = 400;
                result.Message = "D? li?u kh�ng h?p l? - Vui l�ng ki?m tra v� s?a c�c l?i sau";
                result.ValidationErrors = result.FieldErrors.SelectMany(x => x.Value).ToList();
                
                // Add helpful guidance for holiday template
                result.ValidationErrors.Add("?? H??NG D?N HOLIDAY TEMPLATE:");
                result.ValidationErrors.Add($"� Ng�y hi?n t?i: {currentTime:dd/MM/yyyy} - KH�NG th? ch?n");
                result.ValidationErrors.Add($"� Ng�y s?m nh?t: {minimumDate:dd/MM/yyyy} (sau 30 ng�y)");
                result.ValidationErrors.Add($"� Ng�y mu?n nh?t: {maxFutureDate:dd/MM/yyyy} (t?i ?a 2 n?m)");
                result.ValidationErrors.Add($"� V� d? JSON h?p l?: {{\"tourDate\": \"{minimumDate.AddDays(7):yyyy-MM-dd}\"}}");
                result.ValidationErrors.Add("� ? ??C BI?T: Holiday template c� th? ch?n b?t k? ng�y n�o trong tu?n (Monday-Sunday)");
                result.ValidationErrors.Add("� ? Kh�c v?i template th??ng ch? cho ph�p Saturday/Sunday");
            }

            return result;
        }

        /// <summary>
        /// Validate holiday tour template business rules
        /// Cho ph�p t?t c? c�c ng�y trong tu?n (Monday - Sunday)
        /// </summary>
        public static ResponseValidationDto ValidateHolidayBusinessRules(TourTemplate template)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200,
                ValidationErrors = new List<string>(),
                FieldErrors = new Dictionary<string, List<string>>()
            };

            // Validate template type
            if (!Enum.IsDefined(typeof(TourTemplateType), template.TemplateType))
            {
                AddFieldError(result, "TemplateType", "Lo?i tour template kh�ng h?p l?");
            }

            // Validate Month/Year combination
            if (template.Month < 1 || template.Month > 12)
            {
                AddFieldError(result, "Month", "Th�ng ph?i t? 1 ??n 12");
            }

            if (template.Year < 2024 || template.Year > 2030)
            {
                AddFieldError(result, "Year", "N?m ph?i t? 2024 ??n 2030");
            }

            // Validate schedule day - ALLOW ALL DAYS OF WEEK for holiday template
            // This is the key difference from regular template validator
            if (!Enum.IsDefined(typeof(ScheduleDay), template.ScheduleDays))
            {
                AddFieldError(result, "ScheduleDays", "Ng�y trong tu?n kh�ng h?p l?");
            }
            // NO restriction on Saturday/Sunday only - all days are allowed

            // Validate slot date for holiday template
            var tourDate = new DateTime(template.Year, template.Month, 1);
            var firstSlotValidation = ValidateHolidaySlotDate(template.CreatedAt, tourDate);
            if (!firstSlotValidation.IsValid)
            {
                AddFieldError(result, "TourDate", firstSlotValidation.ErrorMessage);
            }

            // Set validation result
            result.IsValid = !result.FieldErrors.Any();
            if (!result.IsValid)
            {
                result.StatusCode = 400;
                result.Message = "Vi ph?m quy t?c kinh doanh cho holiday template";
                result.ValidationErrors = result.FieldErrors.SelectMany(x => x.Value).ToList();
            }

            return result;
        }

        /// <summary>
        /// Validate holiday template slot date - similar to regular template but more flexible
        /// </summary>
        public static (bool IsValid, string ErrorMessage) ValidateHolidaySlotDate(DateTime createdAt, DateTime tourDate)
        {
            // Rule 1: Tour date must be in the future
            var currentTime = VietnamTimeZoneUtility.GetVietnamNow();
            if (tourDate <= currentTime)
            {
                return (false, $"Ng�y tour ph?i trong t??ng lai. Ng�y hi?n t?i: {currentTime:dd/MM/yyyy}");
            }
            
            // Rule 2: Apply 30-day rule
            var minimumDate = createdAt.AddDays(30);
            if (tourDate < minimumDate)
            {
                var suggestedDate = minimumDate.AddDays(7);
                return (false, $"Ng�y tour ph?i sau �t nh?t 30 ng�y t? ng�y t?o ({createdAt:dd/MM/yyyy}). " +
                              $"Ng�y s?m nh?t c� th?: {minimumDate:dd/MM/yyyy}. " +
                              $"G?i �: Ch?n ng�y {suggestedDate:dd/MM/yyyy} ho?c mu?n h?n.");
            }
            
            // Rule 3: Not too far in the future (2 years max)
            var maxFutureDate = currentTime.AddYears(2);
            if (tourDate > maxFutureDate)
            {
                return (false, $"Ng�y tour kh�ng ???c qu� 2 n?m trong t??ng lai. " +
                              $"Ng�y mu?n nh?t c� th?: {maxFutureDate:dd/MM/yyyy}");
            }
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Get schedule day from date for holiday template
        /// Returns the actual day of week without restrictions
        /// </summary>
        public static ScheduleDay GetScheduleDayFromDate(DateOnly date)
        {
            var dateTime = date.ToDateTime(TimeOnly.MinValue);
            return dateTime.DayOfWeek switch
            {
                DayOfWeek.Sunday => ScheduleDay.Sunday,
                DayOfWeek.Monday => ScheduleDay.Monday,
                DayOfWeek.Tuesday => ScheduleDay.Tuesday,
                DayOfWeek.Wednesday => ScheduleDay.Wednesday,
                DayOfWeek.Thursday => ScheduleDay.Thursday,
                DayOfWeek.Friday => ScheduleDay.Friday,
                DayOfWeek.Saturday => ScheduleDay.Saturday,
                _ => ScheduleDay.Sunday // Default fallback
            };
        }

        /// <summary>
        /// Validate if day is valid for holiday template
        /// All days are valid for holiday templates
        /// </summary>
        public static bool IsValidHolidayScheduleDay(ScheduleDay scheduleDay)
        {
            // All days are valid for holiday templates
            return Enum.IsDefined(typeof(ScheduleDay), scheduleDay);
        }

        /// <summary>
        /// Get valid schedule days for holiday template
        /// Returns all days of the week
        /// </summary>
        public static List<ScheduleDay> GetValidHolidayScheduleDays()
        {
            return new List<ScheduleDay>
            {
                ScheduleDay.Sunday,
                ScheduleDay.Monday,
                ScheduleDay.Tuesday,
                ScheduleDay.Wednesday,
                ScheduleDay.Thursday,
                ScheduleDay.Friday,
                ScheduleDay.Saturday
            };
        }

        /// <summary>
        /// Get valid schedule days with Vietnamese names for holiday template
        /// </summary>
        public static Dictionary<ScheduleDay, string> GetValidHolidayScheduleDaysWithNames()
        {
            return GetValidHolidayScheduleDays()
                .ToDictionary(day => day, day => day.GetVietnameseName());
        }

        /// <summary>
        /// Helper method to add field error
        /// </summary>
        private static void AddFieldError(ResponseValidationDto result, string fieldName, string errorMessage)
        {
            if (!result.FieldErrors.ContainsKey(fieldName))
            {
                result.FieldErrors[fieldName] = new List<string>();
            }
            result.FieldErrors[fieldName].Add(errorMessage);
        }

        /// <summary>
        /// Validate holiday tour template update request
        /// Cho ph�p ch?n b?t k? ng�y n�o trong tu?n cho ng�y m?i
        /// </summary>
        public static ResponseValidationDto ValidateUpdateRequest(RequestUpdateHolidayTourTemplateDto request, TourTemplate existingTemplate)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200,
                ValidationErrors = new List<string>(),
                FieldErrors = new Dictionary<string, List<string>>()
            };

            // Title validation (if provided)
            if (request.Title != null)
            {
                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    AddFieldError(result, nameof(request.Title), "T�n template kh�ng ???c ?? tr?ng");
                }
                else if (request.Title.Length > 200)
                {
                    AddFieldError(result, nameof(request.Title), "T�n template kh�ng ???c v??t qu� 200 k� t?");
                }
            }

            // Location validation (if provided)
            if (request.StartLocation != null)
            {
                if (string.IsNullOrWhiteSpace(request.StartLocation))
                {
                    AddFieldError(result, nameof(request.StartLocation), "?i?m b?t ??u kh�ng ???c ?? tr?ng");
                }
                else if (request.StartLocation.Length > 500)
                {
                    AddFieldError(result, nameof(request.StartLocation), "?i?m b?t ??u kh�ng ???c v??t qu� 500 k� t?");
                }
            }

            if (request.EndLocation != null)
            {
                if (string.IsNullOrWhiteSpace(request.EndLocation))
                {
                    AddFieldError(result, nameof(request.EndLocation), "?i?m k?t th�c kh�ng ???c ?? tr?ng");
                }
                else if (request.EndLocation.Length > 500)
                {
                    AddFieldError(result, nameof(request.EndLocation), "?i?m k?t th�c kh�ng ???c v??t qu� 500 k� t?");
                }
            }

            // Template type validation (if provided)
            if (request.TemplateType.HasValue && !Enum.IsDefined(typeof(TourTemplateType), request.TemplateType.Value))
            {
                AddFieldError(result, nameof(request.TemplateType), "Lo?i tour template kh�ng h?p l?");
            }

            // Tour date validation (if provided) - Apply same business rules
            if (request.TourDate.HasValue)
            {
                var currentTime = VietnamTimeZoneUtility.GetVietnamNow();
                var newTourDateTime = request.TourDate.Value.ToDateTime(TimeOnly.MinValue);

                // Rule 1: Tour date must be in the future
                if (request.TourDate.Value <= DateOnly.FromDateTime(currentTime))
                {
                    AddFieldError(result, nameof(request.TourDate), "Ng�y tour m?i ph?i l� ng�y trong t??ng lai");
                }

                // Rule 2: Apply the same 30-day rule as when creating
                var minimumDate = existingTemplate.CreatedAt.AddDays(30);
                if (newTourDateTime < minimumDate)
                {
                    var suggestedDate = minimumDate.AddDays(7);
                    AddFieldError(result, nameof(request.TourDate), 
                        $"Ng�y tour ph?i sau �t nh?t 30 ng�y t? ng�y t?o template ({existingTemplate.CreatedAt:dd/MM/yyyy}). " +
                        $"Ng�y s?m nh?t c� th?: {minimumDate:dd/MM/yyyy}. " +
                        $"G?i �: Ch?n ng�y {suggestedDate:dd/MM/yyyy} ho?c mu?n h?n. " +
                        $"V� d? JSON h?p l?: \"tourDate\": \"{suggestedDate:yyyy-MM-dd}\"");
                }

                // Rule 3: Tour date should not be too far in the future (2 years max)
                var maxFutureDate = DateOnly.FromDateTime(currentTime.AddYears(2));
                if (request.TourDate.Value > maxFutureDate)
                {
                    AddFieldError(result, nameof(request.TourDate), 
                        $"Ng�y tour kh�ng ???c qu� 2 n?m trong t??ng lai. " +
                        $"Ng�y mu?n nh?t c� th?: {maxFutureDate:dd/MM/yyyy}");
                }

                // Rule 4: Validate year range
                if (request.TourDate.Value.Year < 2024 || request.TourDate.Value.Year > 2030)
                {
                    AddFieldError(result, nameof(request.TourDate), "N?m c?a ng�y tour ph?i t? 2024 ??n 2030");
                }
            }

            // Set validation result
            result.IsValid = !result.FieldErrors.Any();
            if (!result.IsValid)
            {
                result.StatusCode = 400;
                result.Message = "D? li?u c?p nh?t kh�ng h?p l? - Vui l�ng ki?m tra v� s?a c�c l?i sau";
                result.ValidationErrors = result.FieldErrors.SelectMany(x => x.Value).ToList();
                
                // Add helpful guidance for holiday template update
                var currentTime = VietnamTimeZoneUtility.GetVietnamNow();
                var minimumDate = existingTemplate.CreatedAt.AddDays(30);
                var maxFutureDate = DateOnly.FromDateTime(currentTime.AddYears(2));
                
                result.ValidationErrors.Add("?? H??NG D?N C?P NH?T HOLIDAY TEMPLATE:");
                result.ValidationErrors.Add($"� Template ???c t?o: {existingTemplate.CreatedAt:dd/MM/yyyy}");
                result.ValidationErrors.Add($"� Ng�y s?m nh?t cho tourDate: {minimumDate:dd/MM/yyyy} (sau 30 ng�y t? ng�y t?o)");
                result.ValidationErrors.Add($"� Ng�y mu?n nh?t cho tourDate: {maxFutureDate:dd/MM/yyyy} (t?i ?a 2 n?m)");
                result.ValidationErrors.Add("� ? ??C BI?T: Holiday template c� th? ch?n b?t k? ng�y n�o trong tu?n");
                result.ValidationErrors.Add("� ?? Ch? g?i fields mu?n thay ??i, ?? null cho fields kh�ng thay ??i");
            }

            return result;
        }
    }
}