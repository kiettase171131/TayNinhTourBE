using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Utilities;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
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
        private readonly IServiceProvider _serviceProvider;

        public TourDetailsService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TourDetailsService> logger,
            IServiceProvider serviceProvider)
            : base(mapper, unitOfWork)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
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
                    success = true,
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
                        success = false,
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
                        Message = "Tiêu đề lịch trình đã tồn tại",
                        success = false
                    };
                }

                // Create new tour detail using User.Id directly
                _logger.LogInformation("Creating TourDetails for User ID: {UserId}", createdById);

                var tourDetail = new TourDetails
                {
                    Id = Guid.NewGuid(),
                    TourTemplateId = request.TourTemplateId,
                    Title = request.Title,
                    Description = request.Description,
                    SkillsRequired = request.SkillsRequired,
                    ImageUrls = GetImageUrlsFromRequest(request.ImageUrls, request.ImageUrl),
                    CreatedById = createdById, // Use User.Id directly
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

                // SAVE SPECIALTY SHOP SELECTIONS: Lưu danh sách shops được chọn để mời sau khi admin duyệt
                if (request.SpecialtyShopIds != null && request.SpecialtyShopIds.Any())
                {
                    _logger.LogInformation("Saving {ShopCount} SpecialtyShop selections for TourDetails {TourDetailId}",
                        request.SpecialtyShopIds.Count, tourDetail.Id);

                    var shopInvitations = new List<TourDetailsSpecialtyShop>();
                    foreach (var shopId in request.SpecialtyShopIds.Distinct())
                    {
                        // Validate shop exists and is active
                        var shop = await _unitOfWork.SpecialtyShopRepository.GetByIdAsync(shopId);
                        if (shop != null && shop.IsShopActive && shop.IsActive)
                        {
                            var invitation = new TourDetailsSpecialtyShop
                            {
                                Id = Guid.NewGuid(),
                                TourDetailsId = tourDetail.Id,
                                SpecialtyShopId = shopId,
                                InvitedAt = DateTime.UtcNow,
                                Status = ShopInvitationStatus.Pending,
                                ExpiresAt = DateTime.UtcNow.AddDays(7), // 7 days to respond
                                CreatedById = createdById,
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            };

                            shopInvitations.Add(invitation);
                        }
                        else
                        {
                            _logger.LogWarning("SpecialtyShop {ShopId} not found or inactive, skipping invitation", shopId);
                        }
                    }

                    if (shopInvitations.Any())
                    {
                        foreach (var invitation in shopInvitations)
                        {
                            await _unitOfWork.TourDetailsSpecialtyShopRepository.AddAsync(invitation);
                        }
                        await _unitOfWork.SaveChangesAsync();

                        _logger.LogInformation("Successfully saved {InvitationCount} SpecialtyShop invitations for TourDetails {TourDetailId}",
                            shopInvitations.Count, tourDetail.Id);
                    }
                }
                else
                {
                    _logger.LogInformation("No SpecialtyShops selected for TourDetails {TourDetailId}", tourDetail.Id);
                }

                // Get created item with relationships
                var createdDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetail.Id);
                var tourDetailDto = _mapper.Map<TourDetailDto>(createdDetail);

                _logger.LogInformation("Successfully created tour detail {TourDetailId}", tourDetail.Id);

                return new ResponseCreateTourDetailDto
                {
                    StatusCode = 201,
                    Message = "Tạo lịch trình thành công. Lời mời sẽ được gửi sau khi admin duyệt.",
                    success = true,
                    Data = tourDetailDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tour detail for TourTemplate {TourTemplateId}", request.TourTemplateId);
                return new ResponseCreateTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tạo lịch trình",
                    success = false
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

                if (request.ImageUrls != null || request.ImageUrl != null)
                    existingDetail.ImageUrls = GetImageUrlsFromRequest(request.ImageUrls, request.ImageUrl);

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
                    success = true,
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

                // Soft delete using User.Id directly
                existingDetail.IsDeleted = true;
                existingDetail.DeletedAt = DateTime.UtcNow;
                existingDetail.UpdatedById = deletedById; // Use User.Id directly
                existingDetail.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourDetailsRepository.UpdateAsync(existingDetail);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted tour detail {TourDetailId}", tourDetailId);

                return new ResponseDeleteTourDetailDto
                {
                    StatusCode = 200,
                    Message = "Xóa lịch trình thành công",
                    success = true,
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
                    success = true,
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

        // TODO: Update to use SpecialtyShopRepository after merge
        /*
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
                    success = true,
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
        */

        public async Task<ResponseGetAvailableShopsDto> GetAvailableShopsAsync(bool includeInactive = false, string? searchKeyword = null)
        {
            // TODO: Implement with SpecialtyShopRepository after merge
            throw new NotImplementedException("This method will be updated to use SpecialtyShopRepository");
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
                    success = true,
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
                    Message = "Có lỗi xảy ra khi lấy danh sách lịch trình",
                    success = false
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
                        Message = "Không tìm thấy lịch trình",
                        success = false
                    };
                }

                var tourDetailDto = _mapper.Map<TourDetailDto>(tourDetail);

                return new ResponseGetTourDetailDto
                {
                    StatusCode = 200,
                    Message = "Lấy thông tin lịch trình thành công",
                    Data = tourDetailDto,
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TourDetail by ID: {TourDetailsId}", tourDetailsId);
                return new ResponseGetTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thông tin lịch trình",
                    success = false
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

                // Use GetWithDetailsAsync to load navigation properties
                var tourDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(request.TourDetailsId);

                if (tourDetail == null || tourDetail.IsDeleted)
                {
                    return new ResponseGetTimelineDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy lịch trình"
                    };
                }

                // Ensure TourTemplate is loaded
                if (tourDetail.TourTemplate == null)
                {
                    _logger.LogError("TourTemplate not loaded for TourDetails {TourDetailsId}", request.TourDetailsId);
                    return new ResponseGetTimelineDto
                    {
                        StatusCode = 500,
                        Message = "Lỗi tải thông tin tour template"
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
                    success = true,
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

        /// <summary>
        /// Admin duyệt hoặc từ chối tour details
        /// </summary>
        public async Task<BaseResposeDto> ApproveRejectTourDetailAsync(Guid tourDetailId, RequestApprovalTourDetailDto request, Guid adminId)
        {
            try
            {
                _logger.LogInformation("Admin {AdminId} processing approval for TourDetail {TourDetailId}", adminId, tourDetailId);

                // Validate tour detail exists
                var tourDetail = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailId);
                if (tourDetail == null || tourDetail.IsDeleted)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy tour detail",
                        success = false
                    };
                }

                // Validate business rules
                if (!request.IsApproved && string.IsNullOrWhiteSpace(request.Comment))
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Bình luận là bắt buộc khi từ chối tour detail",
                        success = false
                    };
                }

                // Update only specific fields to avoid foreign key constraint issues
                // Use direct SQL update to avoid Entity Framework tracking issues with UpdatedById
                var sql = @"UPDATE TourDetails
                           SET Status = @status,
                               CommentApproved = @comment,
                               UpdatedAt = @updatedAt
                           WHERE Id = @id";

                var parameters = new[]
                {
                    new MySqlParameter("@status", (int)(request.IsApproved ? TourDetailsStatus.Approved : TourDetailsStatus.Rejected)),
                    new MySqlParameter("@comment", request.Comment ?? (object)DBNull.Value),
                    new MySqlParameter("@updatedAt", DateTime.UtcNow),
                    new MySqlParameter("@id", tourDetailId)
                };

                await _unitOfWork.ExecuteSqlRawAsync(sql, parameters);

                // Nếu reject, hủy tất cả invitation pending để có thể tạo mới khi approve lại
                if (!request.IsApproved)
                {
                    await CancelPendingInvitationsAsync(tourDetailId);
                }

                await _unitOfWork.SaveChangesAsync();

                var statusText = request.IsApproved ? "duyệt" : "từ chối";
                _logger.LogInformation("Successfully {Action} TourDetail {TourDetailId} by Admin {AdminId}", statusText, tourDetailId, adminId);

                // 🔔 GỬI THÔNG BÁO ĐẾN TOUR COMPANY
                using var scope = _serviceProvider.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<ITourCompanyNotificationService>();

                if (request.IsApproved)
                {
                    // Gửi thông báo duyệt
                    await notificationService.NotifyTourApprovalAsync(
                        tourDetail.CreatedById,
                        tourDetail.Title,
                        request.Comment);

                    // TRIGGER EMAIL INVITATIONS: Gửi email mời khi admin approve TourDetails
                    await TriggerApprovalEmailsAsync(tourDetail, adminId);
                }
                else
                {
                    // Gửi thông báo từ chối
                    await notificationService.NotifyTourRejectionAsync(
                        tourDetail.CreatedById,
                        tourDetail.Title,
                        request.Comment!);
                }

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = $"Đã {statusText} tour detail thành công. Thông báo đã được gửi đến Tour Company.",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing approval for TourDetail {TourDetailId} by Admin {AdminId}", tourDetailId, adminId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi xử lý duyệt tour detail",
                    success = false
                };
            }
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
                        success = false
                    };
                }

                // Create new timeline item
                var timelineItem = new TimelineItem
                {
                    Id = Guid.NewGuid(),
                    TourDetailsId = request.TourDetailsId,
                    CheckInTime = TimeSpan.Parse(request.CheckInTime),
                    Activity = request.Activity,
                    SpecialtyShopId = request.SpecialtyShopId,
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
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tạo timeline item",
                    success = false
                };
            }
        }

        public async Task<ResponseCreateTimelineItemsDto> CreateTimelineItemsAsync(RequestCreateTimelineItemsDto request, Guid createdById)
        {
            try
            {
                // Validate TourDetails exists
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(request.TourDetailsId);
                if (tourDetails == null)
                {
                    return new ResponseCreateTimelineItemsDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        success = false
                    };
                }

                // Validate sortOrder conflicts within request
                var requestSortOrders = request.TimelineItems
                    .Where(item => item.SortOrder.HasValue)
                    .Select(item => item.SortOrder.Value)
                    .ToList();

                var duplicateSortOrders = requestSortOrders
                    .GroupBy(x => x)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateSortOrders.Any())
                {
                    return new ResponseCreateTimelineItemsDto
                    {
                        StatusCode = 400,
                        Message = $"SortOrder bị trùng lặp trong request: {string.Join(", ", duplicateSortOrders)}",
                        success = false,
                        Errors = new List<string> { $"Các sortOrder bị trùng: {string.Join(", ", duplicateSortOrders)}" }
                    };
                }

                // Get existing timeline items and check for conflicts
                var existingItems = await _unitOfWork.TimelineItemRepository.GetAllAsync(
                    t => t.TourDetailsId == request.TourDetailsId);
                var existingSortOrders = existingItems.Select(t => t.SortOrder).ToHashSet();

                var conflictingSortOrders = requestSortOrders
                    .Where(sortOrder => existingSortOrders.Contains(sortOrder))
                    .ToList();

                if (conflictingSortOrders.Any())
                {
                    return new ResponseCreateTimelineItemsDto
                    {
                        StatusCode = 409,
                        Message = $"SortOrder đã tồn tại: {string.Join(", ", conflictingSortOrders)}",
                        success = false,
                        Errors = new List<string> { $"Các sortOrder đã tồn tại trong timeline: {string.Join(", ", conflictingSortOrders)}" }
                    };
                }

                var createdItems = new List<TimelineItemDto>();
                var errors = new List<string>();
                int successCount = 0;
                int failedCount = 0;

                int currentMaxSortOrder = existingItems.Any() ? existingItems.Max(t => t.SortOrder) : 0;

                foreach (var itemRequest in request.TimelineItems)
                {
                    try
                    {
                        // Validate time format
                        if (!TimeSpan.TryParse(itemRequest.CheckInTime, out var checkInTime))
                        {
                            errors.Add($"Định dạng thời gian không hợp lệ: {itemRequest.CheckInTime}");
                            failedCount++;
                            continue;
                        }

                        // Create timeline item
                        var timelineItem = new TimelineItem
                        {
                            Id = Guid.NewGuid(),
                            TourDetailsId = request.TourDetailsId,
                            CheckInTime = checkInTime,
                            Activity = itemRequest.Activity,
                            SpecialtyShopId = itemRequest.SpecialtyShopId,
                            SortOrder = itemRequest.SortOrder.HasValue ? itemRequest.SortOrder.Value : (++currentMaxSortOrder),
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow,
                            CreatedById = createdById
                        };

                        await _unitOfWork.TimelineItemRepository.AddAsync(timelineItem);

                        // Map to DTO
                        var timelineItemDto = _mapper.Map<TimelineItemDto>(timelineItem);
                        createdItems.Add(timelineItemDto);
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Lỗi tạo timeline item '{itemRequest.Activity}': {ex.Message}");
                        failedCount++;
                    }
                }

                // Save all changes
                if (successCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                }

                return new ResponseCreateTimelineItemsDto
                {
                    StatusCode = successCount > 0 ? 201 : 400,
                    Message = $"Tạo thành công {successCount}/{request.TimelineItems.Count} timeline items",
                    success = successCount > 0,
                    Data = createdItems,
                    CreatedCount = successCount,
                    FailedCount = failedCount,
                    Errors = errors
                };
            }
            catch (Exception ex)
            {
                return new ResponseCreateTimelineItemsDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tạo timeline items",
                    success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        private async Task<int> GetNextSortOrderAsync(Guid tourDetailsId)
        {
            var existingItems = await _unitOfWork.TimelineItemRepository.GetAllAsync(
                t => t.TourDetailsId == tourDetailsId);
            return existingItems.Any() ? existingItems.Max(t => t.SortOrder) + 1 : 1;
        }

        /// <summary>
        /// Lấy danh sách TourDetails với filter theo status và quyền user
        /// </summary>
        public async Task<ResponseGetTourDetailsDto> GetTourDetailsWithPermissionAsync(Guid tourTemplateId, Guid currentUserId, string userRole, bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting tour details for TourTemplate {TourTemplateId} with permission for user {UserId} role {UserRole}",
                    tourTemplateId, currentUserId, userRole);

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

                // Filter theo quyền user
                var filteredTourDetails = FilterTourDetailsByPermission(tourDetails, currentUserId, userRole);

                // Map to DTOs
                var tourDetailDtos = _mapper.Map<List<TourDetailDto>>(filteredTourDetails);

                return new ResponseGetTourDetailsDto
                {
                    StatusCode = 200,
                    Message = "Lấy danh sách lịch trình thành công",
                    success = true,
                    Data = tourDetailDtos,
                    TotalCount = tourDetailDtos.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour details with permission for TourTemplate {TourTemplateId}", tourTemplateId);
                return new ResponseGetTourDetailsDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách lịch trình"
                };
            }
        }

        /// <summary>
        /// Filter TourDetails theo quyền user
        /// </summary>
        private IEnumerable<TourDetails> FilterTourDetailsByPermission(IEnumerable<TourDetails> tourDetails, Guid currentUserId, string userRole)
        {
            switch (userRole.ToLower())
            {
                case "admin":
                    // Admin thấy tất cả TourDetails
                    return tourDetails;

                case "tour guide":
                case "specialty shop":
                case "tour company":
                    // Tour Guide/Shop/Company thấy:
                    // - TourDetails của mình (tất cả status)
                    // - TourDetails đã approved của người khác
                    return tourDetails.Where(td =>
                        td.CreatedById == currentUserId ||
                        td.Status == TourDetailsStatus.Approved);

                case "user":
                default:
                    // User thường chỉ thấy TourDetails đã approved
                    return tourDetails.Where(td => td.Status == TourDetailsStatus.Approved);
            }
        }

        #endregion

        // ===== TOUR GUIDE ASSIGNMENT WORKFLOW =====

        public async Task<BaseResposeDto> GetGuideAssignmentStatusAsync(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Getting guide assignment status for TourDetails {TourDetailsId}", tourDetailsId);

                var tourDetails = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailsId);
                if (tourDetails == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        success = false
                    };
                }

                // Get invitations for this TourDetails
                var invitations = await _unitOfWork.TourGuideInvitationRepository.GetByTourDetailsAsync(tourDetailsId);

                // Get assigned guide info if exists
                var assignedGuide = tourDetails.TourOperation?.TourGuideId != null
                    ? await _unitOfWork.TourGuideRepository.GetByIdAsync(tourDetails.TourOperation.TourGuideId.Value)
                    : null;

                var statusInfo = new
                {
                    TourDetailsId = tourDetailsId,
                    Title = tourDetails.Title,
                    Status = tourDetails.Status.ToString(),
                    SkillsRequired = tourDetails.SkillsRequired,
                    AssignedGuide = assignedGuide != null ? new
                    {
                        Id = assignedGuide.Id,
                        Name = assignedGuide.FullName,
                        Email = assignedGuide.Email
                    } : null,
                    InvitationsSummary = new
                    {
                        Total = invitations.Count(),
                        Pending = invitations.Count(i => i.Status == InvitationStatus.Pending),
                        Accepted = invitations.Count(i => i.Status == InvitationStatus.Accepted),
                        Rejected = invitations.Count(i => i.Status == InvitationStatus.Rejected),
                        Expired = invitations.Count(i => i.Status == InvitationStatus.Expired)
                    },
                    CreatedAt = tourDetails.CreatedAt,
                    UpdatedAt = tourDetails.UpdatedAt
                };

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Lấy trạng thái phân công thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guide assignment status for TourDetails {TourDetailsId}", tourDetailsId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra: {ex.Message}",
                    success = false
                };
            }
        }

        public async Task<BaseResposeDto> ManualInviteGuideAsync(Guid tourDetailsId, Guid guideId, Guid userId)
        {
            try
            {
                _logger.LogInformation("User {UserId} manually inviting Guide {GuideId} for TourDetails {TourDetailsId}",
                    userId, guideId, tourDetailsId);

                // Validate TourDetails exists and belongs to user
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailsId);
                if (tourDetails == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        success = false
                    };
                }

                if (tourDetails.CreatedById != userId)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 403,
                        Message = "Bạn không có quyền mời hướng dẫn viên cho tour này",
                        success = false
                    };
                }

                // Check if TourDetails is in correct status for manual invitation
                if (tourDetails.Status != TourDetailsStatus.AwaitingGuideAssignment)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "TourDetails không ở trạng thái cho phép mời thủ công",
                        success = false
                    };
                }

                // Use invitation service to create manual invitation
                using var scope = _serviceProvider.CreateScope();
                var invitationService = scope.ServiceProvider.GetRequiredService<ITourGuideInvitationService>();
                var result = await invitationService.CreateManualInvitationAsync(tourDetailsId, guideId, userId);

                _logger.LogInformation("Manual invitation result for TourDetails {TourDetailsId}: {success}",
                    tourDetailsId, result.success);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual invitation for TourDetails {TourDetailsId} to Guide {GuideId}",
                    tourDetailsId, guideId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"Có lỗi xảy ra khi mời hướng dẫn viên: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// Trigger email invitations khi admin approve TourDetails
        /// Gửi email mời SpecialtyShop và TourGuide
        /// </summary>
        private async Task TriggerApprovalEmailsAsync(TourDetails tourDetail, Guid adminId)
        {
            try
            {
                _logger.LogInformation("Triggering approval emails for TourDetails {TourDetailId}", tourDetail.Id);

                using var scope = _serviceProvider.CreateScope();
                var emailSender = scope.ServiceProvider.GetRequiredService<EmailSender>();

                // 1. SEND SPECIALTY SHOP INVITATIONS
                await SendSpecialtyShopInvitationsAsync(tourDetail, emailSender);

                // 2. SEND TOUR GUIDE INVITATIONS
                await SendTourGuideInvitationsAsync(tourDetail, adminId);

                _logger.LogInformation("Successfully triggered all approval emails for TourDetails {TourDetailId}", tourDetail.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering approval emails for TourDetails {TourDetailId}", tourDetail.Id);
                // Don't fail the approval process if email sending fails
            }
        }

        /// <summary>
        /// Gửi email mời SpecialtyShop tham gia tour
        /// </summary>
        private async Task SendSpecialtyShopInvitationsAsync(TourDetails tourDetail, EmailSender emailSender)
        {
            try
            {
                // Lấy danh sách SpecialtyShop invitations
                var shopInvitations = await _unitOfWork.TourDetailsSpecialtyShopRepository
                    .GetByTourDetailsIdAsync(tourDetail.Id);

                if (!shopInvitations.Any())
                {
                    _logger.LogInformation("No SpecialtyShop invitations found for TourDetails {TourDetailId}", tourDetail.Id);
                    return;
                }

                _logger.LogInformation("Sending emails to {ShopCount} SpecialtyShops for TourDetails {TourDetailId}",
                    shopInvitations.Count(), tourDetail.Id);

                // Lấy thông tin TourTemplate để có tour date
                var tourTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(tourDetail.TourTemplateId);
                var tourCompany = await _unitOfWork.UserRepository.GetByIdAsync(tourDetail.CreatedById);

                foreach (var invitation in shopInvitations)
                {
                    try
                    {
                        await emailSender.SendSpecialtyShopTourInvitationAsync(
                            invitation.SpecialtyShop.User.Email,
                            invitation.SpecialtyShop.ShopName,
                            invitation.SpecialtyShop.User.Name,
                            tourDetail.Title,
                            tourCompany?.Name ?? "Tour Company",
                            DateTime.Now.AddDays(30), // Placeholder tour date
                            invitation.ExpiresAt,
                            invitation.Id.ToString()
                        );

                        _logger.LogInformation("Successfully sent email to SpecialtyShop {ShopId} for TourDetails {TourDetailId}",
                            invitation.SpecialtyShopId, tourDetail.Id);
                    }
                    catch (Exception emailEx)
                    {
                        _logger.LogError(emailEx, "Failed to send email to SpecialtyShop {ShopId} for TourDetails {TourDetailId}",
                            invitation.SpecialtyShopId, tourDetail.Id);
                        // Continue with other emails
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SpecialtyShop invitations for TourDetails {TourDetailId}", tourDetail.Id);
            }
        }

        /// <summary>
        /// Gửi email mời TourGuide dựa trên SkillsRequired
        /// </summary>
        private async Task SendTourGuideInvitationsAsync(TourDetails tourDetail, Guid adminId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tourDetail.SkillsRequired))
                {
                    _logger.LogInformation("No skills required for TourDetails {TourDetailId}, skipping TourGuide invitations", tourDetail.Id);
                    return;
                }

                _logger.LogInformation("=== STARTING TOURGUIDE INVITATION PROCESS ===");
                _logger.LogInformation("TourDetails ID: {TourDetailId}", tourDetail.Id);
                _logger.LogInformation("TourDetails Title: {Title}", tourDetail.Title);
                _logger.LogInformation("Skills Required: {SkillsRequired}", tourDetail.SkillsRequired);
                _logger.LogInformation("Admin ID: {AdminId}", adminId);

                using var scope = _serviceProvider.CreateScope();
                var invitationService = scope.ServiceProvider.GetRequiredService<ITourGuideInvitationService>();

                _logger.LogInformation("TourGuideInvitationService resolved successfully");

                var invitationResult = await invitationService.CreateAutomaticInvitationsAsync(tourDetail.Id, adminId);

                _logger.LogInformation("CreateAutomaticInvitationsAsync completed");
                _logger.LogInformation("Result Success: {success}", invitationResult.success);
                _logger.LogInformation("Result Message: {Message}", invitationResult.Message);
                _logger.LogInformation("Result StatusCode: {StatusCode}", invitationResult.StatusCode);

                if (invitationResult.success)
                {
                    _logger.LogInformation("✅ Successfully created TourGuide invitations for TourDetails {TourDetailId}", tourDetail.Id);
                }
                else
                {
                    _logger.LogWarning("❌ Failed to create TourGuide invitations for TourDetails {TourDetailId}: {Message}",
                        tourDetail.Id, invitationResult.Message);
                }

                _logger.LogInformation("=== TOURGUIDE INVITATION PROCESS COMPLETED ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending TourGuide invitations for TourDetails {TourDetailId}", tourDetail.Id);
            }
        }

        /// <summary>
        /// Hủy tất cả invitation pending cho tour details khi admin reject
        /// </summary>
        private async Task CancelPendingInvitationsAsync(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Cancelling pending invitations for TourDetails {TourDetailsId}", tourDetailsId);

                // Lấy tất cả invitation pending
                var pendingInvitations = await _unitOfWork.TourGuideInvitationRepository
                    .GetAllAsync(inv => inv.TourDetailsId == tourDetailsId &&
                                       inv.Status == InvitationStatus.Pending &&
                                       !inv.IsDeleted);

                if (pendingInvitations.Any())
                {
                    foreach (var invitation in pendingInvitations)
                    {
                        invitation.Status = InvitationStatus.Expired;
                        invitation.UpdatedAt = DateTime.UtcNow;
                        invitation.RejectionReason = "Tour details bị từ chối bởi admin";

                        await _unitOfWork.TourGuideInvitationRepository.UpdateAsync(invitation);
                    }

                    _logger.LogInformation("Cancelled {Count} pending invitations for TourDetails {TourDetailsId}",
                        pendingInvitations.Count(), tourDetailsId);
                }
                else
                {
                    _logger.LogInformation("No pending invitations found for TourDetails {TourDetailsId}", tourDetailsId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling pending invitations for TourDetails {TourDetailsId}", tourDetailsId);
                // Không throw exception để không ảnh hưởng đến flow chính
            }
        }

        /// <summary>
        /// Helper method để xử lý backward compatibility giữa ImageUrls và ImageUrl
        /// </summary>
        private List<string> GetImageUrlsFromRequest(List<string>? imageUrls, string? imageUrl)
        {
            // Ưu tiên ImageUrls nếu có
            if (imageUrls != null && imageUrls.Any())
            {
                return imageUrls;
            }

            // Fallback sang ImageUrl nếu có
            if (!string.IsNullOrEmpty(imageUrl))
            {
                return new List<string> { imageUrl };
            }

            // Trả về empty list nếu không có gì
            return new List<string>();
        }
    }
}
