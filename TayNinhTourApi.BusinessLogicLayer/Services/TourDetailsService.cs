using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Globalization;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho quản lý timeline chi tiết của tour template
    /// Cung cấp các operations để thêm, sửa, xóa và sắp xếp lại timeline items
    /// </summary>
    public class TourDetailsService : BaseService, ITourDetailsService
    {
        private readonly ILogger<TourDetailsService> _logger;

        public TourDetailsService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TourDetailsService> logger) 
            : base(mapper, unitOfWork)
        {
            _logger = logger;
        }

        public async Task<ResponseGetTimelineDto> GetTimelineAsync(RequestGetTimelineDto request)
        {
            try
            {
                _logger.LogInformation("Getting timeline for TourTemplate {TourTemplateId}", request.TourTemplateId);

                // Kiểm tra tour template tồn tại
                var tourTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(request.TourTemplateId);
                if (tourTemplate == null || tourTemplate.IsDeleted)
                {
                    return new ResponseGetTimelineDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour template"
                    };
                }

                // Lấy timeline details
                var tourDetails = await _unitOfWork.TourDetailsRepository
                    .GetByTourTemplateOrderedAsync(request.TourTemplateId, request.IncludeInactive);

                // Map to DTOs
                var timelineItems = _mapper.Map<List<TourDetailDto>>(tourDetails);

                var timelineDto = new TimelineDto
                {
                    TourTemplateId = request.TourTemplateId,
                    TourTemplateTitle = tourTemplate.Title,
                    Items = timelineItems,
                    TotalItems = timelineItems.Count,
                    TotalDuration = CalculateTotalDuration(timelineItems)
                };

                return new ResponseGetTimelineDto
                {
                    StatusCode = 200,
                    Message = "Lấy timeline thành công",
                    Data = timelineDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline for TourTemplate {TourTemplateId}", request.TourTemplateId);
                return new ResponseGetTimelineDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy timeline"
                };
            }
        }

        public async Task<ResponseCreateTourDetailDto> AddTimelineItemAsync(RequestCreateTourDetailDto request, Guid createdById)
        {
            try
            {
                _logger.LogInformation("Adding timeline item for TourTemplate {TourTemplateId}", request.TourTemplateId);

                // Validate request
                var validationResult = await ValidateCreateRequest(request);
                if (!validationResult.IsValid)
                {
                    return new ResponseCreateTourDetailDto
                    {
                        StatusCode = 400,
                        Message = "Dữ liệu không hợp lệ",
                        ValidationErrors = validationResult.Errors
                    };
                }

                // Use TimeSlot directly from request
                var timeSlot = request.TimeSlot;

                // Determine sort order
                int sortOrder;
                if (request.SortOrder.HasValue)
                {
                    sortOrder = request.SortOrder.Value;
                    // Update existing items' sort order if needed
                    await _unitOfWork.TourDetailsRepository.UpdateSortOrdersAsync(request.TourTemplateId, sortOrder, 1);
                }
                else
                {
                    // Auto-assign to end
                    var lastDetail = await _unitOfWork.TourDetailsRepository.GetLastDetailAsync(request.TourTemplateId);
                    sortOrder = (lastDetail?.SortOrder ?? 0) + 1;
                }

                // Create new tour detail
                var tourDetail = new TourDetails
                {
                    Id = Guid.NewGuid(),
                    TourTemplateId = request.TourTemplateId,
                    TimeSlot = timeSlot,
                    Location = request.Location,
                    Description = request.Description,
                    ShopId = request.ShopId,
                    SortOrder = sortOrder,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _unitOfWork.TourDetailsRepository.AddAsync(tourDetail);
                await _unitOfWork.SaveChangesAsync();

                // Get created item with relationships
                var createdDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetail.Id);
                var tourDetailDto = _mapper.Map<TourDetailDto>(createdDetail);

                _logger.LogInformation("Successfully added timeline item {TourDetailId}", tourDetail.Id);

                return new ResponseCreateTourDetailDto
                {
                    StatusCode = 201,
                    Message = "Thêm mốc thời gian thành công",
                    Data = tourDetailDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding timeline item for TourTemplate {TourTemplateId}", request.TourTemplateId);
                return new ResponseCreateTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi thêm mốc thời gian"
                };
            }
        }

        public async Task<ResponseUpdateTourDetailDto> UpdateTimelineItemAsync(Guid tourDetailId, RequestUpdateTourDetailDto request, Guid updatedById)
        {
            try
            {
                _logger.LogInformation("Updating timeline item {TourDetailId}", tourDetailId);

                // Get existing tour detail
                var existingDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailId);
                if (existingDetail == null)
                {
                    return new ResponseUpdateTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy mốc thời gian này"
                    };
                }

                // Validate update request
                var validationResult = await ValidateUpdateRequest(request, existingDetail);
                if (!validationResult.IsValid)
                {
                    return new ResponseUpdateTourDetailDto
                    {
                        StatusCode = 400,
                        Message = "Dữ liệu không hợp lệ",
                        ValidationErrors = validationResult.Errors
                    };
                }

                // Update fields
                if (request.TimeSlot.HasValue)
                {
                    existingDetail.TimeSlot = request.TimeSlot.Value;
                }

                if (request.Location != null)
                    existingDetail.Location = request.Location;

                if (request.Description != null)
                    existingDetail.Description = request.Description;

                if (request.ShopId.HasValue)
                    existingDetail.ShopId = request.ShopId;

                existingDetail.UpdatedById = updatedById;
                existingDetail.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourDetailsRepository.UpdateAsync(existingDetail);
                await _unitOfWork.SaveChangesAsync();

                // Get updated item with relationships
                var updatedDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailId);
                var tourDetailDto = _mapper.Map<TourDetailDto>(updatedDetail);

                _logger.LogInformation("Successfully updated timeline item {TourDetailId}", tourDetailId);

                return new ResponseUpdateTourDetailDto
                {
                    StatusCode = 200,
                    Message = "Cập nhật mốc thời gian thành công",
                    Data = tourDetailDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating timeline item {TourDetailId}", tourDetailId);
                return new ResponseUpdateTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi cập nhật mốc thời gian"
                };
            }
        }

        public async Task<ResponseDeleteTourDetailDto> DeleteTimelineItemAsync(Guid tourDetailId, Guid deletedById)
        {
            try
            {
                _logger.LogInformation("Deleting timeline item {TourDetailId}", tourDetailId);

                // Get existing tour detail
                var existingDetail = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailId);
                if (existingDetail == null || existingDetail.IsDeleted)
                {
                    return new ResponseDeleteTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy mốc thời gian này"
                    };
                }

                // Check if can delete
                var canDelete = await CanDeleteTimelineItemAsync(tourDetailId);
                if (!canDelete)
                {
                    return new ResponseDeleteTourDetailDto
                    {
                        StatusCode = 400,
                        Message = "Không thể xóa mốc thời gian này do ràng buộc nghiệp vụ"
                    };
                }

                // Soft delete
                existingDetail.IsDeleted = true;
                existingDetail.DeletedAt = DateTime.UtcNow;
                existingDetail.UpdatedById = deletedById;
                existingDetail.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourDetailsRepository.UpdateAsync(existingDetail);

                // Reorder remaining items
                await _unitOfWork.TourDetailsRepository.UpdateSortOrdersAsync(
                    existingDetail.TourTemplateId, 
                    existingDetail.SortOrder + 1, 
                    -1);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted timeline item {TourDetailId}", tourDetailId);

                return new ResponseDeleteTourDetailDto
                {
                    StatusCode = 200,
                    Message = "Xóa mốc thời gian thành công",
                    Success = true,
                    DeletedId = tourDetailId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting timeline item {TourDetailId}", tourDetailId);
                return new ResponseDeleteTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi xóa mốc thời gian"
                };
            }
        }

        public async Task<ResponseReorderTimelineDto> ReorderTimelineAsync(RequestReorderTimelineDto request, Guid updatedById)
        {
            try
            {
                _logger.LogInformation("Reordering timeline for TourTemplate {TourTemplateId}", request.TourTemplateId);

                // Validate tour template exists
                var tourTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(request.TourTemplateId);
                if (tourTemplate == null || tourTemplate.IsDeleted)
                {
                    return new ResponseReorderTimelineDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour template"
                    };
                }

                // Get existing details
                var existingDetails = await _unitOfWork.TourDetailsRepository
                    .GetByTourTemplateOrderedAsync(request.TourTemplateId, false);

                var existingIds = existingDetails.Select(d => d.Id).ToList();
                var requestedIds = request.OrderedDetailIds;

                // Validate all IDs exist and belong to the template
                if (!requestedIds.All(id => existingIds.Contains(id)) || requestedIds.Count != existingIds.Count)
                {
                    return new ResponseReorderTimelineDto
                    {
                        StatusCode = 400,
                        Message = "Danh sách ID không hợp lệ hoặc không đầy đủ"
                    };
                }

                // Update sort orders
                for (int i = 0; i < requestedIds.Count; i++)
                {
                    var detail = existingDetails.First(d => d.Id == requestedIds[i]);
                    detail.SortOrder = i + 1;
                    detail.UpdatedById = updatedById;
                    detail.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.TourDetailsRepository.UpdateAsync(detail);
                }

                await _unitOfWork.SaveChangesAsync();

                // Get updated timeline
                var updatedDetails = await _unitOfWork.TourDetailsRepository
                    .GetByTourTemplateOrderedAsync(request.TourTemplateId, false);

                var timelineItems = _mapper.Map<List<TourDetailDto>>(updatedDetails);
                var timelineDto = new TimelineDto
                {
                    TourTemplateId = request.TourTemplateId,
                    TourTemplateTitle = tourTemplate.Title,
                    Items = timelineItems,
                    TotalItems = timelineItems.Count,
                    TotalDuration = CalculateTotalDuration(timelineItems)
                };

                _logger.LogInformation("Successfully reordered timeline for TourTemplate {TourTemplateId}", request.TourTemplateId);

                return new ResponseReorderTimelineDto
                {
                    StatusCode = 200,
                    Message = "Sắp xếp lại timeline thành công",
                    Data = timelineDto,
                    ReorderedCount = requestedIds.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering timeline for TourTemplate {TourTemplateId}", request.TourTemplateId);
                return new ResponseReorderTimelineDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi sắp xếp lại timeline"
                };
            }
        }

        public async Task<ResponseGetAvailableShopsDto> GetAvailableShopsAsync(bool includeInactive = false, string? searchKeyword = null)
        {
            try
            {
                _logger.LogInformation("Getting available shops, includeInactive: {IncludeInactive}, searchKeyword: {SearchKeyword}",
                    includeInactive, searchKeyword);

                var shops = await _unitOfWork.ShopRepository.GetAllAsync(includeInactive);

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchKeyword))
                {
                    shops = shops.Where(s =>
                        s.Name.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ||
                        s.Location.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase) ||
                        (s.Description != null && s.Description.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase))
                    );
                }

                var shopSummaries = shops.Select(s => new ShopSummaryDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Location = s.Location,
                    Description = s.Description,
                    PhoneNumber = s.PhoneNumber,
                    IsActive = s.IsActive
                }).OrderBy(s => s.Name).ToList();

                return new ResponseGetAvailableShopsDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách shops thành công",
                    Data = shopSummaries,
                    TotalCount = shopSummaries.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available shops");
                return new ResponseGetAvailableShopsDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách shops"
                };
            }
        }

        public async Task<ResponseValidateTimelineDto> ValidateTimelineAsync(Guid tourTemplateId)
        {
            try
            {
                _logger.LogInformation("Validating timeline for TourTemplate {TourTemplateId}", tourTemplateId);

                var errors = new List<string>();
                var warnings = new List<string>();

                // Get timeline details
                var details = await _unitOfWork.TourDetailsRepository
                    .GetByTourTemplateOrderedAsync(tourTemplateId, false);

                if (!details.Any())
                {
                    warnings.Add("Timeline chưa có mốc thời gian nào");
                }
                else
                {
                    // Check for duplicate time slots
                    var timeSlots = details.Select(d => d.TimeSlot).ToList();
                    var duplicates = timeSlots.GroupBy(t => t).Where(g => g.Count() > 1).Select(g => g.Key);

                    foreach (var duplicate in duplicates)
                    {
                        errors.Add($"Có nhiều hơn một hoạt động tại thời gian {duplicate:HH:mm}");
                    }

                    // Check for logical time progression
                    for (int i = 1; i < details.Count(); i++)
                    {
                        var current = details.ElementAt(i);
                        var previous = details.ElementAt(i - 1);

                        if (current.TimeSlot <= previous.TimeSlot)
                        {
                            warnings.Add($"Thời gian tại vị trí {i + 1} ({current.TimeSlot:HH:mm}) không theo thứ tự tăng dần");
                        }
                    }

                    // Check for missing essential information
                    var itemsWithoutLocation = details.Where(d => string.IsNullOrEmpty(d.Location)).Count();
                    if (itemsWithoutLocation > 0)
                    {
                        warnings.Add($"Có {itemsWithoutLocation} mốc thời gian chưa có thông tin địa điểm");
                    }
                }

                return new ResponseValidateTimelineDto
                {
                    StatusCode = 200,
                    Message = "Validation hoàn thành",
                    IsValid = errors.Count == 0,
                    ValidationErrors = errors,
                    Warnings = warnings
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating timeline for TourTemplate {TourTemplateId}", tourTemplateId);
                return new ResponseValidateTimelineDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi validate timeline"
                };
            }
        }

        // Helper methods will be added in the next part due to line limit
        private decimal CalculateTotalDuration(List<TourDetailDto> items)
        {
            if (items.Count < 2) return 0;

            var firstTime = items.First().TimeSlot;
            var lastTime = items.Last().TimeSlot;

            return (decimal)(lastTime.ToTimeSpan() - firstTime.ToTimeSpan()).TotalHours;
        }

        private async Task<(bool IsValid, List<string> Errors)> ValidateCreateRequest(RequestCreateTourDetailDto request)
        {
            var errors = new List<string>();

            // Check tour template exists
            var tourTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(request.TourTemplateId);
            if (tourTemplate == null || tourTemplate.IsDeleted)
            {
                errors.Add("Tour template không tồn tại");
            }

            // Check shop exists if provided
            if (request.ShopId.HasValue)
            {
                var shop = await _unitOfWork.ShopRepository.GetByIdAsync(request.ShopId.Value);
                if (shop == null || shop.IsDeleted)
                {
                    errors.Add("Shop không tồn tại");
                }
            }

            return (errors.Count == 0, errors);
        }

        private async Task<(bool IsValid, List<string> Errors)> ValidateUpdateRequest(RequestUpdateTourDetailDto request, TourDetails existingDetail)
        {
            var errors = new List<string>();

            // Check shop exists if provided
            if (request.ShopId.HasValue)
            {
                var shop = await _unitOfWork.ShopRepository.GetByIdAsync(request.ShopId.Value);
                if (shop == null || shop.IsDeleted)
                {
                    errors.Add("Shop không tồn tại");
                }
            }

            return (errors.Count == 0, errors);
        }

        public async Task<ResponseTimelineStatisticsDto> GetTimelineStatisticsAsync(Guid tourTemplateId)
        {
            try
            {
                _logger.LogInformation("Getting timeline statistics for TourTemplate {TourTemplateId}", tourTemplateId);

                var details = await _unitOfWork.TourDetailsRepository
                    .GetByTourTemplateOrderedAsync(tourTemplateId, false);

                if (!details.Any())
                {
                    return new ResponseTimelineStatisticsDto
                    {
                        StatusCode = 200,
                        Message = "Timeline chưa có dữ liệu",
                        Data = new TimelineStatistics()
                    };
                }

                var usedShops = details.Where(d => d.ShopId.HasValue && d.Shop != null)
                    .Select(d => new ShopSummaryDto
                    {
                        Id = d.Shop!.Id,
                        Name = d.Shop.Name,
                        Location = d.Shop.Location,
                        Description = d.Shop.Description,
                        PhoneNumber = d.Shop.PhoneNumber,
                        IsActive = d.Shop.IsActive
                    }).Distinct().ToList();

                var timeSlots = details.Select(d => d.TimeSlot).ToList();
                var totalDuration = timeSlots.Count > 1
                    ? (decimal)(timeSlots.Max().ToTimeSpan() - timeSlots.Min().ToTimeSpan()).TotalHours
                    : 0;

                var statistics = new TimelineStatistics
                {
                    TotalItems = details.Count(),
                    ItemsWithShop = details.Count(d => d.ShopId.HasValue),
                    ItemsWithoutShop = details.Count(d => !d.ShopId.HasValue),
                    EarliestTime = timeSlots.Any() ? timeSlots.Min() : null,
                    LatestTime = timeSlots.Any() ? timeSlots.Max() : null,
                    TotalDuration = totalDuration,
                    UsedShops = usedShops
                };

                return new ResponseTimelineStatisticsDto
                {
                    StatusCode = 200,
                    Message = "Lấy thống kê timeline thành công",
                    Data = statistics
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline statistics for TourTemplate {TourTemplateId}", tourTemplateId);
                return new ResponseTimelineStatisticsDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thống kê timeline"
                };
            }
        }

        public async Task<bool> CanDeleteTimelineItemAsync(Guid tourDetailId)
        {
            try
            {
                // Get the tour detail
                var tourDetail = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailId);
                if (tourDetail == null || tourDetail.IsDeleted)
                {
                    return false;
                }

                // Business rules for deletion:
                // 1. Always allow deletion for now (can be extended with more complex rules)
                // 2. Could check if there are any bookings referencing this detail
                // 3. Could check if this is a critical timeline item

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if timeline item {TourDetailId} can be deleted", tourDetailId);
                return false;
            }
        }

        public async Task<ResponseCreateTourDetailDto> DuplicateTimelineItemAsync(Guid tourDetailId, Guid createdById)
        {
            try
            {
                _logger.LogInformation("Duplicating timeline item {TourDetailId}", tourDetailId);

                // Get original tour detail
                var originalDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailId);
                if (originalDetail == null)
                {
                    return new ResponseCreateTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy mốc thời gian gốc"
                    };
                }

                // Get next sort order
                var lastDetail = await _unitOfWork.TourDetailsRepository.GetLastDetailAsync(originalDetail.TourTemplateId);
                var nextSortOrder = (lastDetail?.SortOrder ?? 0) + 1;

                // Create duplicate with modified time (add 30 minutes)
                var duplicatedDetail = new TourDetails
                {
                    Id = Guid.NewGuid(),
                    TourTemplateId = originalDetail.TourTemplateId,
                    TimeSlot = originalDetail.TimeSlot.AddMinutes(30), // Add 30 minutes to avoid conflict
                    Location = originalDetail.Location + " (Copy)",
                    Description = originalDetail.Description,
                    ShopId = originalDetail.ShopId,
                    SortOrder = nextSortOrder,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _unitOfWork.TourDetailsRepository.AddAsync(duplicatedDetail);
                await _unitOfWork.SaveChangesAsync();

                // Get created item with relationships
                var createdDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(duplicatedDetail.Id);
                var tourDetailDto = _mapper.Map<TourDetailDto>(createdDetail);

                _logger.LogInformation("Successfully duplicated timeline item {OriginalId} to {NewId}", tourDetailId, duplicatedDetail.Id);

                return new ResponseCreateTourDetailDto
                {
                    StatusCode = 201,
                    Message = "Sao chép mốc thời gian thành công",
                    Data = tourDetailDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating timeline item {TourDetailId}", tourDetailId);
                return new ResponseCreateTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi sao chép mốc thời gian"
                };
            }
        }

        public async Task<ResponseUpdateTourDetailDto> GetTimelineItemByIdAsync(Guid tourDetailId)
        {
            try
            {
                _logger.LogInformation("Getting timeline item {TourDetailId}", tourDetailId);

                var tourDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailId);
                if (tourDetail == null)
                {
                    return new ResponseUpdateTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy mốc thời gian này"
                    };
                }

                var tourDetailDto = _mapper.Map<TourDetailDto>(tourDetail);

                return new ResponseUpdateTourDetailDto
                {
                    StatusCode = 200,
                    Message = "Lấy thông tin mốc thời gian thành công",
                    Data = tourDetailDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline item {TourDetailId}", tourDetailId);
                return new ResponseUpdateTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thông tin mốc thời gian"
                };
            }
        }
    }
