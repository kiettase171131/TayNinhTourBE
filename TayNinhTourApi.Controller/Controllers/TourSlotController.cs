using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;

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
        /// <returns>Danh sách TourSlots của TourTemplate</returns>
        [HttpGet("tour-template/{tourTemplateId}")]
        public async Task<IActionResult> GetSlotsByTourTemplate(Guid tourTemplateId)
        {
            try
            {
                var slots = await _tourSlotService.GetSlotsByTourTemplateAsync(tourTemplateId);

                return Ok(new
                {
                    success = true,
                    message = "Lấy danh sách tour slots của tour template thành công",
                    data = slots,
                    totalCount = slots.Count(),
                    tourTemplateId
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
    }
}
