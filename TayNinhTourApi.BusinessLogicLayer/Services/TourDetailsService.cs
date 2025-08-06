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
                    ImageUrls = GetImageUrlListFromRequest(request.ImageUrls, request.ImageUrl),
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

                // Check if tour has guide assigned - prevent editing if guide is already assigned
                bool hasGuideAssigned = existingDetail.TourOperation?.TourGuideId != null;
                if (hasGuideAssigned)
                {
                    return new ResponseUpdateTourDetailDto
                    {
                        StatusCode = 400,
                        Message = "Đã có hướng dẫn viên tham gia tour, không thể edit nữa",
                        success = false
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

                // Store original status for logic check
                var originalStatus = existingDetail.Status;

                // Update fields
                if (!string.IsNullOrEmpty(request.Title))
                    existingDetail.Title = request.Title;

                if (request.Description != null)
                    existingDetail.Description = request.Description;

                if (request.ImageUrls != null || request.ImageUrl != null)
                    existingDetail.ImageUrls = GetImageUrlListFromRequest(request.ImageUrls, request.ImageUrl);

                // Check status change logic:
                // If status is AwaitingGuideAssignment (waiting for guide approval) → send back to admin for approval
                if (originalStatus == TourDetailsStatus.AwaitingGuideAssignment)
                {
                    existingDetail.Status = TourDetailsStatus.AwaitingAdminApproval;
                    existingDetail.CommentApproved = null; // Clear previous admin comment
                    _logger.LogInformation("Tour detail {TourDetailId} status changed from AwaitingGuideAssignment to AwaitingAdminApproval due to edit", 
                        tourDetailId);
                }

                existingDetail.UpdatedById = updatedById;
                existingDetail.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourDetailsRepository.UpdateAsync(existingDetail);
                await _unitOfWork.SaveChangesAsync();

                // Send notification if status changed back to admin approval
                if (originalStatus == TourDetailsStatus.AwaitingGuideAssignment)
                {
                    try
                    {
                        // TODO: Send notification when notification service is available
                        _logger.LogInformation("Would send notification about status change back to admin approval for TourDetail {TourDetailId}", tourDetailId);
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogError(notificationEx, "Error sending notification for status change on TourDetail {TourDetailId}", tourDetailId);
                        // Don't fail the update if notification fails
                    }
                }
                
                // Get updated item with relationships
                var updatedDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(tourDetailId);
                var tourDetailDto = _mapper.Map<TourDetailDto>(updatedDetail);

                // Prepare response message based on status change
                string message = "Cập nhật lịch trình thành công";
                if (originalStatus == TourDetailsStatus.AwaitingGuideAssignment)
                {
                    message += ". Tour đã được gửi lại cho admin duyệt do có thay đổi trong lúc chờ hướng dẫn viên.";
                }

                _logger.LogInformation("Successfully updated tour detail {TourDetailId}", tourDetailId);

                return new ResponseUpdateTourDetailDto
                {
                    StatusCode = 200,
                    Message = message,
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

        public async Task<ResponseUpdateTourDetailDto> UpdateTimelineItemAsync(Guid timelineItemId, RequestUpdateTimelineItemDto request, Guid updatedById)
        {
            try
            {
                _logger.LogInformation("Updating timeline item {TimelineItemId} by user {UserId}", timelineItemId, updatedById);

                // 1. Get the timeline item first
                var timelineItem = await _unitOfWork.TimelineItemRepository.GetByIdAsync(timelineItemId);
                if (timelineItem == null)
                {
                    return new ResponseUpdateTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy timeline item này",
                        success = false
                    };
                }

                // 2. Get the related TourDetails
                var existingDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(timelineItem.TourDetailsId);
                if (existingDetail == null)
                {
                    return new ResponseUpdateTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy TourDetails liên quan",
                        success = false
                    };
                }

                // 3. BUSINESS RULE 1: Check if tour has guide assigned - prevent editing if guide is already assigned
                bool hasGuideAssigned = existingDetail.TourOperation?.TourGuideId != null;
                if (hasGuideAssigned)
                {
                    return new ResponseUpdateTourDetailDto
                    {
                        StatusCode = 400,
                        Message = "Đã có hướng dẫn viên tham gia tour, không thể edit timeline nữa",
                        success = false
                    };
                }

                // 4. Check ownership
                if (existingDetail.CreatedById != updatedById)
                {
                    return new ResponseUpdateTourDetailDto
                    {
                        StatusCode = 403,
                        Message = "Bạn không có quyền chỉnh sửa timeline item này",
                        success = false
                    };
                }

                // Store original status for logic check
                var originalStatus = existingDetail.Status;

                // 5. Update timeline item fields if provided
                if (!string.IsNullOrEmpty(request.Activity)
                    && request.Activity != timelineItem.Activity)
                {
                    timelineItem.Activity = request.Activity;
                }

                TimeSpan? newCheckInTime = null;
                if (!string.IsNullOrEmpty(request.CheckInTime))
                {
                    if (TimeSpan.TryParse(request.CheckInTime, out var parsedTime))
                    {
                        timelineItem.CheckInTime = parsedTime;
                        newCheckInTime = parsedTime;
                    }
                    else
                    {
                        return new ResponseUpdateTourDetailDto
                        {
                            StatusCode = 400,
                            Message = "Định dạng thời gian không hợp lệ",
                            success = false
                        };
                    }
                }

                if (request.SpecialtyShopId.HasValue)
                {
                    timelineItem.SpecialtyShopId = request.SpecialtyShopId;
                }

                if (request.SortOrder.HasValue)
                {
                    // Check for sort order conflicts
                    var existingItemWithSameOrder = await _unitOfWork.TimelineItemRepository
                        .GetAllAsync(t => t.TourDetailsId == timelineItem.TourDetailsId && 
                                         t.SortOrder == request.SortOrder.Value && 
                                         t.Id != timelineItemId);

                    if (existingItemWithSameOrder.Any())
                    {
                        return new ResponseUpdateTourDetailDto
                        {
                            StatusCode = 400,
                            Message = $"SortOrder {request.SortOrder.Value} đã được sử dụng",
                            success = false
                        };
                    }

                    timelineItem.SortOrder = request.SortOrder.Value;
                }

                timelineItem.UpdatedAt = DateTime.UtcNow;
                timelineItem.UpdatedById = updatedById;

                // 6. NEW: Validate time chronological order if time or sort order is being updated
                if (newCheckInTime.HasValue || request.SortOrder.HasValue)
                {
                    // Get all other timeline items for validation
                    var allTimelineItems = await _unitOfWork.TimelineItemRepository.GetAllAsync(
                        t => t.TourDetailsId == timelineItem.TourDetailsId && !t.IsDeleted && t.IsActive && t.Id != timelineItemId);

                    var finalCheckInTime = newCheckInTime ?? timelineItem.CheckInTime;
                    var finalSortOrder = request.SortOrder ?? timelineItem.SortOrder;

                    // Validate chronological order with other items
                    foreach (var otherItem in allTimelineItems)
                    {
                        // If updated item has a lower sort order, its time must be < other item's time
                        if (finalSortOrder < otherItem.SortOrder && finalCheckInTime >= otherItem.CheckInTime)
                        {
                            return new ResponseUpdateTourDetailDto
                            {
                                StatusCode = 400,
                                Message = $"Thời gian không hợp lệ: Hoạt động '{timelineItem.Activity}' (SortOrder {finalSortOrder}) có thời gian {finalCheckInTime:hh\\:mm} " +
                                         $"phải nhỏ hơn thời gian của '{otherItem.Activity}' (SortOrder {otherItem.SortOrder}) là {otherItem.CheckInTime:hh\\:mm}. " +
                                         $"Đây là tour du lịch trong ngày, không thể có 2 hoạt động cùng 1 khung giờ.",
                                success = false
                            };
                        }
                        
                        // If updated item has a higher sort order, its time must be > other item's time
                        if (finalSortOrder > otherItem.SortOrder && finalCheckInTime <= otherItem.CheckInTime)
                        {
                            return new ResponseUpdateTourDetailDto
                            {
                                StatusCode = 400,
                                Message = $"Thời gian không hợp lệ: Hoạt động '{timelineItem.Activity}' (SortOrder {finalSortOrder}) có thời gian {finalCheckInTime:hh\\:mm} " +
                                         $"phải lớn hơn thời gian của '{otherItem.Activity}' (SortOrder {otherItem.SortOrder}) là {otherItem.CheckInTime:hh\\:mm}. " +
                                         $"Đây là tour du lịch trong ngày, không thể có 2 hoạt động cùng 1 khung giờ.",
                                success = false
                            };
                        }
                    }
                }

                // 7. BUSINESS RULE 2: Check TourDetails status and update if needed
                bool tourDetailsStatusWillChange = false;

                // If TourDetails status is AwaitingGuideAssignment (waiting for guide assignment) → send back to admin for approval
                if (originalStatus == TourDetailsStatus.AwaitingGuideAssignment)
                {
                    existingDetail.Status = TourDetailsStatus.AwaitingAdminApproval; // Reset to "pending admin approval"
                    existingDetail.CommentApproved = null; // Clear previous admin comment
                    existingDetail.UpdatedAt = DateTime.UtcNow;
                    existingDetail.UpdatedById = updatedById;
                    
                    tourDetailsStatusWillChange = true;
                    _logger.LogInformation("TourDetails {TourDetailsId} status changed from AwaitingGuideAssignment to AwaitingAdminApproval due to timeline edit", 
                        existingDetail.Id);
                    
                    // Update TourDetails in database
                    await _unitOfWork.TourDetailsRepository.UpdateAsync(existingDetail);
                }

                // 8. Save timeline item changes
                await _unitOfWork.TimelineItemRepository.UpdateAsync(timelineItem);
                await _unitOfWork.SaveChangesAsync();

                // 9. Send notification if TourDetails status changed
                if (tourDetailsStatusWillChange)
                {
                    try
                    {
                        // TODO: Send notification when notification service is available
                        _logger.LogInformation("Would send notification about TourDetails status change back to admin approval for TourDetails {TourDetailsId}", existingDetail.Id);
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogError(notificationEx, "Error sending notification for status change on TourDetails {TourDetailsId}", existingDetail.Id);
                        // Don't fail the update if notification fails
                    }
                }

                // 10. Get updated TourDetails with relationships for response
                var updatedDetail = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(existingDetail.Id);
                var tourDetailDto = _mapper.Map<TourDetailDto>(updatedDetail);

                // 11. Prepare response message based on status change
                string message = "Cập nhật timeline item thành công";
                if (tourDetailsStatusWillChange)
                {
                    message += ". Tour đã được gửi lại cho admin duyệt do có thay đổi trong lúc chờ hướng dẫn viên được phân công.";
                }

                _logger.LogInformation("Successfully updated timeline item {TimelineItemId}", timelineItemId);

                return new ResponseUpdateTourDetailDto
                {
                    StatusCode = 200,
                    Message = message,
                    success = true,
                    Data = tourDetailDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating timeline item {TimelineItemId}", timelineItemId);
                return new ResponseUpdateTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi cập nhật timeline item",
                    success = false
                };
            }
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

                // TODO: Send notifications when notification services are available
                // For now, just log the action
                _logger.LogInformation("Would send {Status} notification to TourCompany {CompanyId} for TourDetail {TourDetailId}",
                    statusText, tourDetail.CreatedById, tourDetailId);

                // TRIGGER AUTOMATIC GUIDE INVITATIONS: Chỉ gửi thông báo in-app, không gửi email để cải thiện hiệu suất
                if (request.IsApproved)
                {
                    // Loại bỏ TriggerApprovalEmailsAsync để cải thiện hiệu suất
                    // await TriggerApprovalEmailsAsync(tourDetail, adminId);

                    // 🔔 NEW: TRIGGER AUTOMATIC GUIDE INVITATIONS when approved
                    await TriggerAutomaticGuideInvitationsAsync(tourDetail, adminId);
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
                _logger.LogInformation("Creating single timeline item for TourDetails {TourDetailsId} by user {UserId}", 
                    request.TourDetailsId, createdById);

                // 1. Validate TourDetails exists
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(request.TourDetailsId);
                if (tourDetails == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        success = false
                    };
                }

                // 2. BUSINESS RULE: Check if tour has guide assigned - prevent adding if guide is already assigned
                bool hasGuideAssigned = tourDetails.TourOperation?.TourGuideId != null;
                if (hasGuideAssigned)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Đã có hướng dẫn viên tham gia tour, không thể thêm timeline item nữa",
                        success = false
                    };
                }

                // 3. Check ownership
                if (tourDetails.CreatedById != createdById)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 403,
                        Message = "Bạn không có quyền chỉnh sửa timeline cho TourDetails này",
                        success = false
                    };
                }

                // 4. Validate time format
                if (!TimeSpan.TryParse(request.CheckInTime, out var checkInTime))
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 400,
                        Message = "Định dạng thời gian không hợp lệ",
                        success = false
                    };
                }

                // 5. Get existing timeline items for validation
                var existingItems = await _unitOfWork.TimelineItemRepository.GetAllAsync(
                    t => t.TourDetailsId == request.TourDetailsId && !t.IsDeleted && t.IsActive);

                // 6. NEW: Validate time chronological order based on sort order
                int newSortOrder = request.SortOrder ?? await GetNextSortOrderAsync(request.TourDetailsId);
                
                // Check if there are existing items that would conflict with time order
                foreach (var existingItem in existingItems)
                {
                    // If new item has a lower sort order, its time must be < existing item's time
                    if (newSortOrder < existingItem.SortOrder && checkInTime >= existingItem.CheckInTime)
                    {
                        return new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = $"Thời gian không hợp lệ: Hoạt động '{request.Activity}' (SortOrder {newSortOrder}) có thời gian {checkInTime:hh\\:mm} " +
                                     $"phải nhỏ hơn thời gian của '{existingItem.Activity}' (SortOrder {existingItem.SortOrder}) là {existingItem.CheckInTime:hh\\:mm}. " +
                                     $"Đây là tour du lịch trong ngày, không thể có 2 hoạt động cùng 1 khung giờ.",
                            success = false
                        };
                    }
                    
                    // If new item has a higher sort order, its time must be > existing item's time
                    if (newSortOrder > existingItem.SortOrder && checkInTime <= existingItem.CheckInTime)
                    {
                        return new BaseResposeDto
                        {
                            StatusCode = 400,
                            Message = $"Thời gian không hợp lệ: Hoạt động '{request.Activity}' (SortOrder {newSortOrder}) có thời gian {checkInTime:hh\\:mm} " +
                                     $"phải lớn hơn thời gian của '{existingItem.Activity}' (SortOrder {existingItem.SortOrder}) là {existingItem.CheckInTime:hh\\:mm}. " +
                                     $"Đây là tour du lịch trong ngày, không thể có 2 hoạt động cùng 1 khung giờ.",
                            success = false
                        };
                    }
                }

                // Store original status for logic check
                var originalStatus = tourDetails.Status;

                // 7. Create new timeline item
                var timelineItem = new TimelineItem
                {
                    Id = Guid.NewGuid(),
                    TourDetailsId = request.TourDetailsId,
                    CheckInTime = checkInTime,
                    Activity = request.Activity,
                    SpecialtyShopId = request.SpecialtyShopId,
                    SortOrder = newSortOrder,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = createdById
                };

                await _unitOfWork.TimelineItemRepository.AddAsync(timelineItem);

                // 8. BUSINESS RULE: Auto-set status về Pending để admin duyệt lại
                bool tourDetailsStatusChanged = false;

                // Always set status to Pending when adding new timeline item (regardless of current status)
                tourDetails.Status = TourDetailsStatus.Pending;
                tourDetails.CommentApproved = null; // Clear previous admin comment
                tourDetails.UpdatedAt = DateTime.UtcNow;
                tourDetails.UpdatedById = createdById;
                
                tourDetailsStatusChanged = true;
                _logger.LogInformation("TourDetails {TourDetailsId} status changed from {OriginalStatus} to Pending due to new timeline item added", 
                    tourDetails.Id, originalStatus);
                
                // Update TourDetails in database
                await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);

                // 9. Save all changes
                await _unitOfWork.SaveChangesAsync();

                // 10. Send notification if TourDetails status changed
                if (tourDetailsStatusChanged)
                {
                    try
                    {
                        // TODO: Send notification when notification service is available
                        _logger.LogInformation("Would send notification about TourDetails status change to Pending for TourDetails {TourDetailsId}", tourDetails.Id);
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogError(notificationEx, "Error sending notification for TourDetails status change on TourDetails {TourDetailsId}", tourDetails.Id);
                        // Don't fail the creation if notification fails
                    }
                }

                // 11. Prepare response message based on status change
                string message = "Tạo timeline item thành công";
                if (tourDetailsStatusChanged)
                {
                    message += ". Tour đã được chuyển về trạng thái 'Chờ duyệt' để admin xem xét lại.";
                }

                return new BaseResposeDto
                {
                    StatusCode = 201,
                    Message = message,
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating timeline item for TourDetails {TourDetailsId}", request.TourDetailsId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tạo timeline item",
                    success = false
                };
            }
        }

        public async Task<ResponseDeleteTourDetailDto> DeleteTimelineItemAsync(Guid timelineItemId, Guid deletedById)
        {
            try
            {
                _logger.LogInformation("Deleting timeline item {TimelineItemId} by user {UserId}", timelineItemId, deletedById);

                var timelineItem = await _unitOfWork.TimelineItemRepository.GetByIdAsync(timelineItemId);
                if (timelineItem == null)
                {
                    return new ResponseDeleteTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy timeline item này",
                        success = false
                    };
                }

                // Soft delete timeline item
                timelineItem.IsDeleted = true;
                timelineItem.DeletedAt = DateTime.UtcNow;
                timelineItem.UpdatedAt = DateTime.UtcNow;
                timelineItem.UpdatedById = deletedById;

                await _unitOfWork.TimelineItemRepository.UpdateAsync(timelineItem);
                await _unitOfWork.SaveChangesAsync();

                return new ResponseDeleteTourDetailDto
                {
                    StatusCode = 200,
                    Message = "Xóa timeline item thành công",
                    success = true,
                    DeletedTourDetailId = timelineItemId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting timeline item {TimelineItemId}", timelineItemId);
                return new ResponseDeleteTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi xóa timeline item",
                    success = false
                };
            }
        }

        public async Task<ResponseReorderTimelineDto> ReorderTimelineAsync(RequestReorderTimelineDto request, Guid updatedById)
        {
            try
            {
                _logger.LogInformation("Reordering timeline for TourDetails {TourDetailsId}", request.TourDetailsId);

                // Validate timeline items exist
                var timelineItems = await _unitOfWork.TimelineItemRepository
                    .GetAllAsync(t => t.TourDetailsId == request.TourDetailsId && !t.IsDeleted);

                if (!timelineItems.Any())
                {
                    return new ResponseReorderTimelineDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy timeline items",
                        success = false
                    };
                }

                // Update sort orders based on new order
                for (int i = 0; i < request.TimelineItemIds.Count; i++)
                {
                    var itemId = request.TimelineItemIds[i];
                    var item = timelineItems.FirstOrDefault(t => t.Id == itemId);
                    if (item != null)
                    {
                        item.SortOrder = i + 1;
                        item.UpdatedAt = DateTime.UtcNow;
                        item.UpdatedById = updatedById;
                        await _unitOfWork.TimelineItemRepository.UpdateAsync(item);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                return new ResponseReorderTimelineDto
                {
                    StatusCode = 200,
                    Message = "Sắp xếp lại timeline thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering timeline for TourDetails {TourDetailsId}", request.TourDetailsId);
                return new ResponseReorderTimelineDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi sắp xếp lại timeline",
                    success = false
                };
            }
        }

        public async Task<ResponseTimelineStatisticsDto> GetTimelineStatisticsAsync(Guid tourTemplateId)
        {
            try
            {
                _logger.LogInformation("Getting timeline statistics for TourTemplate {TourTemplateId}", tourTemplateId);

                var tourDetails = await _unitOfWork.TourDetailsRepository
                    .GetByTourTemplateOrderedAsync(tourTemplateId, false);

                var totalTourDetails = tourDetails.Count();
                var totalTimelineItems = tourDetails.Sum(td => td.Timeline?.Count(t => t.IsActive) ?? 0);
                var avgItemsPerDetail = totalTourDetails > 0 ? (double)totalTimelineItems / totalTourDetails : 0;

                return new ResponseTimelineStatisticsDto
                {
                    StatusCode = 200,
                    Message = "Lấy thống kê timeline thành công",
                    success = true,
                    TotalTourDetails = totalTourDetails,
                    TotalTimelineItems = totalTimelineItems,
                    AverageItemsPerDetail = Math.Round(avgItemsPerDetail, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline statistics for TourTemplate {TourTemplateId}", tourTemplateId);
                return new ResponseTimelineStatisticsDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thống kê timeline",
                    success = false
                };
            }
        }

        public async Task<bool> CanDeleteTimelineItemAsync(Guid timelineItemId)
        {
            try
            {
                var timelineItem = await _unitOfWork.TimelineItemRepository.GetByIdAsync(timelineItemId);
                if (timelineItem == null) return false;

                // Get the related TourDetails
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(timelineItem.TourDetailsId);
                if (tourDetails == null) return false;

                // Check if tour has guide assigned - prevent deletion if guide is already assigned
                var tourOperation = await _unitOfWork.TourOperationRepository
                    .GetAllAsync(to => to.TourDetailsId == tourDetails.Id);
                
                bool hasGuideAssigned = tourOperation.Any(to => to.TourGuideId != null);
                
                return !hasGuideAssigned;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if timeline item {TimelineItemId} can be deleted", timelineItemId);
                return false;
            }
        }

        public async Task<ResponseCreateTourDetailDto> DuplicateTimelineItemAsync(Guid timelineItemId, Guid createdById)
        {
            try
            {
                _logger.LogInformation("Duplicating timeline item {TimelineItemId} by user {UserId}", timelineItemId, createdById);

                var originalItem = await _unitOfWork.TimelineItemRepository.GetByIdAsync(timelineItemId);
                if (originalItem == null)
                {
                    return new ResponseCreateTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy timeline item để duplicate",
                        success = false
                    };
                }

                // Create duplicated item with new sort order
                var duplicatedItem = new TimelineItem
                {
                    Id = Guid.NewGuid(),
                    TourDetailsId = originalItem.TourDetailsId,
                    CheckInTime = originalItem.CheckInTime,
                    Activity = $"{originalItem.Activity} (Copy)",
                    SpecialtyShopId = originalItem.SpecialtyShopId,
                    SortOrder = await GetNextSortOrderAsync(originalItem.TourDetailsId),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = createdById
                };

                await _unitOfWork.TimelineItemRepository.AddAsync(duplicatedItem);
                await _unitOfWork.SaveChangesAsync();

                // Return TourDetails response format for consistency
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(originalItem.TourDetailsId);
                var tourDetailDto = _mapper.Map<TourDetailDto>(tourDetails);

                return new ResponseCreateTourDetailDto
                {
                    StatusCode = 201,
                    Message = "Duplicate timeline item thành công",
                    success = true,
                    Data = tourDetailDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating timeline item {TimelineItemId}", timelineItemId);
                return new ResponseCreateTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi duplicate timeline item",
                    success = false
                };
            }
        }

        public async Task<ResponseUpdateTourDetailDto> GetTimelineItemByIdAsync(Guid timelineItemId)
        {
            try
            {
                _logger.LogInformation("Getting timeline item by ID {TimelineItemId}", timelineItemId);

                var timelineItem = await _unitOfWork.TimelineItemRepository.GetByIdAsync(timelineItemId);
                if (timelineItem == null)
                {
                    return new ResponseUpdateTourDetailDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy timeline item",
                        success = false
                    };
                }

                // Get the related TourDetails
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(timelineItem.TourDetailsId);
                var tourDetailDto = _mapper.Map<TourDetailDto>(tourDetails);

                return new ResponseUpdateTourDetailDto
                {
                    StatusCode = 200,
                    Message = "Lấy thông tin timeline item thành công",
                    success = true,
                    Data = tourDetailDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting timeline item by ID {TimelineItemId}", timelineItemId);
                return new ResponseUpdateTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy thông tin timeline item",
                    success = false
                };
            }
        }

        public async Task<ResponseCreateTimelineItemsDto> CreateTimelineItemsAsync(RequestCreateTimelineItemsDto request, Guid createdById)
        {
            try
            {
                _logger.LogInformation("Creating {Count} timeline items for TourDetails {TourDetailsId} by user {UserId}",
                    request.TimelineItems.Count, request.TourDetailsId, createdById);

                // 1. Validate TourDetails exists
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetWithDetailsAsync(request.TourDetailsId);
                if (tourDetails == null)
                {
                    return new ResponseCreateTimelineItemsDto
                    {
                        StatusCode = 404,
                        Message = "TourDetails không tồn tại",
                        success = false
                    };
                }

                // 2. BUSINESS RULE: Check if tour has guide assigned - prevent adding if guide is already assigned
                bool hasGuideAssigned = tourDetails.TourOperation?.TourGuideId != null;
                if (hasGuideAssigned)
                {
                    return new ResponseCreateTimelineItemsDto
                    {
                        StatusCode = 400,
                        Message = "Đã có hướng dẫn viên tham gia tour, không thể thêm timeline items nữa",
                        success = false
                    };
                }

                // 3. Check ownership
                if (tourDetails.CreatedById != createdById)
                {
                    return new ResponseCreateTimelineItemsDto
                    {
                        StatusCode = 403,
                        Message = "Bạn không có quyền chỉnh sửa timeline cho TourDetails này",
                        success = false
                    };
                }

                // 4. Validate sortOrder conflicts within request
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

                // 5. Get existing timeline items and check for conflicts
                var existingItems = await _unitOfWork.TimelineItemRepository.GetAllAsync(
                    t => t.TourDetailsId == request.TourDetailsId && !t.IsDeleted);
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

                // 6. NEW: Validate time chronological order based on sort order
                var timeValidationErrors = new List<string>();
                
                // Create a combined list of existing and new timeline items for validation
                var allTimelineItems = new List<(int SortOrder, TimeSpan CheckInTime, string Activity)>();
                
                // Add existing items
                foreach (var existingItem in existingItems.Where(t => t.IsActive))
                {
                    allTimelineItems.Add((existingItem.SortOrder, existingItem.CheckInTime, existingItem.Activity));
                }

                // Add new items from request (validate time format first)
                var validNewItems = new List<(int SortOrder, TimeSpan CheckInTime, string Activity)>();
                foreach (var newItem in request.TimelineItems)
                {
                    if (!TimeSpan.TryParse(newItem.CheckInTime, out var checkInTime))
                    {
                        timeValidationErrors.Add($"Định dạng thời gian không hợp lệ cho hoạt động '{newItem.Activity}': {newItem.CheckInTime}");
                        continue;
                    }

                    int sortOrder = newItem.SortOrder.HasValue ? newItem.SortOrder.Value : (existingItems.Any() ? existingItems.Max(t => t.SortOrder) + validNewItems.Count + 1 : validNewItems.Count + 1);
                    validNewItems.Add((sortOrder, checkInTime, newItem.Activity));
                    allTimelineItems.Add((sortOrder, checkInTime, newItem.Activity));
                }

                if (timeValidationErrors.Any())
                {
                    return new ResponseCreateTimelineItemsDto
                    {
                        StatusCode = 400,
                        Message = "Có lỗi định dạng thời gian",
                        success = false,
                        Errors = timeValidationErrors
                    };
                }

                // Sort all items by sort order and validate chronological order
                var sortedItems = allTimelineItems.OrderBy(item => item.SortOrder).ToList();
                
                for (int i = 1; i < sortedItems.Count; i++)
                {
                    var previousItem = sortedItems[i - 1];
                    var currentItem = sortedItems[i];
                    
                    // Check if current time is strictly greater than previous time (no equal times allowed)
                    if (currentItem.CheckInTime <= previousItem.CheckInTime)
                    {
                        timeValidationErrors.Add(
                            $"Thời gian không hợp lệ: Hoạt động '{currentItem.Activity}' (SortOrder {currentItem.SortOrder}) có thời gian {currentItem.CheckInTime:hh\\:mm} " +
                            $"phải lớn hơn thời gian của '{previousItem.Activity}' (SortOrder {previousItem.SortOrder}) là {previousItem.CheckInTime:hh\\:mm}. " +
                            $"Đây là tour du lịch trong ngày, không thể có 2 hoạt động cùng 1 khung giờ.");
                    }
                }

                if (timeValidationErrors.Any())
                {
                    return new ResponseCreateTimelineItemsDto
                    {
                        StatusCode = 400,
                        Message = "Thứ tự thời gian timeline không hợp lệ",
                        success = false,
                        Errors = timeValidationErrors
                    };
                }

                var createdItems = new List<TimelineItemDto>();
                var errors = new List<string>();
                int successCount = 0;
                int failedCount = 0;

                int currentMaxSortOrder = existingItems.Any() ? existingItems.Max(t => t.SortOrder) : 0;

                // Store original status for logic check
                var originalStatus = tourDetails.Status;

                // 7. Create timeline items
                foreach (var itemRequest in request.TimelineItems)
                {
                    try
                    {
                        // Validate time format (already done above, but keeping for safety)
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
                            SortOrder = itemRequest.SortOrder.HasValue ? itemRequest.SortOrder.Value : (existingItems.Any() ? existingItems.Max(t => t.SortOrder) + validNewItems.Count + 1 : validNewItems.Count + 1),
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
                        _logger.LogError(ex, "Error creating timeline item '{Activity}' for TourDetails {TourDetailsId}", 
                            itemRequest.Activity, request.TourDetailsId);
                    }
                }

                // 8. BUSINESS RULE: Auto-set status về Pending để admin duyệt lại
                bool tourDetailsStatusChanged = false;

                // Always set status to Pending when adding new timeline items (regardless of current status)
                tourDetails.Status = TourDetailsStatus.Pending;
                tourDetails.CommentApproved = null; // Clear previous admin comment
                tourDetails.UpdatedAt = DateTime.UtcNow;
                tourDetails.UpdatedById = createdById;
                
                tourDetailsStatusChanged = true;
                _logger.LogInformation("TourDetails {TourDetailsId} status changed from {OriginalStatus} to Pending due to new timeline items added", 
                    tourDetails.Id, originalStatus);
                
                // Update TourDetails in database
                await _unitOfWork.TourDetailsRepository.UpdateAsync(tourDetails);

                // 9. Save all changes
                if (successCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                }

                // 10. Send notification if TourDetails status changed
                if (tourDetailsStatusChanged)
                {
                    try
                    {
                        // TODO: Send notification when notification service is available
                        _logger.LogInformation("Would send notification about TourDetails status change to Pending for TourDetails {TourDetailsId}", tourDetails.Id);
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogError(notificationEx, "Error sending notification for TourDetails status change on TourDetails {TourDetailsId}", tourDetails.Id);
                        // Don't fail the creation if notification fails
                    }
                }

                // 11. Prepare response message based on status change
                string message = $"Tạo thành công {successCount}/{request.TimelineItems.Count} timeline items";
                if (tourDetailsStatusChanged)
                {
                    message += ". Tour đã được chuyển về trạng thái 'Chờ duyệt' để admin xem xét lại.";
                }

                return new ResponseCreateTimelineItemsDto
                {
                    StatusCode = successCount > 0 ? 201 : 400,
                    Message = message,
                    success = successCount > 0,
                    Data = createdItems,
                    CreatedCount = successCount,
                    FailedCount = failedCount,
                    Errors = errors
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating timeline items for TourDetails {TourDetailsId}", request.TourDetailsId);
                return new ResponseCreateTimelineItemsDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi tạo timeline items",
                    success = false,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        #endregion

        #region Missing Interface Methods

        /// <summary>
        /// Lấy full timeline cho tour template với sort order (DEPRECATED - backward compatibility)
        /// </summary>
        [Obsolete("Use GetTimelineByTourDetailsAsync instead. This method will be removed in future versions.")]
        public async Task<ResponseGetTimelineDto> GetTimelineAsync(RequestGetTimelineDto request)
        {
            try
            {
                _logger.LogWarning("Using deprecated GetTimelineAsync method for TourTemplate {TourTemplateId}", request.TourTemplateId);

                // For backward compatibility, we'll try to get the first TourDetails of the template
                var tourDetails = await _unitOfWork.TourDetailsRepository
                    .GetByTourTemplateOrderedAsync(request.TourTemplateId, request.IncludeInactive);

                var firstTourDetail = tourDetails.FirstOrDefault();
                if (firstTourDetail == null)
                {
                    return new ResponseGetTimelineDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy TourDetails nào cho template này"
                    };
                }

                // Use the new method internally
                var newRequest = new RequestGetTimelineByTourDetailsDto
                {
                    TourDetailsId = firstTourDetail.Id,
                    IncludeInactive = request.IncludeInactive,
                    IncludeShopInfo = request.IncludeShopInfo
                };

                return await GetTimelineByTourDetailsAsync(newRequest);
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

        /// <summary>
        /// Thêm mốc thời gian mới vào timeline
        /// </summary>
        public async Task<ResponseCreateTourDetailDto> AddTimelineItemAsync(RequestCreateTourDetailDto request, Guid createdById)
        {
            try
            {
                _logger.LogWarning("Using deprecated AddTimelineItemAsync method. Consider using CreateTimelineItemAsync instead.");
                
                // This method is deprecated, but for backward compatibility we'll create a TourDetail
                return await CreateTourDetailAsync(request, createdById);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding timeline item");
                return new ResponseCreateTourDetailDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi thêm timeline item",
                    success = false
                };
            }
        }

        /// <summary>
        /// Validate timeline để kiểm tra tính hợp lệ
        /// </summary>
        public async Task<ResponseValidateTimelineDto> ValidateTimelineAsync(Guid tourTemplateId)
        {
            try
            {
                _logger.LogInformation("Validating timeline for TourTemplate {TourTemplateId}", tourTemplateId);

                var validationErrors = new List<string>();

                // Check if tour template exists
                var tourTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(tourTemplateId);
                if (tourTemplate == null || tourTemplate.IsDeleted)
                {
                    validationErrors.Add("Tour template không tồn tại");
                }

                // Get all tour details for this template
                var tourDetails = await _unitOfWork.TourDetailsRepository
                    .GetByTourTemplateOrderedAsync(tourTemplateId, false);

                if (!tourDetails.Any())
                {
                    validationErrors.Add("Chưa có TourDetails nào được tạo cho template này");
                }

                // Validate each TourDetails
                foreach (var tourDetail in tourDetails)
                {
                    if (tourDetail.Timeline == null || !tourDetail.Timeline.Any())
                    {
                        validationErrors.Add($"TourDetails '{tourDetail.Title}' chưa có timeline items");
                    }
                    else
                    {
                        // Check for duplicate sort orders
                        var sortOrders = tourDetail.Timeline.Select(t => t.SortOrder).ToList();
                        var duplicates = sortOrders.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
                        if (duplicates.Any())
                        {
                            validationErrors.Add($"TourDetails '{tourDetail.Title}' có timeline items với sortOrder trùng lặp: {string.Join(", ", duplicates)}");
                        }

                        // Check for invalid times
                        foreach (var timelineItem in tourDetail.Timeline)
                        {
                            if (string.IsNullOrEmpty(timelineItem.Activity))
                            {
                                validationErrors.Add($"TourDetails '{tourDetail.Title}' có timeline item thiếu Activity");
                            }
                        }
                    }
                }

                return new ResponseValidateTimelineDto
                {
                    StatusCode = validationErrors.Any() ? 400 : 200,
                    Message = validationErrors.Any() ? "Timeline có lỗi cần khắc phục" : "Timeline hợp lệ",
                    success = !validationErrors.Any(),
                    ValidationErrors = validationErrors,
                    TotalErrors = validationErrors.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating timeline for TourTemplate {TourTemplateId}", tourTemplateId);
                return new ResponseValidateTimelineDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi validate timeline",
                    success = false
                };
            }
        }

        /// <summary>
        /// Lấy danh sách TourDetails với filter theo status và quyền user
        /// </summary>
        public async Task<ResponseGetTourDetailsDto> GetTourDetailsWithPermissionAsync(Guid tourTemplateId, Guid currentUserId, string userRole, bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting TourDetails with permission for user {UserId} with role {UserRole}", currentUserId, userRole);

                // Get all tour details first
                var tourDetails = await _unitOfWork.TourDetailsRepository
                    .GetByTourTemplateOrderedAsync(tourTemplateId, includeInactive);

                // Apply permission filters based on user role
                if (userRole == "TourCompany")
                {
                    // Tour companies can only see their own TourDetails
                    tourDetails = tourDetails.Where(td => td.CreatedById == currentUserId);
                }
                // Admin can see all TourDetails (no additional filtering)

                var tourDetailDtos = _mapper.Map<List<TourDetailDto>>(tourDetails.ToList());

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
                _logger.LogError(ex, "Error getting TourDetails with permission for user {UserId}", currentUserId);
                return new ResponseGetTourDetailsDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy danh sách lịch trình"
                };
            }
        }

        /// <summary>
        /// Lấy trạng thái phân công hướng dẫn viên cho TourDetails
        /// </summary>
        public async Task<BaseResposeDto> GetGuideAssignmentStatusAsync(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Getting guide assignment status for TourDetails {TourDetailsId}", tourDetailsId);

                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailsId);
                if (tourDetails == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy TourDetails",
                        success = false
                    };
                }

                // Check if there's a TourOperation with assigned guide
                var tourOperation = await _unitOfWork.TourOperationRepository
                    .GetAllAsync(to => to.TourDetailsId == tourDetailsId);
                var hasAssignedGuide = tourOperation.Any(to => to.TourGuideId != null);

                var statusInfo = new
                {
                    TourDetailsId = tourDetailsId,
                    HasAssignedGuide = hasAssignedGuide,
                    Status = tourDetails.Status.ToString(),
                    AssignedGuideCount = tourOperation.Count(to => to.TourGuideId != null)
                };

                return new BaseResposeDto
                {
                    StatusCode = 200,
                    Message = "Lấy trạng thái phân công guide thành công",
                    success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting guide assignment status for TourDetails {TourDetailsId}", tourDetailsId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi lấy trạng thái phân công guide",
                    success = false
                };
            }
        }

        /// <summary>
        /// TourCompany mời thủ công một TourGuide cụ thể
        /// </summary>
        public async Task<BaseResposeDto> ManualInviteGuideAsync(Guid tourDetailsId, Guid guideId, Guid companyId, string? invitationMessage = null)
        {
            try
            {
                _logger.LogInformation("TourCompany {CompanyId} manually inviting Guide {GuideId} for TourDetails {TourDetailsId}",
                    companyId, guideId, tourDetailsId);

                // Validate TourDetails exists and belongs to company
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailsId);
                if (tourDetails == null)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 404,
                        Message = "Không tìm thấy TourDetails",
                        success = false
                    };
                }

                if (tourDetails.CreatedById != companyId)
                {
                    return new BaseResposeDto
                    {
                        StatusCode = 403,
                        Message = "Bạn không có quyền mời guide cho TourDetails này",
                        success = false
                    };
                }

                // ✅ Use TourGuideInvitationService instead of directly creating invitation
                // This will use the enhanced logic with rejection checking
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var invitationService = scope.ServiceProvider.GetRequiredService<ITourGuideInvitationService>();

                    var result = await invitationService.CreateManualInvitationAsync(tourDetailsId, guideId, companyId, invitationMessage);
                    
                    _logger.LogInformation("Manual invitation result for TourDetails {TourDetailsId}, Guide {GuideId}: Status={StatusCode}, Success={Success}, Message={Message}",
                        tourDetailsId, guideId, result.StatusCode, result.success, result.Message);

                    return result;
                }
                catch (Exception serviceEx)
                {
                    _logger.LogError(serviceEx, "Error using TourGuideInvitationService for manual invitation");
                    
                    return new BaseResposeDto {
                        StatusCode = 500,
                        Message = "Có lỗi xảy ra khi xử lý lời mời",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating manual invitation for TourDetails {TourDetailsId} with Guide {GuideId}", 
                    tourDetailsId, guideId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Có lỗi xảy ra khi gửi lời mời",
                    success = false
                };
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get next available sort order for timeline items
        /// </summary>
        private async Task<int> GetNextSortOrderAsync(Guid tourDetailsId)
        {
            var existingItems = await _unitOfWork.TimelineItemRepository
                .GetAllAsync(t => t.TourDetailsId == tourDetailsId && !t.IsDeleted);
            
            return existingItems.Any() ? existingItems.Max(t => t.SortOrder) + 1 : 1;
        }

        /// <summary>
        /// Cancel all pending invitations for a TourDetails
        /// </summary>
        private async Task CancelPendingInvitationsAsync(Guid tourDetailsId)
        {
            try
            {
                var pendingInvitations = await _unitOfWork.TourDetailsSpecialtyShopRepository
                    .GetAllAsync(inv => inv.TourDetailsId == tourDetailsId && 
                                        inv.Status == ShopInvitationStatus.Pending);

                foreach (var invitation in pendingInvitations)
                {
                    invitation.Status = ShopInvitationStatus.Cancelled;
                    invitation.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.TourDetailsSpecialtyShopRepository.UpdateAsync(invitation);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling pending invitations for TourDetails {TourDetailsId}", tourDetailsId);
                // Don't throw - this is a cleanup operation
            }
        }

        /// <summary>
        /// DEPRECATED: Email sending removed for performance optimization
        /// Chỉ ghi log thay vì gửi email để cải thiện hiệu suất hệ thống
        /// </summary>
        private async Task TriggerApprovalEmailsAsync(TourDetails tourDetails, Guid adminId)
        {
            try
            {
                // Get pending shop invitations for logging only
                var shopInvitations = await _unitOfWork.TourDetailsSpecialtyShopRepository
                    .GetAllAsync(inv => inv.TourDetailsId == tourDetails.Id &&
                                        inv.Status == ShopInvitationStatus.Pending);

                foreach (var invitation in shopInvitations)
                {
                    // PERFORMANCE OPTIMIZATION: Chỉ ghi log, không gửi email
                    _logger.LogInformation("SpecialtyShop {ShopId} invitation for TourDetails {TourDetailsId} - Email sending disabled for performance",
                        invitation.SpecialtyShopId, tourDetails.Id);
                }

                _logger.LogInformation("Skipped {EmailCount} approval emails for TourDetails {TourDetailsId} - Performance optimization",
                    shopInvitations.Count(), tourDetails.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing approval notifications for TourDetails {TourDetailsId}", tourDetails.Id);
                // Don't throw - this is a notification operation
            }
        }

        /// <summary>
        /// Trigger automatic guide invitations when admin approves TourDetails
        /// </summary>
        private async Task TriggerAutomaticGuideInvitationsAsync(TourDetails tourDetails, Guid adminId)
        {
            try
            {
                _logger.LogInformation("Triggering automatic guide invitations for approved TourDetails {TourDetailsId}", tourDetails.Id);

                // Get TourGuideInvitationService using DI
                using var scope = _serviceProvider.CreateScope();
                var invitationService = scope.ServiceProvider.GetRequiredService<ITourGuideInvitationService>();

                // Create automatic invitations for suitable guides
                var result = await invitationService.CreateAutomaticInvitationsAsync(tourDetails.Id, adminId);

                if (result.success)
                {
                    _logger.LogInformation("Successfully triggered automatic guide invitations for TourDetails {TourDetailsId}: {Message}",
                        tourDetails.Id, result.Message);

                    // Send notification to TourCompany about approval and guide invitations
                    try
                    {
                        using var notificationScope = _serviceProvider.CreateScope();
                        var tourCompanyNotificationService = notificationScope.ServiceProvider
                            .GetRequiredService<ITourCompanyNotificationService>();

                        await tourCompanyNotificationService.NotifyTourApprovalAsync(
                            tourDetails.CreatedById,
                            tourDetails.Title,
                            tourDetails.CommentApproved);

                        _logger.LogInformation("Successfully sent tour approval notification to TourCompany {CompanyId}", 
                            tourDetails.CreatedById);
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogError(notificationEx, "Error sending tour approval notification for TourDetails {TourDetailsId}", 
                            tourDetails.Id);
                        // Don't fail the main process if notification fails
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to trigger automatic guide invitations for TourDetails {TourDetailsId}: {Message}", 
                        tourDetails.Id, result.Message);

                    // Even if automatic invitations fail, still send approval notification to company
                    try
                    {
                        using var notificationScope = _serviceProvider.CreateScope();
                        var tourCompanyNotificationService = notificationScope.ServiceProvider
                            .GetRequiredService<ITourCompanyNotificationService>();

                        var approvalMessage = !string.IsNullOrEmpty(result.Message) 
                            ? $"Tour đã được duyệt nhưng có lỗi tự động mời hướng dẫn viên: {result.Message}. Vui lòng mời thủ công."
                            : tourDetails.CommentApproved;

                        await tourCompanyNotificationService.NotifyTourApprovalAsync(
                            tourDetails.CreatedById,
                            tourDetails.Title,
                            approvalMessage);

                        _logger.LogInformation("Successfully sent tour approval notification (with guide invitation warning) to TourCompany {CompanyId}", 
                            tourDetails.CreatedById);
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogError(notificationEx, "Error sending tour approval notification for TourDetails {TourDetailsId}", 
                            tourDetails.Id);
                        // Don't fail the main process if notification fails
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering automatic guide invitations for TourDetails {TourDetailsId}", tourDetails.Id);
                
                // Send notification to TourCompany about approval but mention manual invitation needed
                try
                {
                    using var notificationScope = _serviceProvider.CreateScope();
                    var tourCompanyNotificationService = notificationScope.ServiceProvider
                        .GetRequiredService<ITourCompanyNotificationService>();

                    await tourCompanyNotificationService.NotifyTourApprovalAsync(
                        tourDetails.CreatedById,
                        tourDetails.Title,
                        $"Tour đã được duyệt nhưng có lỗi hệ thống khi tự động mời hướng dẫn viên. Vui lòng mời hướng dẫn viên thủ công. Lỗi: {ex.Message}");

                    _logger.LogInformation("Successfully sent tour approval notification (with error warning) to TourCompany {CompanyId}", 
                        tourDetails.CreatedById);
                }
                catch (Exception notificationEx)
                {
                    _logger.LogError(notificationEx, "Error sending tour approval notification for TourDetails {TourDetailsId}", 
                        tourDetails.Id);
                }

                // Don't throw - this is a side operation, shouldn't break the main approval flow
            }
        }

        /// <summary>
        /// Get image URLs from request data
        /// </summary>
        private List<string> GetImageUrlListFromRequest(List<string>? imageUrls, string? singleImageUrl)
        {
            var result = new List<string>();

            if (imageUrls != null && imageUrls.Any())
            {
                result.AddRange(imageUrls.Where(url => !string.IsNullOrWhiteSpace(url)));
            }
            
            if (!string.IsNullOrWhiteSpace(singleImageUrl))
            {
                result.Add(singleImageUrl);
            }

            return result;
        }

        /// <summary>
        /// Get image URLs from request data (legacy method for string return)
        /// </summary>
        private string? GetImageUrlsFromRequest(List<string>? imageUrls, string? singleImageUrl)
        {
            if (imageUrls != null && imageUrls.Any())
            {
                return string.Join(",", imageUrls.Where(url => !string.IsNullOrWhiteSpace(url)));
            }
            
            if (!string.IsNullOrWhiteSpace(singleImageUrl))
            {
                return singleImageUrl;
            }

            return null;
        }

        #endregion
    }
}
