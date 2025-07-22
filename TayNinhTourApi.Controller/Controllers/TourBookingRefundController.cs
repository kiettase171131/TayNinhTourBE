using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBookingRefund;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho quản lý yêu cầu hoàn tiền tour booking của customer
    /// Cung cấp APIs cho customer tạo và theo dõi refund requests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TourBookingRefundController : ControllerBase
    {
        private readonly ITourBookingRefundService _tourBookingRefundService;
        private readonly ILogger<TourBookingRefundController> _logger;

        public TourBookingRefundController(
            ITourBookingRefundService tourBookingRefundService,
            ILogger<TourBookingRefundController> logger)
        {
            _tourBookingRefundService = tourBookingRefundService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo yêu cầu hoàn tiền mới (user cancellation)
        /// </summary>
        /// <param name="createDto">Thông tin yêu cầu hoàn tiền</param>
        /// <returns>Thông tin yêu cầu hoàn tiền vừa tạo</returns>
        [HttpPost]
        public async Task<IActionResult> CreateRefundRequest([FromBody] CreateTourRefundRequestDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var customerId = GetCurrentUserId();
                if (customerId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Creating refund request for customer: {CustomerId}, booking: {BookingId}",
                    customerId, createDto.TourBookingId);

                var result = await _tourBookingRefundService.CreateRefundRequestAsync(createDto, customerId);

                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetRefundRequestById),
                        new { id = result.Data!.Id }, result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund request for customer: {CustomerId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi tạo yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền của customer hiện tại
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <param name="refundType">Lọc theo loại hoàn tiền (null = tất cả)</param>
        /// <param name="pageNumber">Số trang (default: 1)</param>
        /// <param name="pageSize">Kích thước trang (default: 10)</param>
        /// <param name="fromDate">Lọc từ ngày</param>
        /// <param name="toDate">Lọc đến ngày</param>
        /// <returns>Danh sách yêu cầu hoàn tiền với pagination</returns>
        [HttpGet]
        public async Task<IActionResult> GetMyRefundRequests(
            [FromQuery] TourRefundStatus? status = null,
            [FromQuery] TourRefundType? refundType = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var customerId = GetCurrentUserId();
                if (customerId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                var filter = new CustomerRefundFilterDto
                {
                    Status = status,
                    RefundType = refundType,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    FromDate = fromDate,
                    ToDate = toDate
                };

                _logger.LogInformation("Getting refund requests for customer: {CustomerId}, Status: {Status}",
                    customerId, status);

                var result = await _tourBookingRefundService.GetCustomerRefundRequestsAsync(customerId, filter);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund requests for customer: {CustomerId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi lấy danh sách yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một yêu cầu hoàn tiền
        /// </summary>
        /// <param name="id">ID của yêu cầu hoàn tiền</param>
        /// <returns>Thông tin chi tiết yêu cầu hoàn tiền</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRefundRequestById(Guid id)
        {
            try
            {
                var customerId = GetCurrentUserId();
                if (customerId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Getting refund request {RefundRequestId} for customer: {CustomerId}",
                    id, customerId);

                var result = await _tourBookingRefundService.GetCustomerRefundRequestByIdAsync(id, customerId);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else if (result.StatusCode == 404)
                {
                    return NotFound(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund request {RefundRequestId} for customer: {CustomerId}",
                    id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi lấy thông tin yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Hủy yêu cầu hoàn tiền (chỉ khi status = Pending)
        /// </summary>
        /// <param name="id">ID của yêu cầu hoàn tiền</param>
        /// <param name="cancelDto">Thông tin hủy yêu cầu</param>
        /// <returns>Kết quả hủy yêu cầu</returns>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelRefundRequest(Guid id, [FromBody] CancelRefundRequestDto cancelDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var customerId = GetCurrentUserId();
                if (customerId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Cancelling refund request {RefundRequestId} by customer: {CustomerId}",
                    id, customerId);

                var result = await _tourBookingRefundService.CancelRefundRequestAsync(id, customerId, cancelDto);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling refund request {RefundRequestId} for customer: {CustomerId}",
                    id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi hủy yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Cập nhật thông tin ngân hàng của yêu cầu hoàn tiền
        /// </summary>
        /// <param name="id">ID của yêu cầu hoàn tiền</param>
        /// <param name="updateDto">Thông tin ngân hàng mới</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{id}/bank-info")]
        public async Task<IActionResult> UpdateRefundBankInfo(Guid id, [FromBody] UpdateRefundBankInfoDto updateDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var customerId = GetCurrentUserId();
                if (customerId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Updating bank info for refund request {RefundRequestId} by customer: {CustomerId}",
                    id, customerId);

                var result = await _tourBookingRefundService.UpdateRefundBankInfoAsync(id, customerId, updateDto);

                if (result.IsSuccess)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating bank info for refund request {RefundRequestId} for customer: {CustomerId}",
                    id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi cập nhật thông tin ngân hàng");
            }
        }

        /// <summary>
        /// Kiểm tra tour booking có đủ điều kiện hoàn tiền không
        /// </summary>
        /// <param name="tourBookingId">ID của tour booking</param>
        /// <param name="cancellationDate">Ngày dự kiến hủy (optional)</param>
        /// <returns>Kết quả kiểm tra eligibility</returns>
        [HttpGet("check-eligibility/{tourBookingId}")]
        public async Task<IActionResult> CheckRefundEligibility(
            Guid tourBookingId,
            [FromQuery] DateTime? cancellationDate = null)
        {
            try
            {
                var customerId = GetCurrentUserId();
                if (customerId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Checking refund eligibility for booking {BookingId} by customer: {CustomerId}",
                    tourBookingId, customerId);

                var result = await _tourBookingRefundService.CheckRefundEligibilityAsync(tourBookingId, cancellationDate);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking refund eligibility for booking {BookingId} for customer: {CustomerId}",
                    tourBookingId, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi kiểm tra điều kiện hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy refund request theo tour booking ID
        /// </summary>
        /// <param name="tourBookingId">ID của tour booking</param>
        /// <returns>Refund request nếu có</returns>
        [HttpGet("by-booking/{tourBookingId}")]
        public async Task<IActionResult> GetRefundRequestByBookingId(Guid tourBookingId)
        {
            try
            {
                var customerId = GetCurrentUserId();
                if (customerId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Getting refund request for booking {BookingId} by customer: {CustomerId}",
                    tourBookingId, customerId);

                var result = await _tourBookingRefundService.GetRefundRequestByBookingIdAsync(tourBookingId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund request for booking {BookingId} for customer: {CustomerId}",
                    tourBookingId, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi lấy thông tin refund request");
            }
        }

        /// <summary>
        /// Kiểm tra customer có refund request pending nào không
        /// </summary>
        /// <returns>True nếu có pending request</returns>
        [HttpGet("has-pending")]
        public async Task<IActionResult> HasPendingRefundRequest()
        {
            try
            {
                var customerId = GetCurrentUserId();
                if (customerId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                var result = await _tourBookingRefundService.HasPendingRefundRequestAsync(customerId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking pending refund requests for customer: {CustomerId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi kiểm tra pending refund requests");
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Lấy User ID từ JWT token
        /// </summary>
        /// <returns>User ID hoặc Guid.Empty nếu không tìm thấy</returns>
        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
        }

        #endregion
    }
}
