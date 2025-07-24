using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service implementation cho quản lý TourOperation
    /// </summary>
    public class TourOperationService : BaseService, ITourOperationService
    {
        private readonly ILogger<TourOperationService> _logger;
        private readonly ICurrentUserService _currentUserService;
        private readonly IServiceProvider _serviceProvider;

        public TourOperationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TourOperationService> logger,
            ICurrentUserService currentUserService,
            IServiceProvider serviceProvider) : base(mapper, unitOfWork)
        {
            _logger = logger;
            _currentUserService = currentUserService;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Tạo operation mới cho TourSlot
        /// </summary>
        public async Task<ResponseCreateOperationDto> CreateOperationAsync(RequestCreateOperationDto request)
        {
            try
            {
                _logger.LogInformation("Creating operation for TourDetails {TourDetailsId}", request.TourDetailsId);

                // 1. Validate TourDetails exists
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(request.TourDetailsId);
                if (tourDetails == null)
                {
                    return new ResponseCreateOperationDto
                    {
                        success = false,
                        Message = "TourDetails không tồn tại"
                    };
                }

                // 2. Check TourDetails chưa có operation
                var existingOperation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(request.TourDetailsId);
                if (existingOperation != null)
                {
                    return new ResponseCreateOperationDto
                    {
                        success = false,
                        Message = "TourDetails đã có operation"
                    };
                }

                // 3. Validate với Template constraints
                var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(tourDetails.TourTemplateId);
                if (template == null)
                {
                    return new ResponseCreateOperationDto
                    {
                        success = false,
                        Message = "Template không tồn tại"
                    };
                }

                // MaxGuests validation removed - now managed at operation level

                // 4. VALIDATE TOUR READINESS: Skip for initial creation, only validate for updates
                // Allow creating TourOperation for Pending TourDetails during wizard flow
                // Validation will be enforced when TourDetails is approved by admin or public
                if (tourDetails.Status == TourDetailsStatus.Approved || tourDetails.Status == TourDetailsStatus.Public)
                {
                    var (isReady, readinessError) = await ValidateTourDetailsReadinessAsync(request.TourDetailsId);
                    if (!isReady)
                    {
                        _logger.LogWarning("TourDetails {TourDetailsId} not ready for operation creation: {Error}",
                            request.TourDetailsId, readinessError);
                        return new ResponseCreateOperationDto
                        {
                            success = false,
                            Message = readinessError
                        };
                    }
                }

                // 5. Validate TourGuide (nếu có)
                if (request.GuideId.HasValue)
                {
                    var tourGuide = await _unitOfWork.TourGuideRepository.GetByIdAsync(request.GuideId.Value);
                    if (tourGuide == null)
                    {
                        return new ResponseCreateOperationDto
                        {
                            success = false,
                            Message = "TourGuide không hợp lệ"
                        };
                    }
                }

                // 6. Get current User ID directly
                var currentUserId = _currentUserService.GetCurrentUserId();
                _logger.LogInformation("Creating TourOperation for User ID: {UserId}", currentUserId);

                // 7. Create operation using User.Id directly
                var operation = new TourOperation
                {
                    Id = Guid.NewGuid(),
                    TourDetailsId = request.TourDetailsId,
                    Price = request.Price,
                    MaxGuests = request.MaxSeats,
                    Description = request.Description,
                    TourGuideId = request.GuideId,
                    Notes = request.Notes,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = currentUserId // Use User.Id directly
                };

                await _unitOfWork.TourOperationRepository.AddAsync(operation);
                await _unitOfWork.SaveChangesAsync();

                // 8. Sync slot capacity with the new TourOperation MaxGuests
                var tourSlotService = _serviceProvider.GetRequiredService<ITourSlotService>();
                await tourSlotService.SyncSlotsCapacityAsync(request.TourDetailsId, operation.MaxGuests);

                // 9. Return response
                var operationDto = _mapper.Map<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto>(operation);

                _logger.LogInformation("Operation created successfully for TourDetails {TourDetailsId} with MaxGuests {MaxGuests}", 
                    request.TourDetailsId, operation.MaxGuests);

                return new ResponseCreateOperationDto
                {
                    success = true,
                    Message = "Tạo operation thành công",
                    Operation = operationDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating operation for TourDetails {TourDetailsId}", request.TourDetailsId);
                return new ResponseCreateOperationDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi tạo operation"
                };
            }
        }

        /// <summary>
        /// Lấy operation theo TourDetails ID
        /// </summary>
        public async Task<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto?> GetOperationByTourDetailsAsync(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Getting operation for TourDetails {TourDetailsId}", tourDetailsId);

                var operation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(tourDetailsId);
                if (operation == null) return null;

                var operationDto = _mapper.Map<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto>(operation);

                // Get current booking count
                operationDto.BookedSeats = await _unitOfWork.TourBookingRepository.GetTotalBookedGuestsAsync(operation.Id);

                return operationDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting operation for TourDetails {TourDetailsId}", tourDetailsId);
                return null;
            }
        }

        /// <summary>
        /// Lấy operation theo Operation ID
        /// </summary>
        public async Task<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto?> GetOperationByIdAsync(Guid operationId)
        {
            try
            {
                _logger.LogInformation("Getting operation {OperationId}", operationId);

                var operation = await _unitOfWork.TourOperationRepository.GetByIdAsync(operationId);
                if (operation == null) return null;

                var operationDto = _mapper.Map<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto>(operation);

                // Get current booking count
                operationDto.BookedSeats = await _unitOfWork.TourBookingRepository.GetTotalBookedGuestsAsync(operation.Id);

                return operationDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting operation {OperationId}", operationId);
                return null;
            }
        }

        /// <summary>
        /// Cập nhật operation - Enhanced với business rules tương tự TourDetails
        /// </summary>
        public async Task<ResponseUpdateOperationDto> UpdateOperationAsync(Guid id, RequestUpdateOperationDto request)
        {
            try
            {
                _logger.LogInformation("Updating operation {OperationId} with request: {@Request}", id, request);

                // 1. Get existing operation with navigation properties
                var operation = await _unitOfWork.TourOperationRepository.GetByIdAsync(id);
                if (operation == null)
                {
                    return new ResponseUpdateOperationDto
                    {
                        success = false,
                        Message = "Operation không tồn tại"
                    };
                }

                // Store original values for debugging and logic
                var originalStatus = operation.Status;
                var originalPrice = operation.Price;
                var originalGuideId = operation.TourGuideId;

                _logger.LogInformation("Operation {OperationId} current state - Status: {Status}, Price: {Price}, GuideId: {GuideId}", 
                    id, originalStatus, originalPrice, originalGuideId);

                // 2. BUSINESS RULE 1: Check if tour guide is ASSIGNED - prevent editing if guide is already assigned
                // This is the STRONGEST rule - once a guide is assigned and working, NO EDITS allowed
                bool hasGuideAssigned = operation.TourGuideId != null;
                if (hasGuideAssigned)
                {
                    _logger.LogWarning("Operation {OperationId} edit blocked - Guide already assigned: {GuideId}", id, operation.TourGuideId);
                    return new ResponseUpdateOperationDto
                    {
                        success = false,
                        Message = "Đã có hướng dẫn viên tham gia tour operation, không thể edit nữa"
                    };
                }

                // 3. Validate constraints
                if (request.MaxSeats.HasValue)
                {
                    var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(operation.TourDetailsId);
                    var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(tourDetails!.TourTemplateId);

                    // MaxGuests validation removed - now managed at operation level
                }

                // 4. FIXED: Only validate TourGuide if explicitly trying to assign one
                // Allow updating other fields without needing to validate GuideId
                if (request.GuideId.HasValue)
                {
                    // Only validate if we're actually trying to assign a guide
                    var tourGuide = await _unitOfWork.TourGuideRepository.GetByIdAsync(request.GuideId.Value);
                    if (tourGuide == null)
                    {
                        return new ResponseUpdateOperationDto
                        {
                            success = false,
                            Message = "TourGuide không hợp lệ"
                        };
                    }
                }

                // 5. Update fields
                var oldMaxGuests = operation.MaxGuests;
                if (request.Price.HasValue) 
                {
                    _logger.LogInformation("Updating operation {OperationId} price from {OldPrice} to {NewPrice}", 
                        id, operation.Price, request.Price.Value);
                    operation.Price = request.Price.Value;
                }
                if (request.MaxSeats.HasValue) operation.MaxGuests = request.MaxSeats.Value;
                if (request.Description != null) operation.Description = request.Description;
                
                // Only update TourGuideId if explicitly provided in request
                if (request.GuideId.HasValue) 
                {
                    operation.TourGuideId = request.GuideId;
                }
                
                if (request.Notes != null) operation.Notes = request.Notes;
                if (request.IsActive.HasValue) operation.IsActive = request.IsActive.Value;

                // 6. BUSINESS RULE 2: Check TourDetails status and update if needed
                // Get the related TourDetails to check its status
                var relatedTourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(operation.TourDetailsId);
                if (relatedTourDetails == null)
                {
                    return new ResponseUpdateOperationDto
                    {
                        success = false,
                        Message = "TourDetails liên quan không tồn tại"
                    };
                }

                bool tourDetailsStatusWillChange = false;
                var originalTourDetailsStatus = relatedTourDetails.Status;

                // If TourDetails status is PendingConfirmation (waiting for guide acceptance) → send back to admin for approval
                // This only applies when NO guide is assigned yet (rule 2 comes after rule 1)
                if (originalTourDetailsStatus == TourDetailsStatus.AwaitingGuideAssignment)
                {
                    relatedTourDetails.Status = TourDetailsStatus.AwaitingAdminApproval; // Reset to "pending admin approval"
                    relatedTourDetails.CommentApproved = null; // Clear previous admin comment
                    relatedTourDetails.UpdatedAt = DateTime.UtcNow;
                    
                    tourDetailsStatusWillChange = true;
                    _logger.LogInformation("TourDetails {TourDetailsId} status changed from AwaitingGuideAssignment to AwaitingAdminApproval due to operation edit", 
                        operation.TourDetailsId);
                    
                    // Update TourDetails in database
                    await _unitOfWork.TourDetailsRepository.UpdateAsync(relatedTourDetails);
                }

                operation.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("About to save operation {OperationId} - New Price: {Price}, TourDetails Status: {TourDetailsStatus}", 
                    id, operation.Price, relatedTourDetails.Status);

                await _unitOfWork.TourOperationRepository.UpdateAsync(operation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully saved operation {OperationId}", id);

                // 7. Send notification if TourDetails status changed back to admin approval
                if (tourDetailsStatusWillChange)
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                        
                        // Create in-app notification for tour company
                        await notificationService.CreateNotificationAsync(new DTOs.Request.Notification.CreateNotificationDto
                        {
                            UserId = operation.CreatedById,
                            Title = "📝 Tour đã gửi lại admin",
                            Message = $"Tour '{relatedTourDetails.Title}' đã được gửi lại cho admin duyệt do có chỉnh sửa trong lúc chờ hướng dẫn viên được phân công.",
                            Type = DataAccessLayer.Enums.NotificationType.Tour,
                            Priority = DataAccessLayer.Enums.NotificationPriority.Medium,
                            Icon = "📝",
                            ActionUrl = "/tours/awaiting-admin-approval"
                        });

                        _logger.LogInformation("Sent notification about TourDetails status change back to admin approval for TourDetails {TourDetailsId}", operation.TourDetailsId);
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogError(notificationEx, "Error sending notification for TourDetails status change on TourDetails {TourDetailsId}", operation.TourDetailsId);
                        // Don't fail the update if notification fails
                    }
                }

                // 8. Sync slot capacity if MaxGuests changed
                if (request.MaxSeats.HasValue && oldMaxGuests != operation.MaxGuests)
                {
                    var tourSlotService = _serviceProvider.GetRequiredService<ITourSlotService>();
                    await tourSlotService.SyncSlotsCapacityAsync(operation.TourDetailsId, operation.MaxGuests);
                    
                    _logger.LogInformation("Synced slot capacity for TourDetails {TourDetailsId} from {OldCapacity} to {NewCapacity}", 
                        operation.TourDetailsId, oldMaxGuests, operation.MaxGuests);
                }

                // 9. Get fresh data from database to return
                var updatedOperation = await _unitOfWork.TourOperationRepository.GetByIdAsync(id);
                var operationDto = _mapper.Map<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto>(updatedOperation);

                // 10. Prepare response message based on status change
                string message = "Cập nhật operation thành công";
                if (tourDetailsStatusWillChange)
                {
                    message += ". Tour đã được gửi lại cho admin duyệt do có thay đổi trong lúc chờ hướng dẫn viên được phân công.";
                }

                _logger.LogInformation("Operation {OperationId} updated successfully - Response Price: {Price}, TourDetails Status: {TourDetailsStatus}", 
                    id, operationDto.Price, relatedTourDetails.Status);

                return new ResponseUpdateOperationDto
                {
                    success = true,
                    Message = message,
                    Operation = operationDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating operation {OperationId}", id);
                return new ResponseUpdateOperationDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi cập nhật operation"
                };
            }
        }

        /// <summary>
        /// Xóa operation (soft delete)
        /// </summary>
        public async Task<BaseResposeDto> DeleteOperationAsync(Guid id)
        {
            try
            {
                _logger.LogInformation("Deleting operation {OperationId}", id);

                var operation = await _unitOfWork.TourOperationRepository.GetByIdAsync(id);
                if (operation == null)
                {
                    return new BaseResposeDto
                    {
                        success = false,
                        Message = "Operation không tồn tại"
                    };
                }

                // Soft delete
                operation.IsActive = false;
                operation.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourOperationRepository.UpdateAsync(operation);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Operation {OperationId} deleted successfully", id);

                return new BaseResposeDto
                {
                    success = true,
                    Message = "Xóa operation thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting operation {OperationId}", id);
                return new BaseResposeDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi xóa operation"
                };
            }
        }

        /// <summary>
        /// Lấy danh sách operations với filtering
        /// </summary>
        public async Task<List<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto>> GetOperationsAsync(
            Guid? tourTemplateId = null,
            Guid? guideId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool includeInactive = false)
        {
            try
            {
                _logger.LogInformation("Getting operations with filters");

                var (operations, _) = await _unitOfWork.TourOperationRepository.GetPaginatedAsync(
                    1, 1000, guideId, tourTemplateId, includeInactive);

                var operationDtos = _mapper.Map<List<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto>>(operations);

                // Update booking counts for each operation
                foreach (var dto in operationDtos)
                {
                    dto.BookedSeats = await _unitOfWork.TourBookingRepository.GetTotalBookedGuestsAsync(dto.Id);
                }

                return operationDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting operations");
                return new List<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto>();
            }
        }

        /// <summary>
        /// Validate business rules cho operation
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ValidateOperationAsync(RequestCreateOperationDto request)
        {
            try
            {
                // Check TourDetails exists
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(request.TourDetailsId);
                if (tourDetails == null)
                    return (false, "TourDetails không tồn tại");

                // Check TourDetails chưa có operation
                var existingOperation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(request.TourDetailsId);
                if (existingOperation != null)
                    return (false, "TourDetails đã có operation");

                // Check template constraints
                var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(tourDetails.TourTemplateId);
                if (template == null)
                    return (false, "Template không tồn tại");

                // MaxGuests validation removed - now managed at operation level

                // Check TourGuide if provided
                if (request.GuideId.HasValue)
                {
                    var tourGuide = await _unitOfWork.TourGuideRepository.GetByIdAsync(request.GuideId.Value);
                    if (tourGuide == null)
                        return (false, "TourGuide không hợp lệ");
                }

                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating operation");
                return (false, "Lỗi validation");
            }
        }

        /// <summary>
        /// Check xem TourDetails có thể tạo operation không
        /// </summary>
        public async Task<bool> CanCreateOperationForTourDetailsAsync(Guid tourDetailsId)
        {
            try
            {
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailsId);
                if (tourDetails == null) return false;

                var existingOperation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(tourDetailsId);
                return existingOperation == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if can create operation for TourDetails {TourDetailsId}", tourDetailsId);
                return false;
            }
        }

        /// <summary>
        /// Validate TourDetails readiness cho việc tạo TourOperation (public tour)
        /// Kiểm tra TourGuide assignment và SpecialtyShop participation
        /// </summary>
        public async Task<(bool IsReady, string ErrorMessage)> ValidateTourDetailsReadinessAsync(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Validating TourDetails readiness for TourDetailsId {TourDetailsId}", tourDetailsId);

                // 1. Check TourDetails exists and is approved
                var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(tourDetailsId);
                if (tourDetails == null)
                {
                    return (false, "TourDetails không tồn tại");
                }

                // Allow creating TourOperation for Pending TourDetails (initial creation)
                // Only require approval for updates or when TourGuide assignment is needed
                // Also allow Public status for ongoing operation management
                if (tourDetails.Status != TourDetailsStatus.Approved &&
                    tourDetails.Status != TourDetailsStatus.Pending &&
                    tourDetails.Status != TourDetailsStatus.Public)
                {
                    return (false, "TourDetails phải ở trạng thái Pending, Approved hoặc Public");
                }

                var missingRequirements = new List<string>();

                // 2. Check TourGuide Assignment (only for approved and public tours)
                if (tourDetails.Status == TourDetailsStatus.Approved || tourDetails.Status == TourDetailsStatus.Public)
                {
                    bool hasTourGuide = await ValidateTourGuideAssignmentAsync(tourDetailsId);
                    if (!hasTourGuide)
                    {
                        missingRequirements.Add("Chưa có hướng dẫn viên được phân công");
                    }
                }

                // 3. Check SpecialtyShop Participation (only for approved and public tours)
                // For pending tours, shops can be selected in timeline but not invited yet
                if (tourDetails.Status == TourDetailsStatus.Approved || tourDetails.Status == TourDetailsStatus.Public)
                {
                    bool hasSpecialtyShop = await ValidateSpecialtyShopParticipationAsync(tourDetailsId);
                    if (!hasSpecialtyShop)
                    {
                        missingRequirements.Add("Chưa có cửa hàng đặc sản tham gia");
                    }
                }

                // 4. Return result
                if (missingRequirements.Any())
                {
                    var errorMessage = "Không thể tạo tour operation: " + string.Join(", ", missingRequirements);
                    _logger.LogWarning("TourDetails {TourDetailsId} not ready: {ErrorMessage}", tourDetailsId, errorMessage);
                    return (false, errorMessage);
                }

                _logger.LogInformation("TourDetails {TourDetailsId} is ready for TourOperation creation", tourDetailsId);
                return (true, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating TourDetails readiness for {TourDetailsId}", tourDetailsId);
                return (false, "Lỗi kiểm tra tính sẵn sàng của tour");
            }
        }

        /// <summary>
        /// Get detailed readiness status của TourDetails cho frontend checking
        /// </summary>
        public async Task<TourDetailsReadinessDto> GetTourDetailsReadinessAsync(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Getting TourDetails readiness info for {TourDetailsId}", tourDetailsId);

                var readinessDto = new TourDetailsReadinessDto
                {
                    TourDetailsId = tourDetailsId
                };

                // 1. Get TourGuide information
                readinessDto.GuideInfo = await GetTourGuideReadinessInfoAsync(tourDetailsId);
                readinessDto.HasTourGuide = readinessDto.GuideInfo.HasDirectAssignment || readinessDto.GuideInfo.AcceptedInvitations > 0;
                readinessDto.AcceptedGuideInvitations = readinessDto.GuideInfo.AcceptedInvitations;

                // 2. Get SpecialtyShop information
                readinessDto.ShopInfo = await GetSpecialtyShopReadinessInfoAsync(tourDetailsId);
                readinessDto.HasSpecialtyShop = readinessDto.ShopInfo.AcceptedInvitations > 0;
                readinessDto.AcceptedShopInvitations = readinessDto.ShopInfo.AcceptedInvitations;

                // 3. Determine overall readiness
                readinessDto.IsReady = readinessDto.HasTourGuide && readinessDto.HasSpecialtyShop;

                // 4. Build missing requirements list
                if (!readinessDto.HasTourGuide)
                {
                    readinessDto.MissingRequirements.Add("Chưa có hướng dẫn viên được phân công");
                }
                if (!readinessDto.HasSpecialtyShop)
                {
                    readinessDto.MissingRequirements.Add("Chưa có cửa hàng đặc sản tham gia");
                }

                // 5. Build message
                if (readinessDto.IsReady)
                {
                    readinessDto.Message = "Tour đã sẵn sàng để public";
                }
                else
                {
                    readinessDto.Message = "Tour cần có đầy đủ hướng dẫn viên và cửa hàng đặc sản trước khi public";
                }

                return readinessDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TourDetails readiness info for {TourDetailsId}", tourDetailsId);
                return new TourDetailsReadinessDto
                {
                    TourDetailsId = tourDetailsId,
                    IsReady = false,
                    Message = "Lỗi kiểm tra tính sẵn sàng của tour"
                };
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Validate TourGuide assignment cho TourDetails
        /// </summary>
        private async Task<bool> ValidateTourGuideAssignmentAsync(Guid tourDetailsId)
        {
            try
            {
                // 1. Check if TourDetails already has TourOperation with TourGuideId
                var existingOperation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(tourDetailsId);
                if (existingOperation?.TourGuideId != null)
                {
                    _logger.LogInformation("TourDetails {TourDetailsId} has direct guide assignment: {TourGuideId}",
                        tourDetailsId, existingOperation.TourGuideId);
                    return true;
                }

                // 2. Check if any TourGuideInvitation has been accepted
                var invitations = await _unitOfWork.TourGuideInvitationRepository.GetByTourDetailsAsync(tourDetailsId);
                var acceptedInvitations = invitations.Where(i => i.Status == InvitationStatus.Accepted && i.IsActive);

                if (acceptedInvitations.Any())
                {
                    _logger.LogInformation("TourDetails {TourDetailsId} has {Count} accepted guide invitations",
                        tourDetailsId, acceptedInvitations.Count());
                    return true;
                }

                _logger.LogWarning("TourDetails {TourDetailsId} has no guide assignment", tourDetailsId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating TourGuide assignment for {TourDetailsId}", tourDetailsId);
                return false;
            }
        }

        /// <summary>
        /// Validate SpecialtyShop participation cho TourDetails
        /// </summary>
        private async Task<bool> ValidateSpecialtyShopParticipationAsync(Guid tourDetailsId)
        {
            try
            {
                var shopInvitations = await _unitOfWork.TourDetailsSpecialtyShopRepository.GetByTourDetailsIdAsync(tourDetailsId);
                var acceptedShops = shopInvitations.Where(s => s.Status == ShopInvitationStatus.Accepted && s.IsActive);

                if (acceptedShops.Any())
                {
                    _logger.LogInformation("TourDetails {TourDetailsId} has {Count} accepted shop invitations",
                        tourDetailsId, acceptedShops.Count());
                    return true;
                }

                _logger.LogWarning("TourDetails {TourDetailsId} has no accepted shop invitations", tourDetailsId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating SpecialtyShop participation for {TourDetailsId}", tourDetailsId);
                return false;
            }
        }

        /// <summary>
        /// Get detailed TourGuide readiness information
        /// </summary>
        private async Task<TourGuideReadinessInfo> GetTourGuideReadinessInfoAsync(Guid tourDetailsId)
        {
            try
            {
                var guideInfo = new TourGuideReadinessInfo();

                // 1. Check direct assignment in TourOperation
                var existingOperation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(tourDetailsId);
                if (existingOperation?.TourGuideId != null)
                {
                    guideInfo.HasDirectAssignment = true;
                    guideInfo.DirectlyAssignedGuideId = existingOperation.TourGuideId;

                    var tourGuide = await _unitOfWork.TourGuideRepository.GetByIdAsync(existingOperation.TourGuideId.Value);
                    guideInfo.DirectlyAssignedGuideName = tourGuide?.FullName ?? "Unknown Guide";
                }

                // 2. Get invitation statistics
                var invitations = await _unitOfWork.TourGuideInvitationRepository.GetByTourDetailsAsync(tourDetailsId);
                var activeInvitations = invitations.Where(i => i.IsActive).ToList();

                guideInfo.PendingInvitations = activeInvitations.Count(i => i.Status == InvitationStatus.Pending);
                guideInfo.AcceptedInvitations = activeInvitations.Count(i => i.Status == InvitationStatus.Accepted);
                guideInfo.RejectedInvitations = activeInvitations.Count(i => i.Status == InvitationStatus.Rejected);

                // 3. Get accepted guides details
                var acceptedInvitations = activeInvitations.Where(i => i.Status == InvitationStatus.Accepted);
                foreach (var invitation in acceptedInvitations)
                {
                    guideInfo.AcceptedGuides.Add(new AcceptedGuideInfo
                    {
                        GuideId = invitation.GuideId,
                        GuideName = invitation.TourGuide?.FullName ?? "Unknown Guide",
                        GuideEmail = invitation.TourGuide?.Email ?? "Unknown Email",
                        AcceptedAt = invitation.RespondedAt ?? invitation.UpdatedAt ?? DateTime.UtcNow
                    });
                }

                return guideInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting TourGuide readiness info for {TourDetailsId}", tourDetailsId);
                return new TourGuideReadinessInfo();
            }
        }

        /// <summary>
        /// Get detailed SpecialtyShop readiness information
        /// </summary>
        private async Task<SpecialtyShopReadinessInfo> GetSpecialtyShopReadinessInfoAsync(Guid tourDetailsId)
        {
            try
            {
                var shopInfo = new SpecialtyShopReadinessInfo();

                var shopInvitations = await _unitOfWork.TourDetailsSpecialtyShopRepository.GetByTourDetailsIdAsync(tourDetailsId);
                var activeInvitations = shopInvitations.Where(s => s.IsActive).ToList();

                shopInfo.PendingInvitations = activeInvitations.Count(s => s.Status == ShopInvitationStatus.Pending);
                shopInfo.AcceptedInvitations = activeInvitations.Count(s => s.Status == ShopInvitationStatus.Accepted);
                shopInfo.DeclinedInvitations = activeInvitations.Count(s => s.Status == ShopInvitationStatus.Declined);

                // Get accepted shops details
                var acceptedInvitations = activeInvitations.Where(s => s.Status == ShopInvitationStatus.Accepted);
                foreach (var invitation in acceptedInvitations)
                {
                    shopInfo.AcceptedShops.Add(new AcceptedShopInfo
                    {
                        ShopId = invitation.SpecialtyShopId,
                        ShopName = invitation.SpecialtyShop?.ShopName ?? "Unknown Shop",
                        ShopAddress = invitation.SpecialtyShop?.Address,
                        AcceptedAt = invitation.RespondedAt ?? invitation.UpdatedAt ?? DateTime.UtcNow
                    });
                }

                return shopInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting SpecialtyShop readiness info for {TourDetailsId}", tourDetailsId);
                return new SpecialtyShopReadinessInfo();
            }
        }

        #endregion
    }
}
