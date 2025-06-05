using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TayNinhTourApi.BusinessLogicLayer.Common.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho quản lý TourSlot với tự động scheduling
    /// Kế thừa từ BaseService và implement ITourSlotService
    /// </summary>
    public class TourSlotService : BaseService, ITourSlotService
    {
        private readonly ILogger<TourSlotService> _logger;
        private readonly ISchedulingService _schedulingService;
        private readonly ICurrentUserService _currentUserService;

        public TourSlotService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TourSlotService> logger,
            ISchedulingService schedulingService,
            ICurrentUserService currentUserService) : base(mapper, unitOfWork)
        {
            _logger = logger;
            _schedulingService = schedulingService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Tự động tạo tour slots cho một tháng dựa trên tour template
        /// </summary>
        public async Task<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot.ResponseGenerateSlotsDto> GenerateSlotsAsync(RequestGenerateSlotsDto request)
        {
            try
            {
                _logger.LogInformation("Starting generate slots for template {TemplateId}, month {Month}/{Year}",
                    request.TourTemplateId, request.Month, request.Year);

                var response = new TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot.ResponseGenerateSlotsDto();

                // Validate tour template exists
                var tourTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(request.TourTemplateId);
                if (tourTemplate == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Tour template không tồn tại";
                    return response;
                }

                // Validate template has valid ScheduleDays
                var templateValidation = TourTemplateScheduleValidator.ValidateScheduleDay(tourTemplate.ScheduleDays);
                if (!templateValidation.IsValid)
                {
                    response.IsSuccess = false;
                    response.Message = $"Template có ngày không hợp lệ: {templateValidation.ErrorMessage}";
                    return response;
                }

                // Validate input using SchedulingService with template's ScheduleDays
                var validation = _schedulingService.ValidateScheduleInput(request.Year, request.Month, tourTemplate.ScheduleDays);
                if (!validation.IsValid)
                {
                    response.IsSuccess = false;
                    response.Message = validation.Message;
                    response.Errors = validation.ValidationErrors;
                    return response;
                }

                // Calculate weekend dates for the month using template's ScheduleDays
                var weekendDates = _schedulingService.CalculateWeekendDates(request.Year, request.Month, tourTemplate.ScheduleDays);

                // Log để debug
                _logger.LogInformation("Generating slots for template {TemplateId} on {ScheduleDay} for {Month}/{Year}",
                    tourTemplate.Id, tourTemplate.ScheduleDays, request.Month, request.Year);

                if (!weekendDates.Any())
                {
                    response.IsSuccess = false;
                    response.Message = "Không có ngày weekend nào trong tháng được chọn";
                    return response;
                }

                // Check existing slots if not overwriting
                var existingSlots = new List<TourSlot>();
                if (!request.OverwriteExisting)
                {
                    existingSlots = (await _unitOfWork.TourSlotRepository.GetByTourTemplateAsync(request.TourTemplateId))
                        .Where(s => weekendDates.Contains(s.TourDate))
                        .ToList();
                }

                var createdSlots = new List<TourSlot>();
                var skippedSlots = new List<SkippedSlotInfo>();
                var errors = new List<string>();

                foreach (var date in weekendDates)
                {
                    try
                    {
                        // Check if slot already exists
                        var existingSlot = existingSlots.FirstOrDefault(s => s.TourDate == date);
                        if (existingSlot != null && !request.OverwriteExisting)
                        {
                            skippedSlots.Add(new SkippedSlotInfo
                            {
                                Date = date,
                                Reason = "Slot đã tồn tại"
                            });
                            continue;
                        }

                        // Create new slot
                        var currentUserId = _currentUserService.GetCurrentUserId();
                        if (currentUserId == Guid.Empty)
                        {
                            errors.Add($"Không thể xác định user hiện tại để tạo slot cho ngày {date:dd/MM/yyyy}");
                            continue;
                        }

                        var newSlot = new TourSlot
                        {
                            Id = Guid.NewGuid(),
                            TourTemplateId = request.TourTemplateId,
                            TourDate = date,
                            ScheduleDay = GetScheduleDay(date),
                            Status = TourSlotStatus.Available,
                            IsActive = request.AutoActivate,
                            CreatedAt = DateTime.UtcNow,
                            CreatedById = currentUserId
                        };

                        if (existingSlot != null && request.OverwriteExisting)
                        {
                            // Update existing slot
                            existingSlot.Status = TourSlotStatus.Available;
                            existingSlot.IsActive = request.AutoActivate;
                            existingSlot.UpdatedAt = DateTime.UtcNow;
                            existingSlot.UpdatedById = currentUserId;

                            await _unitOfWork.TourSlotRepository.UpdateAsync(existingSlot);
                            createdSlots.Add(existingSlot);
                        }
                        else
                        {
                            // Add new slot
                            await _unitOfWork.TourSlotRepository.AddAsync(newSlot);
                            createdSlots.Add(newSlot);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error creating slot for date {Date}", date);
                        errors.Add($"Lỗi tạo slot cho ngày {date:dd/MM/yyyy}: {ex.Message}");
                    }
                }

                // Save changes
                if (createdSlots.Any())
                {
                    await _unitOfWork.SaveChangesAsync();
                }

                // Map response
                response.IsSuccess = true;
                response.Message = $"Đã tạo thành công {createdSlots.Count} slots";
                response.CreatedSlotsCount = createdSlots.Count;
                response.SkippedSlotsCount = skippedSlots.Count;
                response.FailedSlotsCount = errors.Count;
                response.CreatedSlots = _mapper.Map<List<TourSlotDto>>(createdSlots);
                response.SkippedSlots = skippedSlots;
                response.Errors = errors;

                _logger.LogInformation("Generate slots completed. Created: {Created}, Skipped: {Skipped}, Failed: {Failed}",
                    response.CreatedSlotsCount, response.SkippedSlotsCount, response.FailedSlotsCount);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GenerateSlotsAsync");
                return new TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot.ResponseGenerateSlotsDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tạo slots",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        /// <summary>
        /// Preview các slots sẽ được tạo trước khi commit vào database
        /// </summary>
        public async Task<ResponsePreviewSlotsDto> PreviewSlotsAsync(RequestPreviewSlotsDto request)
        {
            try
            {
                _logger.LogInformation("Previewing slots for template {TemplateId}, month {Month}/{Year}",
                    request.TourTemplateId, request.Month, request.Year);

                var response = new ResponsePreviewSlotsDto();

                // Validate tour template exists
                var tourTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(request.TourTemplateId);
                if (tourTemplate == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Tour template không tồn tại";
                    return response;
                }

                // Validate template has valid ScheduleDays
                var templateValidation = TourTemplateScheduleValidator.ValidateScheduleDay(tourTemplate.ScheduleDays);
                if (!templateValidation.IsValid)
                {
                    response.IsSuccess = false;
                    response.Message = $"Template có ngày không hợp lệ: {templateValidation.ErrorMessage}";
                    return response;
                }

                // Validate input using SchedulingService with template's ScheduleDays
                var validation = _schedulingService.ValidateScheduleInput(request.Year, request.Month, tourTemplate.ScheduleDays);
                if (!validation.IsValid)
                {
                    response.IsSuccess = false;
                    response.Message = validation.Message;
                    return response;
                }

                // Calculate weekend dates for the month using template's ScheduleDays
                var weekendDates = _schedulingService.CalculateWeekendDates(request.Year, request.Month, tourTemplate.ScheduleDays);

                if (!weekendDates.Any())
                {
                    response.IsSuccess = false;
                    response.Message = "Không có ngày weekend nào trong tháng được chọn";
                    return response;
                }

                // Check existing slots
                var existingSlots = (await _unitOfWork.TourSlotRepository.GetByTourTemplateAsync(request.TourTemplateId))
                    .Where(s => weekendDates.Contains(s.TourDate))
                    .ToList();

                // Build preview data
                var slotsToCreate = new List<PreviewSlotInfo>();
                var existingSlotsInfo = new List<PreviewSlotInfo>();

                foreach (var date in weekendDates)
                {
                    var existingSlot = existingSlots.FirstOrDefault(s => s.TourDate == date);
                    var previewInfo = new PreviewSlotInfo
                    {
                        TourDate = date,
                        ScheduleDay = GetScheduleDay(date),
                        ScheduleDayName = GetScheduleDayName(GetScheduleDay(date)),
                        IsExisting = existingSlot != null,
                        ExistingSlotId = existingSlot?.Id
                    };

                    if (existingSlot != null)
                    {
                        existingSlotsInfo.Add(previewInfo);
                    }
                    else
                    {
                        slotsToCreate.Add(previewInfo);
                    }
                }

                // Build month info
                var monthInfo = new MonthInfo
                {
                    Month = request.Month,
                    Year = request.Year,
                    MonthName = CultureInfo.GetCultureInfo("vi-VN").DateTimeFormat.GetMonthName(request.Month),
                    TotalDaysInMonth = DateTime.DaysInMonth(request.Year, request.Month),
                    TotalWeekendsInMonth = weekendDates.Count
                };

                response.IsSuccess = true;
                response.Message = $"Preview thành công cho tháng {request.Month}/{request.Year}";
                response.TotalSlotsToCreate = slotsToCreate.Count;
                response.ExistingSlotsCount = existingSlotsInfo.Count;
                response.SlotsToCreate = slotsToCreate;
                response.ExistingSlots = existingSlotsInfo;
                response.MonthInfo = monthInfo;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PreviewSlotsAsync");
                return new ResponsePreviewSlotsDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi preview slots"
                };
            }
        }



        /// <summary>
        /// Chuyển đổi DayOfWeek sang ScheduleDay enum
        /// </summary>
        private ScheduleDay GetScheduleDay(DateOnly date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Saturday => ScheduleDay.Saturday,
                DayOfWeek.Sunday => ScheduleDay.Sunday,
                _ => throw new ArgumentException($"Date {date} is not a weekend day")
            };
        }

        /// <summary>
        /// Lấy tên tiếng Việt của ngày trong tuần
        /// </summary>
        private string GetScheduleDayName(ScheduleDay scheduleDay)
        {
            return scheduleDay switch
            {
                ScheduleDay.Saturday => "Thứ 7",
                ScheduleDay.Sunday => "Chủ nhật",
                _ => scheduleDay.ToString()
            };
        }

        /// <summary>
        /// Lấy danh sách tour slots với filtering và pagination
        /// </summary>
        public async Task<ResponseGetSlotsDto> GetSlotsAsync(RequestGetSlotsDto request)
        {
            try
            {
                _logger.LogInformation("Getting slots with filters: TemplateId={TemplateId}, FromDate={FromDate}, ToDate={ToDate}",
                    request.TourTemplateId, request.FromDate, request.ToDate);

                var response = new ResponseGetSlotsDto();

                // Get all slots with basic filtering
                var allSlots = await _unitOfWork.TourSlotRepository.GetAvailableSlotsAsync(
                    request.TourTemplateId,
                    request.ScheduleDay,
                    request.FromDate,
                    request.ToDate,
                    request.IncludeInactive);

                var query = allSlots.AsQueryable();

                // Apply additional filters
                if (request.Status.HasValue)
                {
                    query = query.Where(s => s.Status == request.Status.Value);
                }

                if (!string.IsNullOrEmpty(request.SearchKeyword))
                {
                    query = query.Where(s => s.TourTemplate != null &&
                                           s.TourTemplate.Title.Contains(request.SearchKeyword, StringComparison.OrdinalIgnoreCase));
                }

                // Apply sorting
                query = request.SortBy.ToLower() switch
                {
                    "tourdate" => request.SortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(s => s.TourDate)
                        : query.OrderBy(s => s.TourDate),
                    "createdat" => request.SortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(s => s.CreatedAt)
                        : query.OrderBy(s => s.CreatedAt),
                    "status" => request.SortOrder.ToLower() == "desc"
                        ? query.OrderByDescending(s => s.Status)
                        : query.OrderBy(s => s.Status),
                    _ => query.OrderBy(s => s.TourDate)
                };

                // Calculate pagination
                var totalItems = query.Count();
                var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);
                var skip = (request.PageIndex - 1) * request.PageSize;

                var slots = query.Skip(skip).Take(request.PageSize).ToList();

                // Map to DTOs
                var slotDtos = _mapper.Map<List<TourSlotDto>>(slots);

                response.IsSuccess = true;
                response.Message = $"Lấy danh sách slots thành công";
                response.Slots = slotDtos;
                response.Pagination = new PaginationInfo
                {
                    CurrentPage = request.PageIndex,
                    PageSize = request.PageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageIndex > 1,
                    HasNextPage = request.PageIndex < totalPages
                };
                response.AppliedFilters = new FilterInfo
                {
                    TourTemplateId = request.TourTemplateId,
                    FromDate = request.FromDate,
                    ToDate = request.ToDate,
                    Status = request.Status?.ToString(),
                    ScheduleDay = request.ScheduleDay?.ToString(),
                    SearchKeyword = request.SearchKeyword
                };

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlotsAsync");
                return new ResponseGetSlotsDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách slots"
                };
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một tour slot
        /// </summary>
        public async Task<ResponseGetSlotDetailDto> GetSlotDetailAsync(Guid slotId)
        {
            try
            {
                _logger.LogInformation("Getting slot detail for ID: {SlotId}", slotId);

                var response = new ResponseGetSlotDetailDto();

                var slot = await _unitOfWork.TourSlotRepository.GetWithDetailsAsync(slotId);
                if (slot == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Tour slot không tồn tại";
                    return response;
                }

                var slotDetail = _mapper.Map<TourSlotDetailDto>(slot);

                response.IsSuccess = true;
                response.Message = "Lấy thông tin slot thành công";
                response.SlotDetail = slotDetail;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetSlotDetailAsync for ID: {SlotId}", slotId);
                return new ResponseGetSlotDetailDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy thông tin slot"
                };
            }
        }

        /// <summary>
        /// Cập nhật thông tin của một tour slot
        /// </summary>
        public async Task<BaseResposeDto> UpdateSlotAsync(Guid slotId, RequestUpdateSlotDto request)
        {
            try
            {
                _logger.LogInformation("Updating slot {SlotId}", slotId);

                var slot = await _unitOfWork.TourSlotRepository.GetByIdAsync(slotId);
                if (slot == null)
                {
                    return new BaseResposeDto
                    {
                        IsSuccess = false,
                        Message = "Tour slot không tồn tại"
                    };
                }

                // Update properties if provided
                if (request.Status.HasValue)
                {
                    slot.Status = request.Status.Value;
                }

                if (request.IsActive.HasValue)
                {
                    slot.IsActive = request.IsActive.Value;
                }

                slot.UpdatedAt = DateTime.UtcNow;
                slot.UpdatedById = _currentUserService.GetCurrentUserId();

                await _unitOfWork.TourSlotRepository.UpdateAsync(slot);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResposeDto
                {
                    IsSuccess = true,
                    Message = "Cập nhật slot thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating slot {SlotId}", slotId);
                return new BaseResposeDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi cập nhật slot"
                };
            }
        }

        /// <summary>
        /// Xóa một tour slot (soft delete)
        /// </summary>
        public async Task<BaseResposeDto> DeleteSlotAsync(Guid slotId)
        {
            try
            {
                _logger.LogInformation("Deleting slot {SlotId}", slotId);

                var slot = await _unitOfWork.TourSlotRepository.GetByIdAsync(slotId);
                if (slot == null)
                {
                    return new BaseResposeDto
                    {
                        IsSuccess = false,
                        Message = "Tour slot không tồn tại"
                    };
                }

                // TODO: Check if slot has bookings before deleting
                // if (slot.HasBookings)
                // {
                //     return new BaseResposeDto
                //     {
                //         IsSuccess = false,
                //         Message = "Không thể xóa slot đã có booking"
                //     };
                // }

                await _unitOfWork.TourSlotRepository.DeleteAsync(slot.Id);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResposeDto
                {
                    IsSuccess = true,
                    Message = "Xóa slot thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting slot {SlotId}", slotId);
                return new BaseResposeDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa slot"
                };
            }
        }

        /// <summary>
        /// Lấy danh sách slots sắp tới (upcoming)
        /// </summary>
        public async Task<ResponseGetUpcomingSlotsDto> GetUpcomingSlotsAsync(Guid? tourTemplateId = null, int top = 10)
        {
            try
            {
                _logger.LogInformation("Getting upcoming slots. TemplateId: {TemplateId}, Top: {Top}", tourTemplateId, top);

                var response = new ResponseGetUpcomingSlotsDto();

                var upcomingSlots = await _unitOfWork.TourSlotRepository.GetUpcomingSlotsAsync(tourTemplateId, top);
                var totalUpcoming = await _unitOfWork.TourSlotRepository.CountAvailableSlotsAsync(
                    tourTemplateId ?? Guid.Empty,
                    DateOnly.FromDateTime(DateTime.Today),
                    DateOnly.FromDateTime(DateTime.Today.AddYears(1)));

                var slotDtos = _mapper.Map<List<TourSlotDto>>(upcomingSlots);

                response.IsSuccess = true;
                response.Message = "Lấy danh sách upcoming slots thành công";
                response.UpcomingSlots = slotDtos;
                response.TotalUpcomingSlots = totalUpcoming;
                response.ReturnedSlotsCount = slotDtos.Count;
                response.CurrentDate = DateOnly.FromDateTime(DateTime.Today);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetUpcomingSlotsAsync");
                return new ResponseGetUpcomingSlotsDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi lấy upcoming slots"
                };
            }
        }

        /// <summary>
        /// Kiểm tra conflicts khi tạo slots mới
        /// </summary>
        public async Task<ResponseCheckSlotConflictsDto> CheckSlotConflictsAsync(Guid tourTemplateId, IEnumerable<DateOnly> dates)
        {
            try
            {
                _logger.LogInformation("Checking slot conflicts for template {TemplateId}", tourTemplateId);

                var response = new ResponseCheckSlotConflictsDto();

                var existingSlots = (await _unitOfWork.TourSlotRepository.GetByTourTemplateAsync(tourTemplateId))
                    .Where(s => dates.Contains(s.TourDate))
                    .ToList();

                var conflictDates = new List<ConflictSlotInfo>();
                var availableDates = new List<DateOnly>();

                foreach (var date in dates)
                {
                    var existingSlot = existingSlots.FirstOrDefault(s => s.TourDate == date);
                    if (existingSlot != null)
                    {
                        conflictDates.Add(new ConflictSlotInfo
                        {
                            Date = date,
                            ExistingSlotId = existingSlot.Id,
                            ExistingSlotStatus = existingSlot.Status.ToString(),
                            IsActive = existingSlot.IsActive
                        });
                    }
                    else
                    {
                        availableDates.Add(date);
                    }
                }

                response.IsSuccess = true;
                response.Message = $"Kiểm tra conflicts thành công";
                response.HasConflicts = conflictDates.Any();
                response.ConflictCount = conflictDates.Count;
                response.ConflictDates = conflictDates;
                response.AvailableDates = availableDates;

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckSlotConflictsAsync");
                return new ResponseCheckSlotConflictsDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi kiểm tra conflicts"
                };
            }
        }

        /// <summary>
        /// Bulk update status cho nhiều slots cùng lúc
        /// </summary>
        public async Task<BaseResposeDto> BulkUpdateSlotStatusAsync(RequestBulkUpdateSlotStatusDto request)
        {
            try
            {
                _logger.LogInformation("Bulk updating {Count} slots", request.SlotIds.Count);

                var slots = new List<TourSlot>();
                foreach (var slotId in request.SlotIds)
                {
                    var slot = await _unitOfWork.TourSlotRepository.GetByIdAsync(slotId);
                    if (slot != null)
                    {
                        slots.Add(slot);
                    }
                }

                if (!slots.Any())
                {
                    return new BaseResposeDto
                    {
                        IsSuccess = false,
                        Message = "Không tìm thấy slots nào để cập nhật"
                    };
                }

                foreach (var slot in slots)
                {
                    if (request.NewStatus.HasValue)
                    {
                        slot.Status = request.NewStatus.Value;
                    }

                    if (request.NewIsActive.HasValue)
                    {
                        slot.IsActive = request.NewIsActive.Value;
                    }

                    slot.UpdatedAt = DateTime.UtcNow;
                    slot.UpdatedById = Guid.Empty; // TODO: Get from current user context

                    await _unitOfWork.TourSlotRepository.UpdateAsync(slot);
                }

                await _unitOfWork.SaveChangesAsync();

                return new BaseResposeDto
                {
                    IsSuccess = true,
                    Message = $"Cập nhật thành công {slots.Count} slots"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BulkUpdateSlotStatusAsync");
                return new BaseResposeDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi bulk update slots"
                };
            }
        }
    }
}
