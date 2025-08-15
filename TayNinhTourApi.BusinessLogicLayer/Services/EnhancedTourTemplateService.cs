using AutoMapper;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Enhanced TourTemplateService với validation, image handling và proper response DTOs
    /// Đây là version cải tiến của TourTemplateService hiện tại
    /// </summary>
    public class EnhancedTourTemplateService : BaseService, ITourTemplateService
    {
        private readonly TourTemplateImageHandler _imageHandler;
        private readonly ICurrentUserService _currentUserService;

        public EnhancedTourTemplateService(IMapper mapper, IUnitOfWork unitOfWork, ICurrentUserService currentUserService) : base(mapper, unitOfWork)
        {
            _imageHandler = new TourTemplateImageHandler(unitOfWork);
            _currentUserService = currentUserService;
        }

        #region Enhanced CRUD Operations

        public async Task<ResponseCreateTourTemplateDto> CreateTourTemplateAsync(RequestCreateTourTemplateDto request, Guid createdById)
        {
            // Validate request
            var validationResult = TourTemplateValidator.ValidateCreateRequest(request);
            if (!validationResult.IsValid)
            {
                return new ResponseCreateTourTemplateDto
                {
                    StatusCode = validationResult.StatusCode,
                    Message = validationResult.Message,
                    success = false,
                    ValidationErrors = validationResult.ValidationErrors,
                    FieldErrors = validationResult.FieldErrors,
                    Data = null
                };
            }

            // Validate images if provided
            if (request.Images != null && request.Images.Any())
            {
                var imageValidation = await _imageHandler.ValidateImageUrlsAsync(request.Images);
                if (!imageValidation.IsValid)
                {
                    return new ResponseCreateTourTemplateDto
                    {
                        StatusCode = imageValidation.StatusCode,
                        Message = imageValidation.Message,
                        success = false,
                        ValidationErrors = imageValidation.ValidationErrors,
                        FieldErrors = imageValidation.FieldErrors,
                        Data = null
                    };
                }
            }

            try
            {
                // Map DTO to entity
                var tourTemplate = _mapper.Map<TourTemplate>(request);
                tourTemplate.Id = Guid.NewGuid();
                tourTemplate.CreatedById = createdById;
                tourTemplate.CreatedAt = DateTime.UtcNow;
                tourTemplate.IsActive = true;
                tourTemplate.IsDeleted = false;

                // Handle images if provided
                if (request.Images != null && request.Images.Any())
                {
                    var images = await _imageHandler.GetImagesAsync(request.Images);
                    tourTemplate.Images = images;
                }

                // Validate business rules
                var businessValidation = TourTemplateValidator.ValidateBusinessRules(tourTemplate);
                if (!businessValidation.IsValid)
                {
                    return new ResponseCreateTourTemplateDto
                    {
                        StatusCode = businessValidation.StatusCode,
                        Message = businessValidation.Message,
                        success = false,
                        ValidationErrors = businessValidation.ValidationErrors,
                        FieldErrors = businessValidation.FieldErrors,
                        Data = null
                    };
                }

                // Save to database
                await _unitOfWork.TourTemplateRepository.AddAsync(tourTemplate);
                await _unitOfWork.SaveChangesAsync();

                // Map to response DTO
                var responseDto = _mapper.Map<TourTemplateDto>(tourTemplate);

                return new ResponseCreateTourTemplateDto
                {
                    StatusCode = 201,
                    Message = "Tạo tour template thành công",
                    success = true,
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                return new ResponseCreateTourTemplateDto
                {
                    StatusCode = 500,
                    Message = "Lỗi khi tạo tour template",
                    success = false,
                    ValidationErrors = new List<string> { ex.Message },
                    Data = null
                };
            }
        }

        public async Task<ResponseUpdateTourTemplateDto> UpdateTourTemplateAsync(Guid id, RequestUpdateTourTemplateDto request, Guid updatedById)
        {
            try
            {
                // Get existing template
                var existingTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, new[] { "Images", "TourDetails" });
                if (existingTemplate == null || existingTemplate.IsDeleted)
                {
                    return new ResponseUpdateTourTemplateDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour template",
                        success = false,
                        Data = null
                    };
                }

                // Check permission
                var permissionCheck = TourTemplateValidator.ValidatePermission(existingTemplate, updatedById, "cập nhật");
                if (!permissionCheck.IsValid)
                {
                    return new ResponseUpdateTourTemplateDto
                    {
                        StatusCode = permissionCheck.StatusCode,
                        Message = permissionCheck.Message,
                        success = false,
                        ValidationErrors = permissionCheck.ValidationErrors,
                        FieldErrors = permissionCheck.FieldErrors,
                        Data = null
                    };
                }

                // Check if template can be updated - prevent updates if it has public tours with bookings
                var canUpdateCheck = await CanUpdateTourTemplateAsync(id);
                if (!canUpdateCheck.CanUpdate)
                {
                    return new ResponseUpdateTourTemplateDto
                    {
                        StatusCode = 409,
                        Message = canUpdateCheck.Reason,
                        success = false,
                        ValidationErrors = canUpdateCheck.BlockingReasons,
                        Data = null
                    };
                }

                // Validate update request
                var validationResult = TourTemplateValidator.ValidateUpdateRequest(request, existingTemplate);
                if (!validationResult.IsValid)
                {
                    return new ResponseUpdateTourTemplateDto
                    {
                        StatusCode = validationResult.StatusCode,
                        Message = validationResult.Message,
                        success = false,
                        ValidationErrors = validationResult.ValidationErrors,
                        FieldErrors = validationResult.FieldErrors,
                        Data = null
                    };
                }

                // Handle image updates
                if (request.Images != null)
                {
                    var imageUpdateResult = await _imageHandler.UpdateTourTemplateImagesAsync(existingTemplate, request.Images);
                    if (!imageUpdateResult.IsValid)
                    {
                        return new ResponseUpdateTourTemplateDto
                        {
                            StatusCode = imageUpdateResult.StatusCode,
                            Message = imageUpdateResult.Message,
                            success = false,
                            ValidationErrors = imageUpdateResult.ValidationErrors,
                            FieldErrors = imageUpdateResult.FieldErrors,
                            Data = null
                        };
                    }
                }

                // Map updates
                _mapper.Map(request, existingTemplate);

                // Set audit fields
                existingTemplate.UpdatedById = updatedById;
                existingTemplate.UpdatedAt = DateTime.UtcNow;

                // Validate business rules after update
                var businessValidation = TourTemplateValidator.ValidateBusinessRules(existingTemplate);
                if (!businessValidation.IsValid)
                {
                    return new ResponseUpdateTourTemplateDto
                    {
                        StatusCode = businessValidation.StatusCode,
                        Message = businessValidation.Message,
                        success = false,
                        ValidationErrors = businessValidation.ValidationErrors,
                        FieldErrors = businessValidation.FieldErrors,
                        Data = null
                    };
                }

                await _unitOfWork.TourTemplateRepository.Update(existingTemplate);
                await _unitOfWork.SaveChangesAsync();

                // Map to response DTO
                var responseDto = _mapper.Map<TourTemplateDto>(existingTemplate);

                return new ResponseUpdateTourTemplateDto
                {
                    StatusCode = 200,
                    Message = "Cập nhật tour template thành công",
                    success = true,
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                return new ResponseUpdateTourTemplateDto
                {
                    StatusCode = 500,
                    Message = "Lỗi khi cập nhật tour template",
                    success = false,
                    ValidationErrors = new List<string> { ex.Message },
                    Data = null
                };
            }
        }

        public async Task<ResponseDeleteTourTemplateDto> DeleteTourTemplateAsync(Guid id, Guid deletedById)
        {
            try
            {
                var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, new[] { "TourDetails" });
                if (template == null || template.IsDeleted)
                {
                    return new ResponseDeleteTourTemplateDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour template",
                        success = false
                    };
                }

                // Check permission
                var permissionCheck = TourTemplateValidator.ValidatePermission(template, deletedById, "xóa");
                if (!permissionCheck.IsValid)
                {
                    return new ResponseDeleteTourTemplateDto
                    {
                        StatusCode = permissionCheck.StatusCode,
                        Message = permissionCheck.Message,
                        success = false
                    };
                }

                // Check if template can be deleted - prevent deletion if it has public tours with bookings
                var canDeleteCheck = await CanDeleteTourTemplateAsync(id);
                if (!canDeleteCheck.CanDelete)
                {
                    return new ResponseDeleteTourTemplateDto
                    {
                        StatusCode = 409,
                        Message = canDeleteCheck.Reason,
                        success = false,
                        BlockingReasons = canDeleteCheck.BlockingReasons
                    };
                }

                // Soft delete
                template.IsDeleted = true;
                template.DeletedAt = DateTime.UtcNow;
                template.UpdatedById = deletedById;
                template.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourTemplateRepository.Update(template);
                await _unitOfWork.SaveChangesAsync();

                return new ResponseDeleteTourTemplateDto
                {
                    StatusCode = 200,
                    Message = "Xóa tour template thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseDeleteTourTemplateDto
                {
                    StatusCode = 500,
                    Message = "Lỗi khi xóa tour template: " + ex.Message,
                    success = false
                };
            }
        }

        public async Task<ResponseGetTourTemplateDto> GetTourTemplateByIdAsync(Guid id)
        {
            try
            {
                var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, new[] { "CreatedBy", "UpdatedBy", "Images", "TourDetails" });
                if (template == null || template.IsDeleted)
                {
                    return new ResponseGetTourTemplateDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour template",
                        Data = null
                    };
                }

                var responseDto = _mapper.Map<TourTemplateDetailDto>(template);

                return new ResponseGetTourTemplateDto
                {
                    StatusCode = 200,
                    Message = "Lấy thông tin tour template thành công",
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                return new ResponseGetTourTemplateDto
                {
                    StatusCode = 500,
                    Message = "Lỗi khi lấy thông tin tour template",
                    Data = null
                };
            }
        }

        #endregion

        #region Enhanced Business Operations

        public async Task<ResponseCopyTourTemplateDto> CopyTourTemplateAsync(Guid id, string newTitle, Guid createdById)
        {
            try
            {
                var originalTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, new[] { "Images", "TourDetails" });
                if (originalTemplate == null || originalTemplate.IsDeleted)
                {
                    return new ResponseCopyTourTemplateDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour template gốc",
                        Data = null
                    };
                }

                // Create new template based on original
                var newTemplate = new TourTemplate
                {
                    Id = Guid.NewGuid(),
                    Title = newTitle,
                    TemplateType = originalTemplate.TemplateType,
                    ScheduleDays = originalTemplate.ScheduleDays,
                    StartLocation = originalTemplate.StartLocation,
                    EndLocation = originalTemplate.EndLocation,
                    Month = originalTemplate.Month,
                    Year = originalTemplate.Year,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                // Copy images
                if (originalTemplate.Images != null && originalTemplate.Images.Any())
                {
                    var copyImageResult = await _imageHandler.CopyImagesAsync(originalTemplate, newTemplate);
                    if (!copyImageResult.IsValid)
                    {
                        return new ResponseCopyTourTemplateDto
                        {
                            StatusCode = copyImageResult.StatusCode,
                            Message = copyImageResult.Message,
                            Data = null
                        };
                    }
                }

                await _unitOfWork.TourTemplateRepository.AddAsync(newTemplate);
                await _unitOfWork.SaveChangesAsync();

                var responseDto = _mapper.Map<TourTemplateDto>(newTemplate);

                return new ResponseCopyTourTemplateDto
                {
                    StatusCode = 201,
                    Message = "Sao chép tour template thành công",
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                return new ResponseCopyTourTemplateDto
                {
                    StatusCode = 500,
                    Message = "Lỗi khi sao chép tour template",
                    Data = null
                };
            }
        }

        #endregion

        #region Validation Methods

        public async Task<ResponseValidationDto> ValidateCreateRequestAsync(RequestCreateTourTemplateDto request)
        {
            var validationResult = TourTemplateValidator.ValidateCreateRequest(request);

            if (request.Images != null && request.Images.Any())
            {
                var imageValidation = await _imageHandler.ValidateImageUrlsAsync(request.Images);
                if (!imageValidation.IsValid)
                {
                    validationResult.IsValid = false;
                    validationResult.StatusCode = imageValidation.StatusCode;
                    validationResult.Message = imageValidation.Message;
                    validationResult.ValidationErrors.AddRange(imageValidation.ValidationErrors);
                }
            }

            return validationResult;
        }

        public async Task<ResponseValidationDto> ValidateUpdateRequestAsync(Guid id, RequestUpdateTourTemplateDto request)
        {
            var existingTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, null);
            if (existingTemplate == null || existingTemplate.IsDeleted)
            {
                return new ResponseValidationDto
                {
                    IsValid = false,
                    StatusCode = 404,
                    Message = "Không tìm thấy tour template"
                };
            }

            var validationResult = TourTemplateValidator.ValidateUpdateRequest(request, existingTemplate);

            if (request.Images != null)
            {
                var imageValidation = await _imageHandler.ValidateImageUrlsAsync(request.Images);
                if (!imageValidation.IsValid)
                {
                    validationResult.IsValid = false;
                    validationResult.StatusCode = imageValidation.StatusCode;
                    validationResult.Message = imageValidation.Message;
                    validationResult.ValidationErrors.AddRange(imageValidation.ValidationErrors);
                }
            }

            return validationResult;
        }

        #endregion

        #region Business Logic Implementation

        public async Task<ResponseTourTemplateStatisticsDto> GetTourTemplateStatisticsAsync(Guid? createdById = null)
        {
            try
            {
                var allTemplates = createdById.HasValue
                    ? await _unitOfWork.TourTemplateRepository.GetAllAsync(t => t.CreatedById == createdById.Value)
                    : await _unitOfWork.TourTemplateRepository.GetAllAsync();

                var activeTemplates = allTemplates.Where(t => t.IsActive && !t.IsDeleted).ToList();
                var inactiveTemplates = allTemplates.Where(t => !t.IsActive && !t.IsDeleted).ToList();
                var deletedTemplates = allTemplates.Where(t => t.IsDeleted).ToList();

                var statistics = new TourTemplateStatistics
                {
                    TotalTemplates = allTemplates.Count(),
                    ActiveTemplates = activeTemplates.Count,
                    InactiveTemplates = inactiveTemplates.Count,
                    DeletedTemplates = deletedTemplates.Count,
                    TemplatesByType = allTemplates
                        .Where(t => !t.IsDeleted)
                        .GroupBy(t => t.TemplateType)
                        .ToDictionary(g => g.Key.ToString(), g => g.Count()),
                    TemplatesByLocation = allTemplates
                        .Where(t => !t.IsDeleted && !string.IsNullOrEmpty(t.StartLocation))
                        .GroupBy(t => t.StartLocation)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    TemplatesByMonth = allTemplates
                        .Where(t => !t.IsDeleted)
                        .GroupBy(t => $"Tháng {t.Month}")
                        .ToDictionary(g => g.Key, g => g.Count()),
                    TemplatesByYear = allTemplates
                        .Where(t => !t.IsDeleted)
                        .GroupBy(t => t.Year.ToString())
                        .ToDictionary(g => g.Key, g => g.Count())
                };

                // TODO: Add booking and revenue statistics
                statistics.TotalBookings = 0;
                statistics.TotalRevenue = 0;

                return new ResponseTourTemplateStatisticsDto
                {
                    StatusCode = 200,
                    Message = "Lấy thống kê tour templates thành công",
                    Data = statistics
                };
            }
            catch (Exception ex)
            {
                return new ResponseTourTemplateStatisticsDto
                {
                    StatusCode = 500,
                    Message = "Lỗi khi lấy thống kê tour templates",
                    Data = null
                };
            }
        }

        public async Task<(bool success, string Message, int CreatedSlotsCount)> GenerateSlotsForTemplateAsync(
            Guid templateId, int month, int year, bool overwriteExisting = false, bool autoActivate = true)
        {
            try
            {
                // Validate tour template exists
                var tourTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(templateId);
                if (tourTemplate == null)
                {
                    return (false, "Tour template không tồn tại", 0);
                }

                // Validate template has valid ScheduleDays
                var templateValidation = TourTemplateScheduleValidator.ValidateScheduleDay(tourTemplate.ScheduleDays);
                if (!templateValidation.IsValid)
                {
                    return (false, $"Template có ngày không hợp lệ: {templateValidation.ErrorMessage}", 0);
                }

                // Validate first slot date according to business rules
                var firstSlotValidation = TourTemplateValidator.ValidateFirstSlotDate(tourTemplate.CreatedAt, month, year);
                if (!firstSlotValidation.IsValid)
                {
                    return (false, firstSlotValidation.ErrorMessage, 0);
                }

                // Get scheduling service from DI (we need to inject it)
                // For now, we'll create a simple implementation inline
                var weekendDates = CalculateWeekendDates(year, month, tourTemplate.ScheduleDays);

                if (!weekendDates.Any())
                {
                    return (false, "Không có ngày weekend nào trong tháng được chọn", 0);
                }

                var createdSlots = new List<TourSlot>();
                var currentUserId = _currentUserService.GetCurrentUserId();

                foreach (var date in weekendDates)
                {
                    var dateOnly = DateOnly.FromDateTime(date);

                    // Check if slot already exists
                    var existingSlot = await _unitOfWork.TourSlotRepository.GetByDateAsync(templateId, dateOnly);

                    if (existingSlot == null || overwriteExisting)
                    {
                        if (existingSlot != null && overwriteExisting)
                        {
                            // Update existing slot
                            existingSlot.Status = TourSlotStatus.Available;
                            existingSlot.IsActive = autoActivate;
                            existingSlot.UpdatedAt = DateTime.UtcNow;
                            existingSlot.UpdatedById = currentUserId;

                            await _unitOfWork.TourSlotRepository.UpdateAsync(existingSlot);
                            createdSlots.Add(existingSlot);
                        }
                        else
                        {
                            // Create new slot
                            var newSlot = new TourSlot
                            {
                                Id = Guid.NewGuid(),
                                TourTemplateId = templateId,
                                TourDate = dateOnly,
                                ScheduleDay = GetScheduleDay(date),
                                Status = TourSlotStatus.Available,
                                IsActive = autoActivate,
                                CreatedAt = DateTime.UtcNow,
                                CreatedById = currentUserId
                            };

                            await _unitOfWork.TourSlotRepository.AddAsync(newSlot);
                            createdSlots.Add(newSlot);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return (true, $"Tạo thành công {createdSlots.Count} slots", createdSlots.Count);
            }
            catch (Exception ex)
            {
                return (false, $"Có lỗi xảy ra khi tạo slots: {ex.Message}", 0);
            }
        }

        private List<DateTime> CalculateWeekendDates(int year, int month, ScheduleDay scheduleDay)
        {
            var dates = new List<DateTime>();
            var daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var date = new DateTime(year, month, day);
                var dayOfWeek = date.DayOfWeek;

                if ((scheduleDay == ScheduleDay.Saturday && dayOfWeek == DayOfWeek.Saturday) ||
                    (scheduleDay == ScheduleDay.Sunday && dayOfWeek == DayOfWeek.Sunday))
                {
                    dates.Add(date);
                }
            }

            // Return all weekend dates - no artificial limit
            // Let the business logic determine how many slots they need
            return dates.OrderBy(d => d).ToList();
        }

        private ScheduleDay GetScheduleDay(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Saturday => ScheduleDay.Saturday,
                DayOfWeek.Sunday => ScheduleDay.Sunday,
                _ => ScheduleDay.Saturday // Default fallback
            };
        }

        #endregion

        #region Holiday Template Methods

        /// <summary>
        /// Tạo tour template ngày lễ với ngày cụ thể
        /// Tạo template và tự động tạo 1 slot duy nhất cho ngày được chọn
        /// </summary>
        public async Task<ResponseCreateTourTemplateDto> CreateHolidayTourTemplateAsync(RequestCreateHolidayTourTemplateDto request, Guid createdById)
        {
            try
            {
                // Validate basic holiday template request (includes 30-day rule)
                var validationResult = ValidateHolidayTemplateRequest(request);
                if (!validationResult.IsValid)
                {
                    return new ResponseCreateTourTemplateDto
                    {
                        StatusCode = validationResult.StatusCode,
                        Message = validationResult.Message,
                        success = false,
                        ValidationErrors = validationResult.ValidationErrors,
                        FieldErrors = validationResult.FieldErrors,
                        Data = null
                    };
                }

                // Validate images if provided
                if (request.Images != null && request.Images.Any())
                {
                    var imageValidation = await _imageHandler.ValidateImageUrlsAsync(request.Images);
                    if (!imageValidation.IsValid)
                    {
                        return new ResponseCreateTourTemplateDto
                        {
                            StatusCode = imageValidation.StatusCode,
                            Message = imageValidation.Message,
                            success = false,
                            ValidationErrors = imageValidation.ValidationErrors,
                            FieldErrors = imageValidation.FieldErrors,
                            Data = null
                        };
                    }
                }

                // Get schedule day from the tour date
                var scheduleDay = GetScheduleDayFromDate(request.TourDate);

                // Create tour template based on holiday request
                var tourTemplate = new TourTemplate
                {
                    Id = Guid.NewGuid(),
                    Title = request.Title,
                    TemplateType = request.TemplateType,
                    ScheduleDays = scheduleDay,
                    StartLocation = request.StartLocation,
                    EndLocation = request.EndLocation,
                    Month = request.TourDate.Month,
                    Year = request.TourDate.Year,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                // Handle images if provided
                if (request.Images != null && request.Images.Any())
                {
                    var images = await _imageHandler.GetImagesAsync(request.Images);
                    tourTemplate.Images = images;
                }

                // Validate business rules for the created template
                var businessValidation = TourTemplateValidator.ValidateBusinessRules(tourTemplate);
                if (!businessValidation.IsValid)
                {
                    return new ResponseCreateTourTemplateDto
                    {
                        StatusCode = businessValidation.StatusCode,
                        Message = businessValidation.Message,
                        success = false,
                        ValidationErrors = businessValidation.ValidationErrors,
                        FieldErrors = businessValidation.FieldErrors,
                        Data = null
                    };
                }

                // Additional validation: Apply same slot date validation as regular template
                var tourDateTime = request.TourDate.ToDateTime(TimeOnly.MinValue);
                var slotValidation = TourTemplateValidator.ValidateFirstSlotDate(tourTemplate.CreatedAt, tourDateTime.Month, tourDateTime.Year);
                if (!slotValidation.IsValid)
                {
                    return new ResponseCreateTourTemplateDto
                    {
                        StatusCode = 400,
                        Message = "Vi phạm quy tắc tạo slot",
                        success = false,
                        ValidationErrors = new List<string> { slotValidation.ErrorMessage },
                        FieldErrors = new Dictionary<string, List<string>>
                        {
                            ["tourDate"] = new List<string> { slotValidation.ErrorMessage }
                        },
                        Data = null
                    };
                }

                // Save template to database
                await _unitOfWork.TourTemplateRepository.AddAsync(tourTemplate);
                await _unitOfWork.SaveChangesAsync();

                // Create single tour slot for the specific date
                var currentUserId = _currentUserService.GetCurrentUserId();
                var tourSlot = new TourSlot
                {
                    Id = Guid.NewGuid(),
                    TourTemplateId = tourTemplate.Id,
                    TourDate = request.TourDate,
                    ScheduleDay = scheduleDay,
                    Status = TourSlotStatus.Available,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = currentUserId
                };

                await _unitOfWork.TourSlotRepository.AddAsync(tourSlot);
                await _unitOfWork.SaveChangesAsync();

                // Map to response DTO
                var responseDto = _mapper.Map<TourTemplateDto>(tourTemplate);

                return new ResponseCreateTourTemplateDto
                {
                    StatusCode = 201,
                    Message = $"Tạo tour template ngày lễ thành công và đã tạo slot cho ngày {request.TourDate:dd/MM/yyyy} (sau {(tourDateTime - tourTemplate.CreatedAt).Days} ngày từ ngày tạo)",
                    success = true,
                    Data = responseDto
                };
            }
            catch (Exception ex)
            {
                return new ResponseCreateTourTemplateDto
                {
                    StatusCode = 500,
                    Message = "Lỗi khi tạo tour template ngày lễ",
                    success = false,
                    ValidationErrors = new List<string> { ex.Message },
                    Data = null
                };
            }
        }

        /// <summary>
        /// Validate holiday tour template creation request
        /// </summary>
        private ResponseValidationDto ValidateHolidayTemplateRequest(RequestCreateHolidayTourTemplateDto request)
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

            // Tour date validation - Apply same business rules as regular templates
            var currentTime = VietnamTimeZoneUtility.GetVietnamNow();
            var tourDateTime = request.TourDate.ToDateTime(TimeOnly.MinValue);

            // Rule 1: Tour date must be in the future
            if (request.TourDate <= DateOnly.FromDateTime(currentTime))
            {
                AddFieldError(result, nameof(request.TourDate), "Ngày tour phải là ngày trong tương lai");
            }

            // Rule 2: Apply the same 30-day rule as regular templates
            var minimumDate = currentTime.AddDays(30);
            if (tourDateTime < minimumDate)
            {
                var suggestedDate = minimumDate.AddDays(7); // Add 7 more days for safety
                AddFieldError(result, nameof(request.TourDate), 
                    $"Ngày tour phải sau ít nhất 30 ngày từ ngày tạo ({currentTime:dd/MM/yyyy}). " +
                    $"Ngày sớm nhất có thể: {minimumDate:dd/MM/yyyy}. " +
                    $"Gợi ý: Chọn ngày {suggestedDate:dd/MM/yyyy} hoặc muộn hơn. " +
                    $"Ví dụ JSON hợp lệ: \"tourDate\": \"{suggestedDate:yyyy-MM-dd}\"");
            }

            // Rule 3: Tour date should not be too far in the future (same as regular template - 2 years max)
            var maxFutureDate = DateOnly.FromDateTime(currentTime.AddYears(2));
            if (request.TourDate > maxFutureDate)
            {
                AddFieldError(result, nameof(request.TourDate), 
                    $"Ngày tour không được quá 2 năm trong tương lai. " +
                    $"Ngày muộn nhất có thể: {maxFutureDate:dd/MM/yyyy}");
            }

            // Rule 4: Validate year range (same as regular template)
            if (request.TourDate.Year < 2024 || request.TourDate.Year > 2030)
            {
                AddFieldError(result, nameof(request.TourDate), "Năm của ngày tour phải từ 2024 đến 2030");
            }

            // Set validation result
            result.IsValid = !result.FieldErrors.Any();
            if (!result.IsValid)
            {
                result.StatusCode = 400;
                result.Message = "Dữ liệu không hợp lệ - Vui lòng kiểm tra và sửa các lỗi sau";
                result.ValidationErrors = result.FieldErrors.SelectMany(x => x.Value).ToList();
                
                // Add helpful guidance similar to regular template
                result.ValidationErrors.Add("💡 HƯỚNG DẪN HOLIDAY TEMPLATE:");
                result.ValidationErrors.Add($"• Ngày hiện tại: {currentTime:dd/MM/yyyy} - KHÔNG thể chọn");
                result.ValidationErrors.Add($"• Ngày sớm nhất: {minimumDate:dd/MM/yyyy} (sau 30 ngày)");
                result.ValidationErrors.Add($"• Ngày muộn nhất: {maxFutureDate:dd/MM/yyyy} (tối đa 2 năm)");
                result.ValidationErrors.Add($"• Ví dụ JSON hợp lệ: {{\"tourDate\": \"{minimumDate.AddDays(7):yyyy-MM-dd}\"}}");
                result.ValidationErrors.Add("• Khác template thường: Holiday template có thể chọn bất kỳ ngày nào trong tuần");
            }

            return result;
        }

        /// <summary>
        /// Get schedule day from a specific date
        /// </summary>
        private ScheduleDay GetScheduleDayFromDate(DateOnly date)
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
                _ => ScheduleDay.Saturday // Default fallback
            };
        }

        /// <summary>
        /// Helper method to add field errors to validation result
        /// </summary>
        private void AddFieldError(ResponseValidationDto result, string fieldName, string errorMessage)
        {
            if (!result.FieldErrors.ContainsKey(fieldName))
            {
                result.FieldErrors[fieldName] = new List<string>();
            }
            result.FieldErrors[fieldName].Add(errorMessage);
            result.ValidationErrors.Add($"{fieldName}: {errorMessage}");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Tính toán capacity summary cho template
        /// </summary>
        private async Task<TemplateCapacitySummaryDto> CalculateTemplateCapacityAsync(Guid templateId)
        {
            try
            {
                // Get all slots for this template
                var slots = await _unitOfWork.TourSlotRepository.GetAllAsync(
                    s => s.TourTemplateId == templateId && !s.IsDeleted);

                // Get all tour details for this template
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
                    td => td.TourTemplateId == templateId && !td.IsDeleted);

                // Get all operations for these tour details
                var operations = new List<TourOperation>();
                foreach (var detail in tourDetails)
                {
                    var operation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(detail.Id);
                    if (operation != null && operation.IsActive)
                    {
                        operations.Add(operation);
                    }
                }

                // Calculate totals
                var totalSlots = slots.Count();
                var activeSlots = slots.Count(s => s.IsActive);
                var totalMaxCapacity = operations.Sum(o => o.MaxGuests);
                var activeOperations = operations.Count;

                // Calculate total booked guests
                var totalBookedGuests = 0;
                foreach (var operation in operations)
                {
                    totalBookedGuests += await _unitOfWork.TourBookingRepository.GetTotalBookedGuestsAsync(operation.Id);
                }

                // Find next available date
                DateTime? nextAvailableDate = null;
                var futureSlots = slots.Where(s => s.TourDate > DateOnly.FromDateTime(VietnamTimeZoneUtility.GetVietnamNow()) && s.IsActive)
                                      .OrderBy(s => s.TourDate);

                foreach (var slot in futureSlots)
                {
                    if (slot.TourDetailsId.HasValue)
                    {
                        var operation = operations.FirstOrDefault(o => o.TourDetailsId == slot.TourDetailsId.Value);
                        if (operation != null)
                        {
                            var bookedGuests = await _unitOfWork.TourBookingRepository.GetTotalBookedGuestsAsync(operation.Id);
                            if (bookedGuests < operation.MaxGuests)
                            {
                                nextAvailableDate = slot.TourDate.ToDateTime(TimeOnly.MinValue);
                                break;
                            }
                        }
                    }
                }

                return new TemplateCapacitySummaryDto
                {
                    TotalSlots = totalSlots,
                    ActiveSlots = activeSlots,
                    TotalMaxCapacity = totalMaxCapacity,
                    TotalBookedGuests = totalBookedGuests,
                    NextAvailableDate = nextAvailableDate,
                    ActiveOperations = activeOperations
                };
            }
            catch (Exception ex)
            {
                // Log error and return empty summary
                return new TemplateCapacitySummaryDto();
            }
        }

        public async Task<ResponseCanDeleteDto> CanDeleteTourTemplateAsync(Guid id)
        {
            try
            {
                // Get template with related data
                var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, new[] { "TourDetails" });
                if (template == null || template.IsDeleted)
                {
                    return new ResponseCanDeleteDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour template",
                        CanDelete = false,
                        Reason = "Tour template không tồn tại"
                    };
                }

                var blockingReasons = new List<string>();

                // NEW LOGIC: Chỉ ngăn cản xóa khi có tour details được tạo sử dụng template này
                // Không quan tâm đến tour slots nữa - tour slots chỉ là dữ liệu phụ trợ
                
                // Kiểm tra xem có tour details nào được tạo từ template này không
                var existingTourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
                    td => td.TourTemplateId == id && !td.IsDeleted);

                if (existingTourDetails.Any())
                {
                    blockingReasons.Add($"Có {existingTourDetails.Count()} tour details đã được tạo sử dụng template này");
                    
                    // Phân tích chi tiết hơn về các tour details
                    var publicTourDetails = existingTourDetails.Where(td => td.Status == TourDetailsStatus.Public).ToList();
                    var draftTourDetails = existingTourDetails.Where(td => td.Status != TourDetailsStatus.Public).ToList();

                    if (publicTourDetails.Any())
                    {
                        blockingReasons.Add($"Trong đó có {publicTourDetails.Count()} tour details đang ở trạng thái Public");
                        
                        // Kiểm tra bookings cho các tour details public
                        var totalConfirmedBookings = 0;
                        var totalPendingBookings = 0;

                        foreach (var tourDetail in publicTourDetails)
                        {
                            // Get tour operation for this tour detail
                            var operation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(tourDetail.Id);
                            if (operation != null)
                            {
                                // Get active bookings (Confirmed or Pending status)
                                var bookings = await _unitOfWork.TourBookingRepository.GetAllAsync(
                                    b => b.TourOperationId == operation.Id && 
                                         !b.IsDeleted && 
                                         (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending));

                                var confirmedBookings = bookings.Where(b => b.Status == BookingStatus.Confirmed).Count();
                                var pendingBookings = bookings.Where(b => b.Status == BookingStatus.Pending).Count();

                                totalConfirmedBookings += confirmedBookings;
                                totalPendingBookings += pendingBookings;
                            }
                        }

                        if (totalConfirmedBookings > 0)
                        {
                            blockingReasons.Add($"Có {totalConfirmedBookings} booking đã được khách hàng xác nhận");
                        }
                        if (totalPendingBookings > 0)
                        {
                            blockingReasons.Add($"Có {totalPendingBookings} booking đang chờ xử lý từ khách hàng");
                        }
                    }

                    if (draftTourDetails.Any())
                    {
                        blockingReasons.Add($"Có {draftTourDetails.Count()} tour details đang ở trạng thái Draft/WaitToPublic");
                    }

                    // Đưa ra hướng dẫn cụ thể
                    blockingReasons.Add("Vui lòng xóa tất cả tour details liên quan trước khi xóa template");
                    blockingReasons.Add("Hoặc chuyển các tour details sang sử dụng template khác");
                }

                // NOTE: không còn kiểm tra tour slots nữa
                // Tour slots là dữ liệu phụ trợ và có thể tồn tại mà không ảnh hưởng đến việc xóa template
                // Chỉ khi có tour details (tour thực tế) thì mới ngăn cản xóa

                // Determine if can delete - chỉ dựa trên tour details, không quan tâm đến slots
                bool canDelete = !blockingReasons.Any();

                return new ResponseCanDeleteDto
                {
                    StatusCode = canDelete ? 200 : 409,
                    Message = canDelete ? "Có thể xóa tour template" : "Không thể xóa tour template vì có tour details đang sử dụng",
                    CanDelete = canDelete,
                    Reason = canDelete 
                        ? "Template này chỉ có tour slots và có thể xóa an toàn" 
                        : "Tour template này đang được sử dụng bởi các tour details và không thể xóa",
                    BlockingReasons = blockingReasons
                };
            }
            catch (Exception ex)
            {
                return new ResponseCanDeleteDto
                {
                    StatusCode = 500,
                    Message = "Lỗi khi kiểm tra khả năng xóa tour template: " + ex.Message,
                    CanDelete = false,
                    Reason = "Lỗi hệ thống: " + ex.Message
                };
            }
        }

        /// <summary>
        /// Check if tour template can be updated - prevent any updates if it has tour details using the template
        /// </summary>
        public async Task<ResponseCanUpdateDto> CanUpdateTourTemplateAsync(Guid id)
        {
            try
            {
                // Get template with related data
                var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, new[] { "TourDetails" });
                if (template == null || template.IsDeleted)
                {
                    return new ResponseCanUpdateDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour template",
                        CanUpdate = false,
                        Reason = "Tour template không tồn tại"
                    };
                }

                var blockingReasons = new List<string>();

                // NEW LOGIC: Ngăn cản cập nhật khi có bất kỳ tour details nào được tạo sử dụng template này
                // Không chỉ riêng Public như trước đây - bất kỳ trạng thái nào cũng ngăn cản update
                
                // Kiểm tra xem có tour details nào được tạo từ template này không
                var existingTourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
                    td => td.TourTemplateId == id && !td.IsDeleted);

                if (existingTourDetails.Any())
                {
                    blockingReasons.Add($"Có {existingTourDetails.Count()} tour details đã được tạo sử dụng template này");
                    
                    // Phân tích chi tiết hơn về các tour details
                    var publicTourDetails = existingTourDetails.Where(td => td.Status == TourDetailsStatus.Public).ToList();
                    var draftTourDetails = existingTourDetails.Where(td => td.Status != TourDetailsStatus.Public).ToList();

                    if (publicTourDetails.Any())
                    {
                        blockingReasons.Add($"Trong đó có {publicTourDetails.Count()} tour details đang ở trạng thái Public");
                        
                        // Kiểm tra bookings cho các tour details public
                        var totalConfirmedBookings = 0;
                        var totalPendingBookings = 0;

                        foreach (var tourDetail in publicTourDetails)
                        {
                            // Get tour operation for this tour detail
                            var operation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(tourDetail.Id);
                            if (operation != null)
                            {
                                // Get all active bookings
                                var bookings = await _unitOfWork.TourBookingRepository.GetAllAsync(
                                    b => b.TourOperationId == operation.Id && 
                                         !b.IsDeleted && 
                                         (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending));

                                var confirmedBookings = bookings.Where(b => b.Status == BookingStatus.Confirmed).Count();
                                var pendingBookings = bookings.Where(b => b.Status == BookingStatus.Pending).Count();

                                totalConfirmedBookings += confirmedBookings;
                                totalPendingBookings += pendingBookings;
                            }
                        }

                        // Thêm thông tin về bookings nếu có
                        if (totalConfirmedBookings > 0)
                        {
                            blockingReasons.Add($"Đặc biệt nghiêm trọng: Có {totalConfirmedBookings} booking đã được khách hàng xác nhận");
                        }
                        if (totalPendingBookings > 0)
                        {
                            blockingReasons.Add($"Có {totalPendingBookings} booking đang chờ xác nhận từ khách hàng");
                        }
                    }

                    if (draftTourDetails.Any())
                    {
                        blockingReasons.Add($"Có {draftTourDetails.Count()} tour details đang ở trạng thái Draft/WaitToPublic");
                    }

                    // Đưa ra hướng dẫn cụ thể
                    blockingReasons.Add("Việc cập nhật template có thể ảnh hưởng đến các tour details đã được tạo");
                    blockingReasons.Add("Vui lòng xóa tất cả tour details liên quan trước khi cập nhật template");
                    blockingReasons.Add("Hoặc chuyển các tour details sang sử dụng template khác");
                }

                // NOTE: Không còn kiểm tra tour slots nữa
                // Tour slots là dữ liệu phụ trợ và không ngăn cản việc cập nhật template
                // Chỉ khi có tour details (tour thực tế) thì mới ngăn cản cập nhật

                // Determine if can update - chỉ dựa trên tour details, không quan tâm đến slots
                bool canUpdate = !blockingReasons.Any();

                return new ResponseCanUpdateDto
                {
                    StatusCode = canUpdate ? 200 : 409,
                    Message = canUpdate ? "Có thể cập nhật tour template" : "Không thể cập nhật tour template vì có tour details đang sử dụng",
                    CanUpdate = canUpdate,
                    Reason = canUpdate 
                        ? "Template này chỉ có tour slots và có thể cập nhật an toàn" 
                        : "Tour template này đang được sử dụng bởi các tour details và không thể cập nhật",
                    BlockingReasons = blockingReasons
                };
            }
            catch (Exception ex)
            {
                return new ResponseCanUpdateDto
                {
                    StatusCode = 500,
                    Message = "Lỗi khi kiểm tra khả năng cập nhật tour template: " + ex.Message,
                    CanUpdate = false,
                    Reason = "Lỗi hệ thống: " + ex.Message
                };
            }
        }

        #endregion

        #region Additional Required Methods

        public async Task<TourTemplate?> GetTourTemplateWithDetailsAsync(Guid id)
        {
            return await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, new[] { "CreatedBy", "UpdatedBy", "Images", "TourDetails", "TourSlots" });
        }

        public async Task<IEnumerable<TourTemplate>> GetTourTemplatesByCreatedByAsync(Guid createdById, bool includeInactive = false)
        {
            var templates = await _unitOfWork.TourTemplateRepository.GetAllAsync(t =>
                t.CreatedById == createdById &&
                !t.IsDeleted &&
                (includeInactive || t.IsActive));
            return templates;
        }

        public async Task<IEnumerable<TourTemplate>> GetTourTemplatesByTypeAsync(TourTemplateType templateType, bool includeInactive = false)
        {
            var templates = await _unitOfWork.TourTemplateRepository.GetAllAsync(t =>
                t.TemplateType == templateType &&
                !t.IsDeleted &&
                (includeInactive || t.IsActive));
            return templates;
        }

        public async Task<IEnumerable<TourTemplate>> SearchTourTemplatesAsync(string keyword, bool includeInactive = false)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<TourTemplate>();
            }

            var templates = await _unitOfWork.TourTemplateRepository.GetAllAsync(t =>
                (t.Title.Contains(keyword) || t.StartLocation.Contains(keyword)) &&
                !t.IsDeleted &&
                (includeInactive || t.IsActive));
            return templates;
        }

        public async Task<ResponseGetTourTemplatesDto> GetTourTemplatesPaginatedAsync(
            int pageIndex,
            int pageSize,
            TourTemplateType? templateType = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            string? startLocation = null,
            bool includeInactive = false)
        {
            try
            {
                // Build query
                var query = await _unitOfWork.TourTemplateRepository.GetAllAsync(t => !t.IsDeleted && (includeInactive || t.IsActive), new[] { "Images" });

                // Apply filters
                if (templateType.HasValue)
                {
                    query = query.Where(t => t.TemplateType == templateType.Value);
                }

                if (!string.IsNullOrWhiteSpace(startLocation))
                {
                    query = query.Where(t => t.StartLocation.Contains(startLocation));
                }

                var totalCount = query.Count();
                var templates = query.Skip(pageIndex * pageSize).Take(pageSize).ToList();

                var tourTemplateDtos = _mapper.Map<List<TourTemplateSummaryDto>>(templates);

                // Add capacity information for each template
                foreach (var dto in tourTemplateDtos)
                {
                    dto.CapacitySummary = await CalculateTemplateCapacityAsync(dto.Id);
                }

                return new ResponseGetTourTemplatesDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách tour templates thành công",
                    Data = tourTemplateDtos,
                    TotalRecord = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                return new ResponseGetTourTemplatesDto
                {
                    StatusCode = 500,
                    Message = "Lỗi khi lấy danh sách tour templates",
                    Data = new List<TourTemplateSummaryDto>(),
                    TotalRecord = 0,
                    TotalPages = 0
                };
            }
        }

        public async Task<IEnumerable<TourTemplate>> GetPopularTourTemplatesAsync(int top = 10)
        {
            // For now, return most recent active templates
            // TODO: Implement proper popularity logic based on bookings
            var templates = await _unitOfWork.TourTemplateRepository.GetAllAsync(t =>
                !t.IsDeleted && t.IsActive);
            return templates.OrderByDescending(t => t.CreatedAt).Take(top);
        }

        public async Task<ResponseSetActiveStatusDto> SetTourTemplateActiveStatusAsync(Guid id, bool isActive, Guid updatedById)
        {
            try
            {
                var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, null);
                if (template == null || template.IsDeleted)
                {
                    return new ResponseSetActiveStatusDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour template",
                        success = false,
                        NewStatus = false
                    };
                }

                // Check permission
                var permissionCheck = TourTemplateValidator.ValidatePermission(template, updatedById, "thay đổi trạng thái");
                if (!permissionCheck.IsValid)
                {
                    return new ResponseSetActiveStatusDto
                    {
                        StatusCode = permissionCheck.StatusCode,
                        Message = permissionCheck.Message,
                        success = false,
                        NewStatus = template.IsActive
                    };
                }

                template.IsActive = isActive;
                template.UpdatedById = updatedById;
                template.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourTemplateRepository.Update(template);
                await _unitOfWork.SaveChangesAsync();

                return new ResponseSetActiveStatusDto
                {
                    StatusCode = 200,
                    Message = $"Đã {(isActive ? "kích hoạt" : "vô hiệu hóa")} tour template thành công",
                    success = true,
                    NewStatus = isActive
                };
            }
            catch (Exception ex)
            {
                return new ResponseSetActiveStatusDto
                {
                    StatusCode = 500,
                    Message = "Lỗi khi thay đổi trạng thái tour template",
                    success = false,
                    NewStatus = false
                };
            }
        }

        #endregion
    }
}
