    using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Booking;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourFeedback;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Booking;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourFeedback;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller quản lý TourBooking - đặt tour và quản lý booking
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class TourBookingController : ControllerBase
    {
        private readonly ITourBookingService _tourBookingService;
        private readonly ITourFeedbackService _tourFeedback;
        private readonly ILogger<TourBookingController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public TourBookingController(
            ITourBookingService tourBookingService,
            ILogger<TourBookingController> logger,
            ITourFeedbackService tourFeedback,
            IUnitOfWork unitOfWork)
        {
            _tourBookingService = tourBookingService;
            _logger = logger;
            _tourFeedback = tourFeedback;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Tạo booking mới
        /// </summary>
        /// <param name="request">Thông tin booking</param>
        /// <returns>Kết quả tạo booking</returns>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseCreateBookingDto>> CreateBooking([FromBody] RequestCreateBookingDto request)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var response = await _tourBookingService.CreateBookingAsync(request, currentUser);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, new ResponseCreateBookingDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi tạo booking",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Lấy danh sách bookings của user hiện tại
        /// </summary>
        /// <param name="includeInactive">Có bao gồm booking đã hủy không</param>
        /// <returns>Danh sách bookings</returns>
        [HttpGet("my-bookings")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseGetBookingsDto>> GetMyBookings([FromQuery] bool includeInactive = false)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var response = await _tourBookingService.GetMyBookingsAsync(currentUser, includeInactive);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user bookings");
                return StatusCode(500, new ResponseGetBookingsDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách booking",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Lấy danh sách bookings với filter (Admin/TourCompany)
        /// </summary>
        /// <param name="request">Filter parameters</param>
        /// <returns>Danh sách bookings với pagination</returns>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Tour Company")]
        public async Task<ActionResult<ResponseGetBookingsDto>> GetBookings([FromQuery] RequestGetBookingsDto request)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var response = await _tourBookingService.GetBookingsAsync(request, currentUser);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting bookings");
                return StatusCode(500, new ResponseGetBookingsDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi lấy danh sách booking",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết booking theo ID
        /// </summary>
        /// <param name="id">ID của booking</param>
        /// <returns>Chi tiết booking</returns>
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseBookingDto>> GetBookingById(Guid id)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var booking = await _tourBookingService.GetBookingByIdAsync(id, currentUser);

                if (booking == null)
                {
                    return NotFound(new { message = "Booking không tồn tại hoặc bạn không có quyền xem" });
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking {BookingId}", id);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thông tin booking" });
            }
        }

        /// <summary>
        /// Lấy booking theo booking code (public endpoint)
        /// </summary>
        /// <param name="code">Mã booking</param>
        /// <returns>Chi tiết booking</returns>
        [HttpGet("code/{code}")]
        public async Task<ActionResult<ResponseBookingDto>> GetBookingByCode(string code)
        {
            try
            {
                var booking = await _tourBookingService.GetBookingByCodeAsync(code);

                if (booking == null)
                {
                    return NotFound(new { message = "Không tìm thấy booking với mã này" });
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking by code {BookingCode}", code);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi tra cứu booking" });
            }
        }

        /// <summary>
        /// Hủy booking
        /// </summary>
        /// <param name="id">ID của booking</param>
        /// <param name="request">Thông tin hủy booking</param>
        /// <returns>Kết quả hủy booking</returns>
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<ResponseCreateBookingDto>> CancelBooking(Guid id, [FromBody] RequestUpdateBookingStatusDto request)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var response = await _tourBookingService.CancelBookingAsync(id, request, currentUser);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", id);
                return StatusCode(500, new ResponseCreateBookingDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi hủy booking",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Kiểm tra capacity cho TourOperation (LEGACY - shared capacity across all slots)
        /// ⚠️ WARNING: This checks total operation capacity, not individual slot capacity
        /// </summary>
        /// <param name="operationId">ID của TourOperation</param>
        /// <param name="requestedGuests">Số khách muốn booking (optional)</param>
        /// <returns>Thông tin capacity</returns>
        [HttpGet("operation/{operationId}/capacity")]
        public async Task<ActionResult<ResponseCapacityCheckDto>> CheckCapacity(Guid operationId, [FromQuery] int? requestedGuests = null)
        {
            try
            {
                var response = await _tourBookingService.CheckCapacityAsync(operationId, requestedGuests);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking capacity for operation {OperationId}", operationId);
                return StatusCode(500, new ResponseCapacityCheckDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi kiểm tra capacity",
                    StatusCode = 500,
                    TourOperationId = operationId
                });
            }
        }

        /// <summary>
        /// ✅ NEW: Kiểm tra capacity cho TourSlot cụ thể (individual slot capacity)
        /// Use this for slot-specific booking validation
        /// </summary>
        /// <param name="slotId">ID của TourSlot</param>
        /// <param name="requestedGuests">Số khách muốn booking</param>
        /// <returns>Thông tin capacity của slot cụ thể</returns>
        [HttpGet("slot/{slotId}/capacity")]
        public async Task<ActionResult> CheckSlotCapacity(Guid slotId, [FromQuery] int requestedGuests = 1)
        {
            try
            {
                if (requestedGuests <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Số lượng khách phải lớn hơn 0",
                        statusCode = 400
                    });
                }

                // Get slot with current capacity
                var slot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Where(s => s.Id == slotId && !s.IsDeleted)
                    .Select(s => new 
                    {
                        s.Id,
                        s.MaxGuests,
                        s.CurrentBookings,
                        s.IsActive,
                        s.Status,
                        s.TourDate
                    })
                    .FirstOrDefaultAsync();

                if (slot == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Tour slot không tồn tại",
                        statusCode = 404
                    });
                }

                var availableSpots = slot.MaxGuests - slot.CurrentBookings;
                var canBook = slot.IsActive && 
                             slot.Status == TourSlotStatus.Available &&
                             slot.TourDate > DateOnly.FromDateTime(DateTime.UtcNow) &&
                             availableSpots >= requestedGuests;

                var capacityInfo = new
                {
                    slotId = slotId,
                    maxCapacity = slot.MaxGuests,
                    bookedCapacity = slot.CurrentBookings,
                    availableCapacity = availableSpots,
                    bookingPercentage = slot.MaxGuests > 0 ? Math.Round((decimal)slot.CurrentBookings / slot.MaxGuests * 100, 2) : 0,
                    isFull = availableSpots <= 0
                };

                var userMessage = canBook 
                    ? $"Slot này còn {availableSpots} chỗ trống"
                    : availableSpots <= 0 
                        ? "Slot này đã hết chỗ"
                        : !slot.IsActive 
                            ? "Slot này không còn hoạt động"
                            : slot.TourDate <= DateOnly.FromDateTime(DateTime.UtcNow)
                                ? "Slot này đã qua ngày"
                                : $"Slot này chỉ còn {availableSpots} chỗ, không đủ cho {requestedGuests} khách";

                return Ok(new
                {
                    success = true,
                    message = "Kiểm tra capacity slot thành công",
                    statusCode = 200,
                    slotId = slotId,
                    capacityInfo = capacityInfo,
                    canBook = canBook,
                    maxAdditionalGuests = Math.Max(0, availableSpots),
                    userMessage = userMessage,
                    checkedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking slot capacity: {SlotId}", slotId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi kiểm tra capacity slot",
                    statusCode = 500,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Confirm booking (Admin/TourCompany only)
        /// </summary>
        /// <param name="id">ID của booking</param>
        /// <returns>Kết quả confirm booking</returns>
        [HttpPost("{id}/confirm")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Tour Company")]
        public async Task<ActionResult<ResponseCreateBookingDto>> ConfirmBooking(Guid id)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var response = await _tourBookingService.ConfirmBookingAsync(id, currentUser);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming booking {BookingId}", id);
                return StatusCode(500, new ResponseCreateBookingDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi confirm booking",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Cập nhật trạng thái booking (Admin/TourCompany only)
        /// </summary>
        /// <param name="id">ID của booking</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPatch("{id}/status")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Tour Company")]
        public async Task<ActionResult<ResponseCreateBookingDto>> UpdateBookingStatus(Guid id, [FromBody] RequestUpdateBookingStatusDto request)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var response = await _tourBookingService.UpdateBookingStatusAsync(id, request, currentUser);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking status {BookingId}", id);
                return StatusCode(500, new ResponseCreateBookingDto
                {
                    success = false,
                    Message = "Có lỗi xảy ra khi cập nhật trạng thái booking",
                    StatusCode = 500
                });
            }
        }

        /// <summary>
        /// Lấy thống kê bookings cho TourOperation (Admin/TourCompany only)
        /// </summary>
        /// <param name="operationId">ID của TourOperation</param>
        /// <returns>Thống kê booking</returns>
        [HttpGet("operation/{operationId}/statistics")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Tour Company")]
        public async Task<ActionResult<BookingSummaryDto>> GetBookingStatistics(Guid operationId)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var statistics = await _tourBookingService.GetBookingStatisticsAsync(operationId, currentUser);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking statistics for operation {OperationId}", operationId);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lấy thống kê booking" });
            }
        }
        [HttpGet("Feedback-by-slot/{slotId:guid}")]
        public async Task<ActionResult<TourFeedbackResponse>> GetBySlot(Guid slotId, int? pageIndex, int? pageSize, int? minTourRating, int? maxTourRating, bool? onlyWithGuideRating)
        {
            var res = await _tourFeedback.GetTourFeedbacksBySlotAsync(slotId, pageIndex, pageSize, minTourRating, maxTourRating, onlyWithGuideRating);
            return StatusCode(res.StatusCode, res);
        }
        [HttpPost("Feedback-Tour")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<TourFeedbackDto>> Create([FromBody] CreateTourFeedbackRequest request)
        {
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            if (currentUser is null || currentUser.UserId == Guid.Empty) return Unauthorized();
            var res = await _tourFeedback.CreateAsync(currentUser.UserId, request);
            return StatusCode(res.StatusCode, res);
        }
    }
}
