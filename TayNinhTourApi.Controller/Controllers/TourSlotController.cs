using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller quản lý TourSlots - các slot thời gian cụ thể của tour
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TourSlotController : ControllerBase
    {
        private readonly ITourSlotService _tourSlotService;
        private readonly ILogger<TourSlotController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public TourSlotController(
            ITourSlotService tourSlotService,
            ILogger<TourSlotController> logger,
            IUnitOfWork unitOfWork)
        {
            _tourSlotService = tourSlotService;
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Lấy danh sách TourSlots theo filter
        /// </summary>
        /// <param name="tourTemplateId">ID của TourTemplate (optional)</param>
        /// <param name="tourDetailsId">ID của TourDetails (optional)</param>
        /// <param name="fromDate">Từ ngày (optional)</param>
        /// <param name="toDate">Đến ngày (optional)</param>
        /// <param name="scheduleDay">Ngày trong tuần (optional)</param>
        /// <param name="includeInactive">Có bao gồm slots không active không (default: false)</param>
        /// <returns>Danh sách TourSlots</returns>
        [HttpGet]
        public async Task<IActionResult> GetSlots(
            [FromQuery] Guid? tourTemplateId = null,
            [FromQuery] Guid? tourDetailsId = null,
            [FromQuery] DateOnly? fromDate = null,
            [FromQuery] DateOnly? toDate = null,
            [FromQuery] ScheduleDay? scheduleDay = null,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                var slots = await _tourSlotService.GetSlotsAsync(
                    tourTemplateId, 
                    tourDetailsId, 
                    fromDate, 
                    toDate, 
                    scheduleDay, 
                    includeInactive);

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách tour slots thành công",
                    data = slots,
                    totalCount = slots.Count(),
                    filters = new
                    {
                        tourTemplateId,
                        tourDetailsId,
                        fromDate,
                        toDate,
                        scheduleDay,
                        includeInactive
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour slots with filters: TourTemplateId={TourTemplateId}, TourDetailsId={TourDetailsId}", 
                    tourTemplateId, tourDetailsId);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy danh sách tour slots",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết một TourSlot theo ID
        /// </summary>
        /// <param name="id">ID của TourSlot</param>
        /// <returns>Chi tiết TourSlot</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSlotById(Guid id)
        {
            try
            {
                var slot = await _tourSlotService.GetSlotByIdAsync(id);
                
                if (slot == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tour slot"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy chi tiết tour slot thành công",
                    data = slot
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour slot by ID: {SlotId}", id);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy chi tiết tour slot",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy TourSlots của một TourDetails cụ thể
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Danh sách TourSlots của TourDetails</returns>
        [HttpGet("tour-details/{tourDetailsId}")]
        public async Task<IActionResult> GetSlotsByTourDetails(Guid tourDetailsId)
        {
            try
            {
                var slots = await _tourSlotService.GetSlotsByTourDetailsAsync(tourDetailsId);

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách tour slots của tour details thành công",
                    data = slots,
                    totalCount = slots.Count(),
                    tourDetailsId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour slots for TourDetails: {TourDetailsId}", tourDetailsId);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy danh sách tour slots",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy TourSlots của một TourTemplate cụ thể
        /// </summary>
        /// <param name="tourTemplateId">ID của TourTemplate</param>
        /// <param name="onlyUnassigned">Chỉ lấy slots chưa có tour details (default: false)</param>
        /// <param name="includeInactive">Có bao gồm slots không active không (default: false)</param>
        /// <returns>Danh sách TourSlots của TourTemplate</returns>
        [HttpGet("tour-template/{tourTemplateId}")]
        public async Task<IActionResult> GetSlotsByTourTemplate(
            Guid tourTemplateId, 
            [FromQuery] bool onlyUnassigned = false,
            [FromQuery] bool includeInactive = false)
        {
            try
            {
                IEnumerable<TourSlotDto> slots;
                
                if (onlyUnassigned)
                {
                    // Chỉ lấy slots chưa có tour details
                    slots = await _tourSlotService.GetUnassignedTemplateSlotsByTemplateAsync(tourTemplateId, includeInactive);
                }
                else
                {
                    // Lấy tất cả slots của template
                    slots = await _tourSlotService.GetSlotsByTourTemplateAsync(tourTemplateId);
                    
                    if (!includeInactive)
                    {
                        slots = slots.Where(s => s.IsActive);
                    }
                }

                return Ok(new
                {
                    success = true,
                    message = onlyUnassigned 
                        ? "Lấy danh sách tour slots chưa có tour details thành công"
                        : "Lấy danh sách tour slots của tour template thành công",
                    data = slots,
                    totalCount = slots.Count(),
                    tourTemplateId,
                    filters = new { onlyUnassigned, includeInactive }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting tour slots for TourTemplate: {TourTemplateId}", tourTemplateId);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy danh sách tour slots",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy các slots template chưa được assign tour details (slots gốc được tạo từ template)
        /// Endpoint này được sử dụng trong UI để hiển thị các slot ban đầu của template
        /// </summary>
        /// <param name="tourTemplateId">ID của TourTemplate</param>
        /// <param name="includeInactive">Có bao gồm slots không active không (default: false)</param>
        /// <returns>Danh sách slots chưa có tour details</returns>
        [HttpGet("tour-template/{tourTemplateId}/unassigned")]
        public async Task<IActionResult> GetUnassignedTemplateSlots(Guid tourTemplateId, [FromQuery] bool includeInactive = false)
        {
            try
            {
                var slots = await _tourSlotService.GetUnassignedTemplateSlotsByTemplateAsync(tourTemplateId, includeInactive);

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách tour slots chưa có tour details thành công",
                    data = slots,
                    totalCount = slots.Count(),
                    tourTemplateId,
                    description = "Danh sách các slot gốc được tạo từ template (chưa có tour details assign)"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unassigned template slots for TourTemplate: {TourTemplateId}", tourTemplateId);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy danh sách slots chưa có tour details",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Lấy thông tin capacity chi tiết của một slot
        /// </summary>
        /// <param name="id">ID của TourSlot</param>
        /// <returns>Thông tin debug capacity</returns>
        [HttpGet("{id}/debug-capacity")]
        public async Task<IActionResult> GetSlotCapacityDebugInfo(Guid id)
        {
            try
            {
                var (isValid, debugInfo) = await _tourSlotService.GetSlotCapacityDebugInfoAsync(id);

                return Ok(new
                {
                    success = true,
                    message = "Lấy thông tin debug capacity thành công",
                    data = new
                    {
                        slotId = id,
                        isValid,
                        debugInfo,
                        timestamp = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting debug capacity info for slot: {SlotId}", id);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thông tin debug",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Kiểm tra xem có thể booking một slot với số lượng khách yêu cầu không
        /// </summary>
        /// <param name="id">ID của TourSlot</param>
        /// <param name="requestedGuests">Số lượng khách muốn booking</param>
        /// <returns>Kết quả kiểm tra</returns>
        [HttpGet("{id}/can-book")]
        public async Task<IActionResult> CanBookSlot(Guid id, [FromQuery] int requestedGuests = 1)
        {
            try
            {
                if (requestedGuests <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Số lượng khách phải lớn hơn 0"
                    });
                }

                var canBook = await _tourSlotService.CanBookSlotAsync(id, requestedGuests);

                return Ok(new
                {
                    success = true,
                    message = "Kiểm tra khả năng booking thành công",
                    data = new
                    {
                        slotId = id,
                        requestedGuests,
                        canBook,
                        timestamp = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if slot can be booked: {SlotId}, Guests: {Guests}", id, requestedGuests);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi kiểm tra khả năng booking",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết slot với thông tin tour và danh sách user đã book
        /// </summary>
        /// <param name="id">ID của TourSlot</param>
        /// <returns>Chi tiết slot với thông tin tour và danh sách user đã book</returns>
        [HttpGet("{id}/tour-details-and-bookings")]
        public async Task<IActionResult> GetSlotWithTourDetailsAndBookings(Guid id)
        {
            try
            {
                var result = await _tourSlotService.GetSlotWithTourDetailsAndBookingsAsync(id);
                
                if (result == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tour slot"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Lấy chi tiết slot với thông tin tour và bookings thành công",
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slot with tour details and bookings: {SlotId}", id);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy chi tiết slot",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Hủy tour slot công khai và gửi thông báo cho khách hàng
        /// </summary>
        /// <param name="slotId">ID của slot cần hủy</param>
        /// <param name="request">Thông tin lý do hủy tour</param>
        /// <returns>Kết quả hủy tour</returns>
        [HttpPost("{slotId}/cancel-public")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<IActionResult> CancelPublicTourSlot(Guid slotId, [FromBody] CancelPublicTourSlotDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                    });
                }

                // Get current user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("Id")?.Value;
                if (!Guid.TryParse(userIdClaim, out var tourCompanyUserId))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Không thể xác thực người dùng"
                    });
                }

                var (success, message, customersNotified) = await _tourSlotService.CancelPublicTourSlotAsync(
                    slotId, request.Reason, tourCompanyUserId);

                if (!success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message
                    });
                }

                return Ok(new CancelTourSlotResultDto
                {
                    Success = true,
                    Message = message,
                    CustomersNotified = customersNotified,
                    AffectedBookings = 0, // Will be updated if needed
                    TotalRefundAmount = 0, // Will be updated if needed
                    AffectedCustomers = new List<AffectedCustomerInfo>() // Will be updated if needed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling public tour slot: {SlotId}", slotId);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi hủy tour",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// TEST ONLY - Hủy tour slot với debug mode và detailed logging
        /// Endpoint này sẽ skip email nếu có lỗi và vẫn hoàn thành việc hủy
        /// </summary>
        /// <param name="slotId">ID của slot cần hủy</param>
        /// <param name="request">Thông tin lý do hủy tour</param>
        /// <returns>Kết quả hủy tour với debug info</returns>
        [HttpPost("{slotId}/cancel-public-debug")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<IActionResult> CancelPublicTourSlotDebug(Guid slotId, [FromBody] CancelPublicTourSlotDto request)
        {
            try
            {
                _logger.LogInformation("=== DEBUG CANCEL REQUEST RECEIVED ===");
                _logger.LogInformation("SlotId: {SlotId}, Reason: {Reason}", slotId, request?.Reason);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState is invalid: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    
                    return BadRequest(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ",
                        errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage),
                        debug = new
                        {
                            receivedRequest = request,
                            modelStateValid = false,
                            timestamp = DateTime.UtcNow
                        }
                    });
                }

                // Get current user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("Id")?.Value;
                _logger.LogInformation("User claims: NameIdentifier={NameId}, Id={Id}", 
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    User.FindFirst("Id")?.Value);

                if (!Guid.TryParse(userIdClaim, out var tourCompanyUserId))
                {
                    _logger.LogError("Cannot parse user ID from claims: {UserIdClaim}", userIdClaim);
                    
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Không thể xác thực người dùng",
                        debug = new
                        {
                            userIdClaim,
                            claimsCount = User.Claims.Count(),
                            allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray(),
                            timestamp = DateTime.UtcNow
                        }
                    });
                }

                _logger.LogInformation("Authenticated user: {UserId}", tourCompanyUserId);

                // Call service with detailed logging
                var (success, message, customersNotified) = await _tourSlotService.CancelPublicTourSlotAsync(
                    slotId, request.Reason, tourCompanyUserId);

                _logger.LogInformation("Service call completed - Success: {Success}, Message: {Message}, CustomersNotified: {CustomersNotified}", 
                    success, message, customersNotified);

                if (!success)
                {
                    _logger.LogWarning("Service returned failure: {Message}", message);
                    
                    return BadRequest(new
                    {
                        success = false,
                        message,
                        debug = new
                        {
                            slotId,
                            userId = tourCompanyUserId,
                            reason = request.Reason,
                            serviceSuccess = success,
                            timestamp = DateTime.UtcNow
                        }
                    });
                }

                _logger.LogInformation("Cancel operation successful");

                return Ok(new CancelTourSlotResultDto
                {
                    Success = true,
                    Message = message,
                    CustomersNotified = customersNotified,
                    AffectedBookings = 0, // TODO: Update if needed
                    TotalRefundAmount = 0, // TODO: Update if needed
                    AffectedCustomers = new List<AffectedCustomerInfo>() // TODO: Update if needed
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("=== DEBUG CANCEL CONTROLLER EXCEPTION ===");
                _logger.LogError(ex, "Controller exception in debug cancel - SlotId: {SlotId}", slotId);
                _logger.LogError("Exception details: Type={Type}, Message={Message}", ex.GetType().Name, ex.Message);
                _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi hủy tour",
                    error = ex.Message,
                    debug = new
                    {
                        slotId,
                        exceptionType = ex.GetType().Name,
                        innerException = ex.InnerException?.Message,
                        stackTrace = ex.StackTrace,
                        timestamp = DateTime.UtcNow
                    }
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Sync slots capacity với TourOperation MaxGuests
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Kết quả sync</returns>
        [HttpPost("sync-capacity/{tourDetailsId}")]
        public async Task<IActionResult> SyncSlotsCapacity(Guid tourDetailsId)
        {
            try
            {
                // Get TourOperation MaxGuests
                var tourOperation = await _unitOfWork.TourOperationRepository.GetQueryable()
                    .FirstOrDefaultAsync(to => to.TourDetailsId == tourDetailsId);

                if (tourOperation == null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Không tìm thấy TourOperation cho TourDetails này"
                    });
                }

                // Sync slots capacity
                var result = await _tourSlotService.SyncSlotsCapacityAsync(tourDetailsId, tourOperation.MaxGuests);

                return Ok(new
                {
                    success = result,
                    message = result ? "Sync slots capacity thành công" : "Sync slots capacity thất bại",
                    data = new
                    {
                        tourDetailsId,
                        maxGuests = tourOperation.MaxGuests,
                        timestamp = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing slots capacity for TourDetails {TourDetailsId}", tourDetailsId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi sync slots capacity",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Kiểm tra chi tiết slot và thông tin để debug lỗi cancel
        /// </summary>
        /// <param name="slotId">ID của slot cần kiểm tra</param>
        /// <returns>Thông tin debug chi tiết</returns>
        [HttpGet("{slotId}/debug-cancel-info")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<IActionResult> GetSlotDebugCancelInfo(Guid slotId)
        {
            try
            {
                _logger.LogInformation("Debug cancel info requested for slot {SlotId}", slotId);

                // Get current user ID
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("Id")?.Value;
                if (!Guid.TryParse(userIdClaim, out var tourCompanyUserId))
                {
                    return Unauthorized(new
                    {
                        success = false,
                        message = "Không thể xác thực người dùng"
                    });
                }

                // Get slot with all related data (same as cancel method)
                var slot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(s => s.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .Include(s => s.TourTemplate)
                    .Include(s => s.Bookings.Where(b => !b.IsDeleted && 
                        (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)))
                        .ThenInclude(b => b.User)
                    .FirstOrDefaultAsync(s => s.Id == slotId && !s.IsDeleted);

                if (slot == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tour slot",
                        debug = new
                        {
                            slotId,
                            slotFound = false,
                            timestamp = DateTime.UtcNow
                        }
                    });
                }

                var affectedBookings = slot.Bookings.Where(b => !b.IsDeleted && 
                    (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)).ToList();

                var debugInfo = new
                {
                    slot = new
                    {
                        id = slot.Id,
                        isActive = slot.IsActive,
                        status = slot.Status,
                        statusName = slot.Status.ToString(),
                        tourDate = slot.TourDate,
                        tourDetailsId = slot.TourDetailsId,
                        isDeleted = slot.IsDeleted,
                        createdById = slot.CreatedById,
                        updatedById = slot.UpdatedById,
                        createdAt = slot.CreatedAt,
                        updatedAt = slot.UpdatedAt
                    },
                    tourDetails = slot.TourDetails != null ? new
                    {
                        id = slot.TourDetails.Id,
                        title = slot.TourDetails.Title,
                        status = slot.TourDetails.Status,
                        statusName = slot.TourDetails.Status.ToString(),
                        createdById = slot.TourDetails.CreatedById,
                        updatedById = slot.TourDetails.UpdatedById,
                        createdAt = slot.TourDetails.CreatedAt,
                        updatedAt = slot.TourDetails.UpdatedAt,
                        isDeleted = slot.TourDetails.IsDeleted
                    } : null,
                    tourOperation = slot.TourDetails?.TourOperation != null ? new
                    {
                        id = slot.TourDetails.TourOperation.Id,
                        maxGuests = slot.TourDetails.TourOperation.MaxGuests,
                        currentBookings = slot.TourDetails.TourOperation.CurrentBookings,
                        price = slot.TourDetails.TourOperation.Price,
                        isActive = slot.TourDetails.TourOperation.IsActive,
                        status = slot.TourDetails.TourOperation.Status
                    } : null,
                    currentUser = new
                    {
                        userId = tourCompanyUserId,
                        userIdClaim,
                        canCancel = slot.TourDetails?.CreatedById == tourCompanyUserId
                    },
                    bookings = new
                    {
                        totalBookings = slot.Bookings.Count,
                        affectedBookings = affectedBookings.Count,
                        bookingDetails = affectedBookings.Select(b => new
                        {
                            id = b.Id,
                            bookingCode = b.BookingCode,
                            status = b.Status,
                            statusName = b.Status.ToString(),
                            numberOfGuests = b.NumberOfGuests,
                            totalPrice = b.TotalPrice,
                            contactName = b.ContactName,
                            contactEmail = b.ContactEmail,
                            contactPhone = b.ContactPhone,
                            userId = b.UserId,
                            userName = b.User?.Name,
                            userEmail = b.User?.Email,
                            isDeleted = b.IsDeleted,
                            createdAt = b.BookingDate
                        }).ToList()
                    },
                    validationChecks = new
                    {
                        slotExists = slot != null,
                        hasTourDetails = slot.TourDetailsId != null,
                        tourDetailsLoaded = slot.TourDetails != null,
                        tourDetailsIsPublic = slot.TourDetails?.Status == TourDetailsStatus.Public,
                        userOwnsDetails = slot.TourDetails?.CreatedById == tourCompanyUserId,
                        slotIsActive = slot.IsActive,
                        canCancel = slot != null &&
                                   slot.TourDetailsId != null &&
                                   slot.TourDetails != null &&
                                   slot.TourDetails.Status == TourDetailsStatus.Public &&
                                   slot.TourDetails.CreatedById == tourCompanyUserId &&
                                   slot.IsActive
                    },
                    timestamp = DateTime.UtcNow
                };

                return Ok(new
                {
                    success = true,
                    message = "Thông tin debug lấy thành công",
                    data = debugInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting debug cancel info for slot: {SlotId}", slotId);
                
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thông tin debug",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}
