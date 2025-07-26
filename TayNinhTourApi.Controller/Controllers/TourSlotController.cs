using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot;

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

        public TourSlotController(
            ITourSlotService tourSlotService,
            ILogger<TourSlotController> logger)
        {
            _tourSlotService = tourSlotService;
            _logger = logger;
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
    }
}
