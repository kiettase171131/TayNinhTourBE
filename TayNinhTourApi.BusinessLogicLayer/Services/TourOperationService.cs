using AutoMapper;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
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

        public TourOperationService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<TourOperationService> logger,
            ICurrentUserService currentUserService) : base(mapper, unitOfWork)
        {
            _logger = logger;
            _currentUserService = currentUserService;
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
                        IsSuccess = false,
                        Message = "TourDetails không tồn tại"
                    };
                }

                // 2. Check TourDetails chưa có operation
                var existingOperation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(request.TourDetailsId);
                if (existingOperation != null)
                {
                    return new ResponseCreateOperationDto
                    {
                        IsSuccess = false,
                        Message = "TourDetails đã có operation"
                    };
                }

                // 3. Validate với Template constraints
                var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(tourDetails.TourTemplateId);
                if (template == null)
                {
                    return new ResponseCreateOperationDto
                    {
                        IsSuccess = false,
                        Message = "Template không tồn tại"
                    };
                }

                // MaxGuests validation removed - now managed at operation level

                // 4. Validate Guide (nếu có)
                if (request.GuideId.HasValue)
                {
                    var guide = await _unitOfWork.UserRepository.GetByIdAsync(request.GuideId.Value);
                    if (guide == null)
                    {
                        return new ResponseCreateOperationDto
                        {
                            IsSuccess = false,
                            Message = "Guide không hợp lệ"
                        };
                    }
                }

                // 5. Create operation
                var operation = new TourOperation
                {
                    Id = Guid.NewGuid(),
                    TourDetailsId = request.TourDetailsId,
                    Price = request.Price,
                    MaxGuests = request.MaxSeats,
                    Description = request.Description,
                    GuideId = request.GuideId,
                    Notes = request.Notes,
                    IsActive = request.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = _currentUserService.GetCurrentUserId()
                };

                await _unitOfWork.TourOperationRepository.AddAsync(operation);
                await _unitOfWork.SaveChangesAsync();

                // 6. Return response
                var operationDto = _mapper.Map<TourOperationDto>(operation);

                _logger.LogInformation("Operation created successfully for TourDetails {TourDetailsId}", request.TourDetailsId);

                return new ResponseCreateOperationDto
                {
                    IsSuccess = true,
                    Message = "Tạo operation thành công",
                    Operation = operationDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating operation for TourDetails {TourDetailsId}", request.TourDetailsId);
                return new ResponseCreateOperationDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi tạo operation"
                };
            }
        }

        /// <summary>
        /// Lấy operation theo TourDetails ID
        /// </summary>
        public async Task<TourOperationDto?> GetOperationByTourDetailsAsync(Guid tourDetailsId)
        {
            try
            {
                _logger.LogInformation("Getting operation for TourDetails {TourDetailsId}", tourDetailsId);

                var operation = await _unitOfWork.TourOperationRepository.GetByTourDetailsAsync(tourDetailsId);
                if (operation == null) return null;

                return _mapper.Map<TourOperationDto>(operation);
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
        public async Task<TourOperationDto?> GetOperationByIdAsync(Guid operationId)
        {
            try
            {
                _logger.LogInformation("Getting operation {OperationId}", operationId);

                var operation = await _unitOfWork.TourOperationRepository.GetByIdAsync(operationId);
                if (operation == null) return null;

                return _mapper.Map<TourOperationDto>(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting operation {OperationId}", operationId);
                return null;
            }
        }

        /// <summary>
        /// Cập nhật operation
        /// </summary>
        public async Task<ResponseUpdateOperationDto> UpdateOperationAsync(Guid id, RequestUpdateOperationDto request)
        {
            try
            {
                _logger.LogInformation("Updating operation {OperationId}", id);

                // 1. Get existing operation
                var operation = await _unitOfWork.TourOperationRepository.GetByIdAsync(id);
                if (operation == null)
                {
                    return new ResponseUpdateOperationDto
                    {
                        IsSuccess = false,
                        Message = "Operation không tồn tại"
                    };
                }

                // 2. Validate constraints
                if (request.MaxSeats.HasValue)
                {
                    var tourDetails = await _unitOfWork.TourDetailsRepository.GetByIdAsync(operation.TourDetailsId);
                    var template = await _unitOfWork.TourTemplateRepository.GetByIdAsync(tourDetails!.TourTemplateId);

                    // MaxGuests validation removed - now managed at operation level
                }

                // 3. Validate Guide (nếu thay đổi)
                if (request.GuideId.HasValue)
                {
                    var guide = await _unitOfWork.UserRepository.GetByIdAsync(request.GuideId.Value);
                    if (guide == null)
                    {
                        return new ResponseUpdateOperationDto
                        {
                            IsSuccess = false,
                            Message = "Guide không hợp lệ"
                        };
                    }
                }

                // 4. Update fields
                if (request.Price.HasValue) operation.Price = request.Price.Value;
                if (request.MaxSeats.HasValue) operation.MaxGuests = request.MaxSeats.Value;
                if (request.Description != null) operation.Description = request.Description;
                if (request.GuideId.HasValue) operation.GuideId = request.GuideId;
                if (request.Notes != null) operation.Notes = request.Notes;
                if (request.IsActive.HasValue) operation.IsActive = request.IsActive.Value;

                operation.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.TourOperationRepository.UpdateAsync(operation);
                await _unitOfWork.SaveChangesAsync();

                var operationDto = _mapper.Map<TourOperationDto>(operation);

                _logger.LogInformation("Operation {OperationId} updated successfully", id);

                return new ResponseUpdateOperationDto
                {
                    IsSuccess = true,
                    Message = "Cập nhật operation thành công",
                    Operation = operationDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating operation {OperationId}", id);
                return new ResponseUpdateOperationDto
                {
                    IsSuccess = false,
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
                        IsSuccess = false,
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
                    IsSuccess = true,
                    Message = "Xóa operation thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting operation {OperationId}", id);
                return new BaseResposeDto
                {
                    IsSuccess = false,
                    Message = "Có lỗi xảy ra khi xóa operation"
                };
            }
        }

        /// <summary>
        /// Lấy danh sách operations với filtering
        /// </summary>
        public async Task<List<TourOperationDto>> GetOperationsAsync(
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

                return _mapper.Map<List<TourOperationDto>>(operations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting operations");
                return new List<TourOperationDto>();
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

                // Check guide if provided
                if (request.GuideId.HasValue)
                {
                    var guide = await _unitOfWork.UserRepository.GetByIdAsync(request.GuideId.Value);
                    if (guide == null)
                        return (false, "Guide không hợp lệ");
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
    }
}
