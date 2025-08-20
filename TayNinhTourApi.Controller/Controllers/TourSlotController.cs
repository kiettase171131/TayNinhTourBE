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
        /// Lấy TourSlots của một TourDetails cụ thể
        /// </summary>
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
        /// ✅ NEW: Fix status inconsistency cho TourSlots
        /// Fixes slots có availableSpots > 0 nhưng status = FullyBooked
        /// </summary>
        [HttpPost("fix-status-inconsistency/{tourDetailsId}")]
        public async Task<IActionResult> FixSlotStatusInconsistency(Guid tourDetailsId)
        {
            try
            {
                var slots = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Where(s => s.TourDetailsId == tourDetailsId && !s.IsDeleted)
                    .ToListAsync();

                if (!slots.Any())
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy slots cho TourDetails này"
                    });
                }

                int fixedCount = 0;
                var fixedSlots = new List<object>();

                foreach (var slot in slots)
                {
                    var availableSpots = slot.MaxGuests - slot.CurrentBookings;
                    var currentStatus = slot.Status;
                    var shouldBeStatus = availableSpots > 0 ? TourSlotStatus.Available : TourSlotStatus.FullyBooked;

                    // Fix inconsistent status
                    if (currentStatus != shouldBeStatus)
                    {
                        slot.Status = shouldBeStatus;
                        slot.UpdatedAt = DateTime.UtcNow;

                        await _unitOfWork.TourSlotRepository.UpdateAsync(slot);
                        fixedCount++;

                        fixedSlots.Add(new
                        {
                            slotId = slot.Id,
                            tourDate = slot.TourDate,
                            maxGuests = slot.MaxGuests,
                            currentBookings = slot.CurrentBookings,
                            availableSpots = availableSpots,
                            oldStatus = currentStatus.ToString(),
                            newStatus = shouldBeStatus.ToString(),
                            isFixed = true
                        });

                        _logger.LogInformation("Fixed status for slot {SlotId} ({TourDate}): {OldStatus} -> {NewStatus} (AvailableSpots: {AvailableSpots})",
                            slot.Id, slot.TourDate, currentStatus, shouldBeStatus, availableSpots);
                    }
                    else
                    {
                        fixedSlots.Add(new
                        {
                            slotId = slot.Id,
                            tourDate = slot.TourDate,
                            maxGuests = slot.MaxGuests,
                            currentBookings = slot.CurrentBookings,
                            availableSpots = availableSpots,
                            status = currentStatus.ToString(),
                            isFixed = false,
                            reason = "Status already correct"
                        });
                    }
                }

                if (fixedCount > 0)
                {
                    await _unitOfWork.SaveChangesAsync();
                }

                return Ok(new
                {
                    success = true,
                    message = $"Đã fix {fixedCount}/{slots.Count} slots với status inconsistency",
                    data = new
                    {
                        tourDetailsId,
                        totalSlots = slots.Count,
                        fixedSlots = fixedCount,
                        unfixedSlots = slots.Count - fixedCount,
                        details = fixedSlots,
                        timestamp = DateTime.UtcNow
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fixing slot status inconsistency for TourDetails {TourDetailsId}", tourDetailsId);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi fix slot status",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết slot với thông tin tour và danh sách bookings
        /// </summary>
        [HttpGet("{id}/tour-details-and-bookings")]
        public async Task<IActionResult> GetSlotWithTourDetailsAndBookings(Guid id)
        {
            try
            {
                var slot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(s => s.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                            .ThenInclude(to => to!.TourGuide)
                    .Include(s => s.TourDetails)
                        .ThenInclude(td => td!.Timeline.Where(ti => !ti.IsDeleted))
                            .ThenInclude(ti => ti.SpecialtyShop)
                    .Include(s => s.Bookings.Where(b => !b.IsDeleted))
                        .ThenInclude(b => b.User)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

                if (slot == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy tour slot",
                        statusCode = 404
                    });
                }

                var result = new
                {
                    success = true,
                    message = "Lấy thông tin tour slot thành công",
                    statusCode = 200,
                    data = new
                    {
                        slot = new
                        {
                            id = slot.Id,
                            tourDate = slot.TourDate,
                            scheduleDay = slot.ScheduleDay,
                            status = slot.Status,
                            statusName = slot.Status.ToString(),
                            isActive = slot.IsActive,
                            createdAt = slot.CreatedAt,
                            updatedAt = slot.UpdatedAt
                        },
                        tourDetails = slot.TourDetails == null ? null : new
                        {
                            id = slot.TourDetails.Id,
                            title = slot.TourDetails.Title,
                            description = slot.TourDetails.Description,
                            status = slot.TourDetails.Status,
                            timeline = slot.TourDetails.Timeline?.Select(ti => new
                            {
                                id = ti.Id,
                                checkInTime = ti.CheckInTime,
                                activity = ti.Activity,
                                sortOrder = ti.SortOrder,
                                specialtyShop = ti.SpecialtyShop == null ? null : new
                                {
                                    id = ti.SpecialtyShop.Id,
                                    name = ti.SpecialtyShop.ShopName,
                                    location = ti.SpecialtyShop.Location,
                                    phoneNumber = ti.SpecialtyShop.PhoneNumber
                                }
                            }).OrderBy(ti => ti.sortOrder).ToList()
                        },
                        tourOperation = slot.TourDetails?.TourOperation == null ? null : new
                        {
                            id = slot.TourDetails.TourOperation.Id,
                            price = slot.TourDetails.TourOperation.Price,
                            maxGuests = slot.TourDetails.TourOperation.MaxGuests,
                            currentBookings = slot.TourDetails.TourOperation.CurrentBookings,
                            status = slot.TourDetails.TourOperation.Status,
                            isActive = slot.TourDetails.TourOperation.IsActive,
                            tourGuide = slot.TourDetails.TourOperation.TourGuide == null ? null : new
                            {
                                id = slot.TourDetails.TourOperation.TourGuide.Id,
                                fullName = slot.TourDetails.TourOperation.TourGuide.FullName,
                                phoneNumber = slot.TourDetails.TourOperation.TourGuide.PhoneNumber,
                                email = slot.TourDetails.TourOperation.TourGuide.Email
                            }
                        },
                        bookings = slot.Bookings?.Select(b => new
                        {
                            id = b.Id,
                            bookingCode = b.BookingCode,
                            numberOfGuests = b.NumberOfGuests,
                            totalPrice = b.TotalPrice,
                            status = b.Status,
                            contactName = b.ContactName,
                            contactPhone = b.ContactPhone,
                            contactEmail = b.ContactEmail,
                            createdAt = b.CreatedAt,
                            user = b.User == null ? null : new
                            {
                                id = b.User.Id,
                                name = b.User.Name,
                                email = b.User.Email,
                                phoneNumber = b.User.PhoneNumber
                            }
                        }).ToList()
                    }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting slot with tour details and bookings: {SlotId}", id);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi lấy thông tin tour slot",
                    statusCode = 500,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Debug endpoint - Get detailed capacity info for troubleshooting slot availability issues
        /// </summary>
        [HttpGet("{id}/debug-capacity-detailed")]
        public async Task<IActionResult> GetDetailedCapacityDebug(Guid id)
        {
            try
            {
                var slot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(s => s.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

                if (slot == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Slot not found",
                        slotId = id
                    });
                }

                // Real-time booking count calculation from database
                var realTimeBookings = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.TourSlotId == id &&
                               !b.IsDeleted &&
                               (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending))
                    .SumAsync(b => b.NumberOfGuests);

                var debugData = new
                {
                    slotId = id,
                    timestamp = DateTime.UtcNow,

                    // TourSlot capacity data (independent per slot)
                    tourSlot = new
                    {
                        maxGuests = slot.MaxGuests,
                        currentBookings = slot.CurrentBookings,
                        availableSpots = slot.AvailableSpots,
                        isActive = slot.IsActive,
                        status = slot.Status,
                        statusName = slot.Status.ToString(),
                        tourDate = slot.TourDate,
                        isInFuture = slot.TourDate > DateOnly.FromDateTime(DateTime.UtcNow),
                        isBookable = slot.IsActive &&
                                    slot.Status == TourSlotStatus.Available &&
                                    slot.TourDate > DateOnly.FromDateTime(DateTime.UtcNow) &&
                                    slot.AvailableSpots > 0
                    },

                    // Real-time calculation verification
                    realTimeData = new
                    {
                        actualBookingsFromDB = realTimeBookings,
                        slotBookingDiscrepancy = Math.Abs(realTimeBookings - slot.CurrentBookings),
                        discrepancyDetected = Math.Abs(realTimeBookings - slot.CurrentBookings) > 0,
                        realTimeAvailableSpots = slot.MaxGuests - realTimeBookings
                    },

                    // Status inconsistency issues
                    statusIssues = new
                    {
                        hasAvailableSpots = slot.AvailableSpots > 0,
                        statusIsFullyBooked = slot.Status == TourSlotStatus.FullyBooked,
                        statusInconsistency = slot.AvailableSpots > 0 && slot.Status == TourSlotStatus.FullyBooked,
                        shouldBeStatus = slot.AvailableSpots > 0 ? "Available" : "FullyBooked",
                        needsStatusFix = slot.AvailableSpots > 0 && slot.Status == TourSlotStatus.FullyBooked
                    }
                };

                return Ok(new
                {
                    success = true,
                    message = "Debug capacity info retrieved successfully",
                    data = debugData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed capacity debug for slot: {SlotId}", id);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error getting debug info",
                    error = ex.Message,
                    slotId = id
                });
            }
        }
    }
}
