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
                AddFieldError(result, nameof(request.Title), "Tiêu đề tour template là bắt buộc");
            }
            else if (request.Title.Length > 200)
            {
                AddFieldError(result, nameof(request.Title), "Tiêu đề không được vượt quá 200 ký tự");
            }

            // Price validation
            if (request.Price < 0)
            {
                AddFieldError(result, nameof(request.Price), "Giá tour phải lớn hơn hoặc bằng 0");
            }
            else if (request.Price > 100000000) // 100 million VND
            {
                AddFieldError(result, nameof(request.Price), "Giá tour không được vượt quá 100,000,000 VND");
            }

            // Guests validation
            if (request.MaxGuests <= 0)
            {
                AddFieldError(result, nameof(request.MaxGuests), "Số lượng khách tối đa phải lớn hơn 0");
            }
            else if (request.MaxGuests > 1000)
            {
                AddFieldError(result, nameof(request.MaxGuests), "Số lượng khách tối đa không được vượt quá 1000");
            }

            if (request.MinGuests < 1)
            {
                AddFieldError(result, nameof(request.MinGuests), "Số lượng khách tối thiểu phải ít nhất là 1");
            }
            else if (request.MinGuests > request.MaxGuests)
            {
                AddFieldError(result, nameof(request.MinGuests), "Số lượng khách tối thiểu không được lớn hơn số lượng khách tối đa");
            }

            // Duration validation
            if (request.Duration <= 0)
            {
                AddFieldError(result, nameof(request.Duration), "Thời gian tour phải lớn hơn 0");
            }
            else if (request.Duration > 30) // Max 30 days
            {
                AddFieldError(result, nameof(request.Duration), "Thời gian tour không được vượt quá 30 ngày");
            }

            // Location validation
            if (string.IsNullOrWhiteSpace(request.StartLocation))
            {
                AddFieldError(result, nameof(request.StartLocation), "Điểm khởi hành là bắt buộc");
            }

            if (string.IsNullOrWhiteSpace(request.EndLocation))
            {
                AddFieldError(result, nameof(request.EndLocation), "Điểm kết thúc là bắt buộc");
            }

            // Child price validation
            if (request.ChildPrice.HasValue)
            {
                if (request.ChildPrice < 0)
                {
                    AddFieldError(result, nameof(request.ChildPrice), "Giá trẻ em phải lớn hơn hoặc bằng 0");
                }
                else if (request.ChildPrice > request.Price)
                {
                    AddFieldError(result, nameof(request.ChildPrice), "Giá trẻ em không được lớn hơn giá người lớn");
                }

                if (request.ChildMaxAge.HasValue && (request.ChildMaxAge < 1 || request.ChildMaxAge > 17))
                {
                    AddFieldError(result, nameof(request.ChildMaxAge), "Độ tuổi tối đa trẻ em phải từ 1 đến 17");
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
                    AddFieldError(result, nameof(request.Title), "Tiêu đề không được vượt quá 200 ký tự");
                }
            }

            if (request.Price.HasValue)
            {
                if (request.Price < 0)
                {
                    AddFieldError(result, nameof(request.Price), "Giá tour phải lớn hơn hoặc bằng 0");
                }
                else if (request.Price > 100000000)
                {
                    AddFieldError(result, nameof(request.Price), "Giá tour không được vượt quá 100,000,000 VND");
                }
            }

            if (request.MaxGuests.HasValue)
            {
                if (request.MaxGuests <= 0)
                {
                    AddFieldError(result, nameof(request.MaxGuests), "Số lượng khách tối đa phải lớn hơn 0");
                }
                else if (request.MaxGuests > 1000)
                {
                    AddFieldError(result, nameof(request.MaxGuests), "Số lượng khách tối đa không được vượt quá 1000");
                }

                var minGuests = request.MinGuests ?? existingTemplate.MinGuests;
                if (minGuests > request.MaxGuests)
                {
                    AddFieldError(result, nameof(request.MaxGuests), "Số lượng khách tối đa không được nhỏ hơn số lượng khách tối thiểu");
                }
            }

            if (request.MinGuests.HasValue)
            {
                if (request.MinGuests < 1)
                {
                    AddFieldError(result, nameof(request.MinGuests), "Số lượng khách tối thiểu phải ít nhất là 1");
                }

                var maxGuests = request.MaxGuests ?? existingTemplate.MaxGuests;
                if (request.MinGuests > maxGuests)
                {
                    AddFieldError(result, nameof(request.MinGuests), "Số lượng khách tối thiểu không được lớn hơn số lượng khách tối đa");
                }
            }

            if (request.Duration.HasValue)
            {
                if (request.Duration <= 0)
                {
                    AddFieldError(result, nameof(request.Duration), "Thời gian tour phải lớn hơn 0");
                }
                else if (request.Duration > 30)
                {
                    AddFieldError(result, nameof(request.Duration), "Thời gian tour không được vượt quá 30 ngày");
                }
            }

            if (request.ChildPrice.HasValue)
            {
                if (request.ChildPrice < 0)
                {
                    AddFieldError(result, nameof(request.ChildPrice), "Giá trẻ em phải lớn hơn hoặc bằng 0");
                }

                var adultPrice = request.Price ?? existingTemplate.Price;
                if (request.ChildPrice > adultPrice)
                {
                    AddFieldError(result, nameof(request.ChildPrice), "Giá trẻ em không được lớn hơn giá người lớn");
                }
            }

            if (request.ChildMaxAge.HasValue && (request.ChildMaxAge < 1 || request.ChildMaxAge > 17))
            {
                AddFieldError(result, nameof(request.ChildMaxAge), "Độ tuổi tối đa trẻ em phải từ 1 đến 17");
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

            // NEW CONSTRAINT: Validate Saturday OR Sunday only (not both)
            var scheduleValidation = TourTemplateScheduleValidator.ValidateScheduleDay(template.ScheduleDays);
            if (!scheduleValidation.IsValid)
            {
                AddFieldError(result, "ScheduleDays", scheduleValidation.ErrorMessage ?? "Lỗi validation schedule day");
            }

            // Check price consistency
            if (template.ChildPrice.HasValue && template.ChildPrice > template.Price)
            {
                AddFieldError(result, "ChildPrice", "Giá trẻ em không được lớn hơn giá người lớn");
            }

            // Check guest capacity
            if (template.MinGuests > template.MaxGuests)
            {
                AddFieldError(result, "MinGuests", "Số lượng khách tối thiểu không được lớn hơn số lượng khách tối đa");
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
