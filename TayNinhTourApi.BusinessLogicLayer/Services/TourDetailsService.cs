using AutoMapper;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho quản lý lịch trình template của tour
    /// Cung cấp các operations để tạo, sửa, xóa lịch trình template
    /// </summary>
    public class TourDetailsService : BaseService, ITourDetailsService
    {
        private readonly ILogger<TourDetailsService> _logger;

        public TourDetailsService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<TourDetailsService> logger)
            : base(mapper, unitOfWork)
        {
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách lịch trình của tour template
        /// </summary>
        public async Task<ResponseGetTourDetailsDto> GetTourDetailsAsync(Guid tourTemplateId, bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting tour details for TourTemplate {TourTemplateId}", tourTemplateId);

                // Kiểm tra tour template tồn tại
                var tourTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(tourTemplateId);
                if (tourTemplate == null || tourTemplate.IsDeleted)
                {
                    return new ResponseGetTourDetailsDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour template"
                    };
                }

                // Lấy danh sách tour details
                var tourDetails = await _unitOfWork.TourDetailsRepository
                    .GetByTourTemplateOrderedAsync(tourTemplateId, includeInactive);

                // Map to DTOs
                var tourDetailDtos = _mapper.Map<List<TourDetailDto>>(tourDetails);

                return new ResponseGetTourDetailsDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách lịch trình thành công",
                    Data = tourDetailDtos,
                    TotalCount = tourDetailDtos.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour details for TourTemplate {TourTemplateId}", tourTemplateId);
                return new ResponseGetTourDetailsDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách lịch trình"
                };
            }
        }

        /// <summary>
        /// Tạo lịch trình mới cho tour template
        /// </summary>
        public async Task<ResponseCreateTourDetailDto> CreateTourDetailAsync(RequestCreateTourDetailDto request, Guid createdById)
        {
            try
            {
                _logger.LogInformation("Creating tour detail for TourTemplate {TourTemplateId}", request.TourTemplateId);

                // Validate request
                var validationResult = await ValidateCreateRequestAsync(request);
                if (!validationResult.IsValid)
                {
                    return new ResponseCreateTourDetailDto
                    {
                        StatusCode = 400,
                        Message = "Dữ liệu không hợp lệ",
                        ValidationErrors = validationResult.Errors
                    };
                }

                // Kiểm tra title đã tồn tại chưa
                var existingDetail = await _unitOfWork.TourDetailsRepository
                    .GetByTitleAsync(request.TourTemplateId, request.Title);
                if (existingDetail != null)
                {
                    return new ResponseCreateTourDetailDto
                    {
                        StatusCode = 400,
                        Message = "Tiêu đề lịch trình đã tồn tại"
                    };
                }

                // Create new tour detail
                var tourDetail = new TourDetails
                {
                    Id = Guid.NewGuid(),
                    TourTemplateId = request.TourTemplateId,
                    Title = request.Title,
                    Description = request.Description,
                    CreatedById = createdById,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                await _unitOfWork.TourDetailsRepository.AddAsync(tourDetail);
                await _unitOfWork.SaveChangesAsync();

                // AUTO-CLONE: Clone tất cả TourSlots từ TourTemplate để tái sử dụng template
                _logger.LogInformation("Cloning TourSlots from TourTemplate for TourDetails {TourDetailId}", tourDetail.Id);

                // Lấy tất cả template slots (TourDetailsId = null, là slots gốc READ-only)
                var templateSlots = await _unitOfWork.TourSlotRepository
                    .GetByTourTemplateAsync(request.TourTemplateId);

                var templatesSlotsList = templateSlots.Where(slot => slot.TourDetailsId == null).ToList();

                if (templatesSlotsList.Any())
                {
                    // CLONE template slots thành detail slots
                    var clonedSlots = new List<TourSlot>();

                    foreach (var templateSlot in templatesSlotsList)
                    {
                        var clonedSlot = new TourSlot
                        {
                            Id = Guid.NewGuid(),
                            TourTemplateId = templateSlot.TourTemplateId,
                            TourDate = templateSlot.TourDate,
                            ScheduleDay = templateSlot.ScheduleDay,
                            Status = templateSlot.Status,
                            TourDetailsId = tourDetail.Id, // ASSIGN cho detail này
                            IsActive = templateSlot.IsActive,
                            CreatedById = createdById,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _unitOfWork.TourSlotRepository.AddAsync(clonedSlot);
                        clonedSlots.Add(clonedSlot);
                    }

                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation("Successfully cloned {SlotCount} TourSlots for TourDetails {TourDetailId}",
                        clonedSlots.Count, tourDetail.Id);
                }
                else
                {
                    _logger.LogWarning("No template TourSlots found for TourTemplate {TourTemplateId}",
                        request.TourTemplateId);
                }

                // Get created item with relationships
                var createdDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetail.Id);
                var tourDetailDto = _mapper.Map<TourDetailDto>(createdDetail);

                _logger.LogInformation("Successfully created tour detail {TourDetailId}", tourDetail.Id);

                return new ResponseCreateTourDetailDto
                {
                    StatusCode = 201,
                    Message = "Tạo lịch trình thành công",
                    Data = tourDetailDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tour detail for TourTemplate {TourTemplateId}", request.TourTemplateId);
                return new ResponseCreateTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tạo lịch trình"
                };
            }
        }

        /// <summary>
        /// Cập nhật lịch trình
        /// </summary>
        public async Task<ResponseUpdateTourDetailDto> UpdateTourDetailAsync(Guid tourDetailId, RequestUpdateTourDetailDto request, Guid updatedById)
        {
            try
            {
                _logger.LogInformation("Updating tour detail {TourDetailId}", tourDetailId);

                // Get existing tour detail
                var existingDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailId);
                if (existingDetail == null)
                {
                    return new ResponseUpdateTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy lịch trình này"
                    };
                }

                // Validate update request
                var validationResult = await ValidateUpdateRequestAsync(request, existingDetail);
                if (!validationResult.IsValid)
                {
                    return new ResponseUpdateTourDetailDto
                    {
                        StatusCode = 400,
                        Message = "Dữ liệu không hợp lệ",
                        ValidationErrors = validationResult.Errors
                    };
                }

                // Kiểm tra title trùng lặp (nếu có thay đổi title)
                if (!string.IsNullOrEmpty(request.Title) && request.Title != existingDetail.Title)
                {
                    var duplicateTitle = await _unitOfWork.TourDetailsRepository
                        .ExistsByTitleAsync(existingDetail.TourTemplateId, request.Title, tourDetailId);
                    if (duplicateTitle)
                    {
                        return new ResponseUpdateTourDetailDto
                        {
                            StatusCode = 400,
                            Message = "Tiêu đề lịch trình đã tồn tại"
                        };
                    }
                }

                // Update fields
                if (!string.IsNullOrEmpty(request.Title))
                    existingDetail.Title = request.Title;

                if (request.Description != null)
                    existingDetail.Description = request.Description;

                existingDetail.UpdatedById = updatedById;
                existingDetail.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourDetailsRepository.UpdateAsync(existingDetail);
                await _unitOfWork.SaveChangesAsync();

                // Get updated item with relationships
                var updatedDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailId);
                var tourDetailDto = _mapper.Map<TourDetailDto>(updatedDetail);

                _logger.LogInformation("Successfully updated tour detail {TourDetailId}", tourDetailId);

                return new ResponseUpdateTourDetailDto
                {
                    StatusCode = 200,
                    Message = "Cập nhật lịch trình thành công",
                    Data = tourDetailDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tour detail {TourDetailId}", tourDetailId);
                return new ResponseUpdateTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi cập nhật lịch trình"
                };
            }
        }

        /// <summary>
        /// Xóa lịch trình
        /// </summary>
        public async Task<ResponseDeleteTourDetailDto> DeleteTourDetailAsync(Guid tourDetailId, Guid deletedById)
        {
            try
            {
                _logger.LogInformation("Deleting tour detail {TourDetailId}", tourDetailId);

                // Get existing tour detail
                var existingDetail = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailId);
                if (existingDetail == null || existingDetail.IsDeleted)
                {
                    return new ResponseDeleteTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy lịch trình này"
                    };
                }

                // Check if can delete
                var canDelete = await _unitOfWork.TourDetailsRepository.CanDeleteDetailAsync(tourDetailId);
                if (!canDelete)
                {
                    return new ResponseDeleteTourDetailDto
                    {
                        StatusCode = 400,
                        Message = "Không thể xóa lịch trình này do đã có slots hoặc operations được assign"
                    };
                }

                // Soft delete
                existingDetail.IsDeleted = true;
                existingDetail.DeletedAt = DateTime.UtcNow;
                existingDetail.UpdatedById = deletedById;
                existingDetail.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourDetailsRepository.UpdateAsync(existingDetail);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted tour detail {TourDetailId}", tourDetailId);

                return new ResponseDeleteTourDetailDto
                {
                    StatusCode = 200,
                    Message = "Xóa lịch trình thành công",
                    DeletedTourDetailId = tourDetailId,
                    CleanedSlotsCount = 0, // TODO: Count actual cleaned slots
                    CleanedTimelineItemsCount = 0, // TODO: Count actual cleaned timeline items
                    CleanupInfo = "Đã xóa thành công TourDetails và các dữ liệu liên quan"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting tour detail {TourDetailId}", tourDetailId);
                return new ResponseDeleteTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi xóa lịch trình"
                };
            }
        }

        /// <summary>
        /// Tìm kiếm lịch trình theo từ khóa
        /// </summary>
        public async Task<ResponseSearchTourDetailsDto> SearchTourDetailsAsync(string keyword, Guid? tourTemplateId = null, bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Searching tour details with keyword: {Keyword}", keyword);

                var tourDetails = await _unitOfWork.TourDetailsRepository
                    .SearchAsync(keyword, tourTemplateId, includeInactive);

                var tourDetailDtos = _mapper.Map<List<TourDetailDto>>(tourDetails);

                return new ResponseSearchTourDetailsDto
                {
                    StatusCode = 200,
                    Message = "Tìm kiếm lịch trình thành công",
                    Data = tourDetailDtos,
                    TotalCount = tourDetailDtos.Count,
                    SearchKeyword = keyword
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tour details with keyword: {Keyword}", keyword);
                return new ResponseSearchTourDetailsDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tìm kiếm lịch trình"
                };
            }
        }

        public async Task<ResponseGetAvailableShopsDto> GetAvailableShopsAsync(bool includeInactive = false, string? searchKeyword = null)
        {
            try
            {
                _logger.LogInformation("Getting available shops, includeInactive: {IncludeInactive}, searchKeyword: {SearchKeyword}",
                    includeInactive, searchKeyword);

                var shops = includeInactive
                    ? await _unitOfWork.ShopRepository.GetAllAsync()
                    : await _unitOfWork.ShopRepository.GetAllAsync(s => s.IsActive && !s.IsDeleted);

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

        /// <summary>
        /// Lấy lịch trình với pagination
        /// </summary>
        public async Task<ResponseGetTourDetailsPaginatedDto> GetTourDetailsPaginatedAsync(
            int pageIndex,
            int pageSize,
            Guid? tourTemplateId = null,
            string? titleFilter = null,
            bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting paginated tour details, page: {PageIndex}, size: {PageSize}", pageIndex, pageSize);

                var (tourDetails, totalCount) = await _unitOfWork.TourDetailsRepository
                    .GetPaginatedAsync(pageIndex, pageSize, tourTemplateId, titleFilter, includeInactive);

                var tourDetailDtos = _mapper.Map<List<TourDetailDto>>(tourDetails);

                return new ResponseGetTourDetailsPaginatedDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách lịch trình thành công",
                    Data = tourDetailDtos,
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting paginated tour details");
                return new ResponseGetTourDetailsPaginatedDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách lịch trình"
                };
            }
        }

        #region Helper Methods

        /// <summary>
        /// Validate request tạo tour detail
        /// </summary>
        private async Task<(bool IsValid, List<string> Errors)> ValidateCreateRequestAsync(RequestCreateTourDetailDto request)
        {
            var errors = new List<string>();

            // Kiểm tra tour template tồn tại
            var tourTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(request.TourTemplateId);
            if (tourTemplate == null || tourTemplate.IsDeleted)
            {
                errors.Add("Tour template không tồn tại");
            }

            // Kiểm tra title không rỗng
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                errors.Add("Tiêu đề lịch trình không được để trống");
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Validate request cập nhật tour detail
        /// </summary>
        private async Task<(bool IsValid, List<string> Errors)> ValidateUpdateRequestAsync(RequestUpdateTourDetailDto request, TourDetails existingDetail)
        {
            var errors = new List<string>();

            // Kiểm tra có ít nhất một field để update
            if (string.IsNullOrEmpty(request.Title) && request.Description == null)
            {
                errors.Add("Cần có ít nhất một thông tin để cập nhật");
            }

            // Kiểm tra title không rỗng nếu có update
            if (!string.IsNullOrEmpty(request.Title) && string.IsNullOrWhiteSpace(request.Title))
            {
                errors.Add("Tiêu đề lịch trình không được để trống");
            }

            return (errors.Count == 0, errors);
        }

        #endregion

        #region Missing Interface Methods

        /// <summary>
        /// Lấy thông tin chi tiết TourDetails theo ID
        /// </summary>
        public async Task<ResponseGetTourDetailDto> GetTourDetailByIdAsync(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Getting TourDetail by ID: {TourDetailsId}", tourDetailsId);

                var tourDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailsId);
                if (tourDetail == null || tourDetail.IsDeleted)
                {
                    return new ResponseGetTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy lịch trình"
                    };
                }

                var tourDetailDto = _mapper.Map<TourDetailDto>(tourDetail);

                return new ResponseGetTourDetailDto
                {
                    StatusCode = 200,
                    Message = "Lấy thông tin lịch trình thành công",
                    Data = tourDetailDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TourDetail by ID: {TourDetailsId}", tourDetailsId);
                return new ResponseGetTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thông tin lịch trình"
                };
            }
        }

        /// <summary>
        /// Lấy timeline theo TourDetailsId (new approach)
        /// </summary>
        public async Task<ResponseGetTimelineDto> GetTimelineByTourDetailsAsync(RequestGetTimelineByTourDetailsDto request)
        {
            try
            {
                _logger.LogInformation("Getting timeline for TourDetails: {TourDetailsId}", request.TourDetailsId);

                var tourDetail = await _unitOfWork.TourDetailsRepository.GetByIdAsync(request.TourDetailsId);

                if (tourDetail == null || tourDetail.IsDeleted)
                {
                    return new ResponseGetTimelineDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy lịch trình"
                    };
                }

                var timeline = new TimelineDto
                {
                    TourTemplateId = tourDetail.TourTemplate.Id,
                    TourTemplateTitle = tourDetail.TourTemplate.Title,
                    Items = tourDetail.Timeline
                        .Where(item => request.IncludeInactive || item.IsActive)
                        .OrderBy(item => item.SortOrder)
                        .Select(item => _mapper.Map<TimelineItemDto>(item))
                        .ToList(),
                    TotalItems = tourDetail.Timeline.Count(item => request.IncludeInactive || item.IsActive),
                    StartLocation = tourDetail.TourTemplate.StartLocation,
                    EndLocation = tourDetail.TourTemplate.EndLocation,
                    CreatedAt = tourDetail.CreatedAt,
                    UpdatedAt = tourDetail.UpdatedAt
                };

                return new ResponseGetTimelineDto
                {
                    StatusCode = 200,
                    Message = "Lấy timeline thành công",
                    Data = timeline
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline for TourDetails: {TourDetailsId}", request.TourDetailsId);
                return new ResponseGetTimelineDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy timeline"
                };
            }
        }

        // TODO: Implement remaining timeline methods - these are existing methods that need to be updated
        // For now, adding placeholders to satisfy interface requirements

        public async Task<ResponseGetTimelineDto> GetTimelineAsync(RequestGetTimelineDto request)
        {
            // TODO: Update to use TourDetails approach or mark as obsolete
            throw new NotImplementedException("This method will be updated to work with TourDetails approach");
        }

        public async Task<ResponseCreateTourDetailDto> AddTimelineItemAsync(RequestCreateTourDetailDto request, Guid createdById)
        {
            // TODO: Update for new approach
            throw new NotImplementedException("This method will be updated for new TourDetails approach");
        }

        public async Task<ResponseUpdateTourDetailDto> UpdateTimelineItemAsync(Guid timelineItemId, RequestUpdateTourDetailDto request, Guid updatedById)
        {
            // TODO: Update for new approach
            throw new NotImplementedException("This method will be updated for new TourDetails approach");
        }

        public async Task<ResponseDeleteTourDetailDto> DeleteTimelineItemAsync(Guid timelineItemId, Guid deletedById)
        {
            // TODO: Update for new approach
            throw new NotImplementedException("This method will be updated for new TourDetails approach");
        }

        public async Task<ResponseReorderTimelineDto> ReorderTimelineAsync(RequestReorderTimelineDto request, Guid updatedById)
        {
            // TODO: Update for new approach
            throw new NotImplementedException("This method will be updated for new TourDetails approach");
        }

        public async Task<ResponseValidateTimelineDto> ValidateTimelineAsync(Guid tourDetailsId)
        {
            // TODO: Implement validation for TourDetails timeline
            throw new NotImplementedException("This method will be implemented for TourDetails validation");
        }

        public async Task<ResponseTimelineStatisticsDto> GetTimelineStatisticsAsync(Guid tourDetailsId)
        {
            // TODO: Implement statistics for TourDetails timeline
            throw new NotImplementedException("This method will be implemented for TourDetails statistics");
        }

        public async Task<bool> CanDeleteTimelineItemAsync(Guid timelineItemId)
        {
            // TODO: Implement delete validation
            throw new NotImplementedException("This method will be implemented for delete validation");
        }

        public async Task<ResponseCreateTourDetailDto> DuplicateTimelineItemAsync(Guid timelineItemId, Guid createdById)
        {
            // TODO: Implement timeline item duplication
            throw new NotImplementedException("This method will be implemented for timeline item duplication");
        }

        public async Task<ResponseUpdateTourDetailDto> GetTimelineItemByIdAsync(Guid timelineItemId)
        {
            // TODO: Implement getting timeline item by ID
            throw new NotImplementedException("This method will be implemented for getting timeline item");
        }

        public async Task<BaseResposeDto> CreateTimelineItemAsync(RequestCreateTimelineItemDto request, Guid createdById)
        {
            try
            {
                // Validate TourDetails exists
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(request.TourDetailsId);
                if (tourDetails == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        IsSuccess = false
                    };
                }

                // Create new timeline item
                var timelineItem = new TimelineItem
                {
                    Id = Guid.NewGuid(),
                    TourDetailsId = request.TourDetailsId,
                    CheckInTime = TimeSpan.Parse(request.CheckInTime),
                    Activity = request.Activity,
                    ShopId = request.ShopId,
                    SortOrder = await GetNextSortOrderAsync(request.TourDetailsId),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = createdById
                };

                await _unitOfWork.TimelineItemRepository.AddAsync(timelineItem);
                await _unitOfWork.SaveChangesAsync();

                return new BaseResposeDto
                {
                    StatusCode = 201,
                    Message = "Tạo timeline item thành công",
                    IsSuccess = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tạo timeline item",
                    IsSuccess = false
                };
            }
        }

        private async Task<int> GetNextSortOrderAsync(Guid tourDetailsId)
        {
            var existingItems = await _unitOfWork.TimelineItemRepository.GetAllAsync(
                t => t.TourDetailsId == tourDetailsId);
            return existingItems.Any() ? existingItems.Max(t => t.SortOrder) + 1 : 1;
        }

        #endregion
    }
}
