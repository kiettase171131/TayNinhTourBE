using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Utilities
{
    /// <summary>
    /// Utility class cho validation business logic của TourTemplate
    /// </summary>
    public static class TourTemplateValidator
    {
        /// <summary>
        /// Validate tour template creation request
        /// </summary>
        public static ResponseValidationDto ValidateCreateRequest(RequestCreateTourTemplateDto request)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            // Title validation
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                AddFieldError(result, nameof(request.Title), "Tên template là bắt buộc");
            }
            else if (request.Title.Length > 200)
            {
                AddFieldError(result, nameof(request.Title), "Tên template không được vượt quá 200 ký tự");
            }

            // Location validation
            if (string.IsNullOrWhiteSpace(request.StartLocation))
            {
                AddFieldError(result, nameof(request.StartLocation), "Điểm bắt đầu là bắt buộc");
            }

            if (string.IsNullOrWhiteSpace(request.EndLocation))
            {
                AddFieldError(result, nameof(request.EndLocation), "Điểm kết thúc là bắt buộc");
            }

            // Month validation
            if (request.Month < 1 || request.Month > 12)
            {
                AddFieldError(result, nameof(request.Month), "Tháng phải từ 1 đến 12");
            }

            // Year validation
            if (request.Year < 2024 || request.Year > 2030)
            {
                AddFieldError(result, nameof(request.Year), "Năm phải từ 2024 đến 2030");
            }

            // ScheduleDay validation (Saturday OR Sunday only)
            var scheduleValidation = TourTemplateScheduleValidator.ValidateScheduleDay(request.ScheduleDays);
            if (!scheduleValidation.IsValid)
            {
                AddFieldError(result, nameof(request.ScheduleDays), scheduleValidation.ErrorMessage ?? "Chỉ được chọn Thứ 7 hoặc Chủ nhật");
            }

            // Validate first slot date according to new business rules
            var currentTime = VietnamTimeZoneUtility.GetVietnamNow();
            var firstSlotValidation = ValidateFirstSlotDate(currentTime, request.Month, request.Year);
            if (!firstSlotValidation.IsValid)
            {
                AddFieldError(result, "FirstSlotDate", firstSlotValidation.ErrorMessage);
            }

            // Set validation result
            result.IsValid = !result.FieldErrors.Any();
            if (!result.IsValid)
            {
                result.StatusCode = 400;
                result.Message = "Dữ liệu không hợp lệ - Vui lòng kiểm tra và sửa các lỗi sau";
                result.ValidationErrors = result.FieldErrors.SelectMany(x => x.Value).ToList();
                
                // Add helpful guidance
                var vietnamTime = VietnamTimeZoneUtility.GetVietnamNow();
                result.ValidationErrors.Add("💡 HƯỚNG DẪN:");
                result.ValidationErrors.Add($"• Tháng hiện tại: {vietnamTime.Month}/{vietnamTime.Year} - KHÔNG thể chọn");
                result.ValidationErrors.Add($"• Tháng kế tiếp: {vietnamTime.AddMonths(1).Month}/{vietnamTime.AddMonths(1).Year} - Có thể chọn nếu đủ 30 ngày");
                result.ValidationErrors.Add($"• Tháng an toàn: {vietnamTime.AddMonths(2).Month}/{vietnamTime.AddMonths(2).Year} - Luôn có thể chọn");
                result.ValidationErrors.Add("• Chỉ được chọn Saturday HOẶC Sunday (không phải cả hai)");
                result.ValidationErrors.Add("• Ví dụ JSON hợp lệ: {\"month\": " + vietnamTime.AddMonths(2).Month + ", \"year\": " + vietnamTime.AddMonths(2).Year + ", \"scheduleDays\": \"Saturday\"}");
            }

            return result;
        }

        /// <summary>
        /// Validate tour template update request
        /// </summary>
        public static ResponseValidationDto ValidateUpdateRequest(RequestUpdateTourTemplateDto request, TourTemplate existingTemplate)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            // Only validate fields that are being updated (not null)
            if (!string.IsNullOrEmpty(request.Title))
            {
                if (request.Title.Length > 200)
                {
                    AddFieldError(result, nameof(request.Title), "Tên template không được vượt quá 200 ký tự");
                }
            }

            // Validate ScheduleDay if being updated
            if (request.ScheduleDays.HasValue)
            {
                var scheduleValidation = TourTemplateScheduleValidator.ValidateScheduleDay(request.ScheduleDays.Value);
                if (!scheduleValidation.IsValid)
                {
                    AddFieldError(result, nameof(request.ScheduleDays), scheduleValidation.ErrorMessage ?? "Chỉ được chọn Thứ 7 hoặc Chủ nhật");
                }
            }

            // Set validation result
            result.IsValid = !result.FieldErrors.Any();
            if (!result.IsValid)
            {
                result.StatusCode = 400;
                result.Message = "Dữ liệu không hợp lệ";
                result.ValidationErrors = result.FieldErrors.SelectMany(x => x.Value).ToList();
            }

            return result;
        }

        /// <summary>
        /// Validate business rules for tour template
        /// </summary>
        public static ResponseValidationDto ValidateBusinessRules(TourTemplate template)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            // Validate Saturday OR Sunday only (not both)
            var scheduleValidation = TourTemplateScheduleValidator.ValidateScheduleDay(template.ScheduleDays);
            if (!scheduleValidation.IsValid)
            {
                AddFieldError(result, "ScheduleDays", scheduleValidation.ErrorMessage ?? "Lỗi validation schedule day");
            }

            // Validate Month/Year combination
            if (template.Month < 1 || template.Month > 12)
            {
                AddFieldError(result, "Month", "Tháng phải từ 1 đến 12");
            }

            if (template.Year < 2024 || template.Year > 2030)
            {
                AddFieldError(result, "Year", "Năm phải từ 2024 đến 2030");
            }

            // Validate first slot date for existing template
            var firstSlotValidation = ValidateFirstSlotDate(template.CreatedAt, template.Month, template.Year);
            if (!firstSlotValidation.IsValid)
            {
                AddFieldError(result, "FirstSlotDate", firstSlotValidation.ErrorMessage);
            }

            // Set validation result
            result.IsValid = !result.FieldErrors.Any();
            if (!result.IsValid)
            {
                result.StatusCode = 400;
                result.Message = "Vi phạm quy tắc kinh doanh";
                result.ValidationErrors = result.FieldErrors.SelectMany(x => x.Value).ToList();
            }

            return result;
        }

        /// <summary>
        /// Validate ngày bắt đầu của slot đầu tiên (firstSlotDate) dựa trên ngày tạo tour (createdAt).
        /// 
        /// ✅ Quy tắc:
        /// 1. Ngày tháng năm của template phải lớn hơn ngày hiện tại
        /// 2. Slot đầu tiên phải nằm **sau ít nhất 30 ngày** so với ngày tạo.
        /// 3. Slot đầu tiên phải bắt đầu từ **ngày 01 của tháng kế tiếp trở đi**, tức là không được nằm trong cùng tháng hoặc tháng liền kề nhưng chưa nhảy sang ngày 1.
        /// 
        /// ⚠️ Ví dụ:
        /// - createdAt = 28/06/2025 → firstSlotDate phải >= 01/08/2025 (tháng 8 trở đi)
        /// - createdAt = 01/05/2025 → firstSlotDate phải >= 01/07/2025
        /// - Nếu hôm nay là 15/01/2025, không thể tạo template cho tháng 1/2025
        /// 
        /// ❌ Nếu vi phạm điều kiện → trả lỗi:
        /// "Slot đầu tiên phải bắt đầu sau ít nhất 30 ngày và nằm từ tháng kế tiếp trở đi (từ ngày 1 của tháng mới)."
        /// </summary>
        /// <param name="createdAt">Ngày tạo template (hoặc ngày hiện tại khi tạo mới)</param>
        /// <param name="slotMonth">Tháng của slot đầu tiên</param>
        /// <param name="slotYear">Năm của slot đầu tiên</param>
        /// <returns>Kết quả validation</returns>
        public static (bool IsValid, string ErrorMessage) ValidateFirstSlotDate(DateTime createdAt, int slotMonth, int slotYear)
        {
            // Tính ngày đầu tiên của tháng slot (ngày 1)
            var firstSlotDate = new DateTime(slotYear, slotMonth, 1);
            
            // Quy tắc 1: Template phải lớn hơn ngày hiện tại
            // So sánh với tháng hiện tại để đảm bảo không tạo template cho tháng đã qua hoặc tháng hiện tại
            var currentTime = VietnamTimeZoneUtility.GetVietnamNow();
            var currentMonthFirstDay = new DateTime(currentTime.Year, currentTime.Month, 1);
            
            if (firstSlotDate <= currentMonthFirstDay)
            {
                return (false, $"Không thể tạo template cho tháng hiện tại ({currentTime.Month}/{currentTime.Year}) hoặc tháng đã qua. " +
                              $"Vui lòng chọn tháng từ {currentTime.AddMonths(1).Month}/{currentTime.AddMonths(1).Year} trở đi. " +
                              $"Ví dụ: month: {currentTime.AddMonths(2).Month}, year: {currentTime.AddMonths(2).Year}");
            }
            
            // Quy tắc 2: Slot đầu tiên phải sau ít nhất 30 ngày so với ngày tạo
            var minimumDate = createdAt.AddDays(30);
            
            if (firstSlotDate < minimumDate)
            {
                var suggestedMonth = minimumDate.AddMonths(1);
                return (false, $"Slot đầu tiên phải bắt đầu sau ít nhất 30 ngày từ ngày tạo ({createdAt:dd/MM/yyyy}). " +
                              $"Ngày sớm nhất có thể: {minimumDate:dd/MM/yyyy}. " +
                              $"Vui lòng chọn tháng {suggestedMonth.Month}/{suggestedMonth.Year} hoặc muộn hơn. " +
                              $"Ví dụ: month: {suggestedMonth.Month}, year: {suggestedMonth.Year}");
            }
            
            // Quy tắc 3: Slot phải bắt đầu từ ngày 1 của tháng kế tiếp trở đi
            // Tính tháng kế tiếp từ ngày tạo
            var createdAtNextMonth = createdAt.AddMonths(1);
            var nextMonthFirstDay = new DateTime(createdAtNextMonth.Year, createdAtNextMonth.Month, 1);
            
            if (firstSlotDate < nextMonthFirstDay)
            {
                return (false, $"Template phải được tạo cho tháng kế tiếp trở đi. " +
                              $"Ngày tạo: {createdAt:dd/MM/yyyy}, tháng kế tiếp: {createdAtNextMonth.Month}/{createdAtNextMonth.Year}. " +
                              $"Vui lòng chọn month: {createdAtNextMonth.Month}, year: {createdAtNextMonth.Year} hoặc muộn hơn.");
            }
            
            return (true, string.Empty);
        }

        /// <summary>
        /// Validate slot generation request to ensure it complies with first slot date rules
        /// </summary>
        /// <param name="templateCreatedAt">Template creation date</param>
        /// <param name="slotMonth">Month for slot generation</param>
        /// <param name="slotYear">Year for slot generation</param>
        /// <returns>Validation result</returns>
        public static (bool IsValid, string ErrorMessage) ValidateSlotGenerationDate(DateTime templateCreatedAt, int slotMonth, int slotYear)
        {
            return ValidateFirstSlotDate(templateCreatedAt, slotMonth, slotYear);
        }

        /// <summary>
        /// Validate if user can perform action on template
        /// </summary>
        public static ResponseValidationDto ValidatePermission(TourTemplate template, Guid userId, string action)
        {
            var result = new ResponseValidationDto
            {
                IsValid = true,
                StatusCode = 200
            };

            // Check if user is the owner of the template
            if (template.CreatedById != userId)
            {
                result.IsValid = false;
                result.StatusCode = 403;
                result.Message = $"Bạn không có quyền {action} tour template này";
                result.ValidationErrors.Add($"Chỉ người tạo mới có thể {action} tour template");
            }

            return result;
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
    }
}
