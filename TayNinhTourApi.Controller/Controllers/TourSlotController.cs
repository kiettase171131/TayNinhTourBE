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
                    return NotFound(new { 
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
