using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

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
