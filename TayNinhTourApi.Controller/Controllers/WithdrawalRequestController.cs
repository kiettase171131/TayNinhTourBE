using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.WithdrawalRequest;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho quản lý yêu cầu rút tiền của user
    /// Cung cấp APIs cho user tạo và theo dõi withdrawal requests
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class WithdrawalRequestController : ControllerBase
    {
        private readonly IWithdrawalRequestService _withdrawalRequestService;
        private readonly ILogger<WithdrawalRequestController> _logger;

        public WithdrawalRequestController(
            IWithdrawalRequestService withdrawalRequestService, 
            ILogger<WithdrawalRequestController> logger)
        {
            _withdrawalRequestService = withdrawalRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo yêu cầu rút tiền mới
        /// </summary>
        /// <param name="createDto">Thông tin yêu cầu rút tiền</param>
        /// <returns>Thông tin yêu cầu rút tiền vừa tạo</returns>
        [HttpPost]
        public async Task<IActionResult> CreateWithdrawalRequest([FromBody] CreateWithdrawalRequestDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Creating withdrawal request for user: {UserId}, Amount: {Amount}", 
                    userId, createDto.Amount);

                var result = await _withdrawalRequestService.CreateRequestAsync(createDto, userId);
                
                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetWithdrawalRequestById), 
                        new { id = result.Data.Id }, result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating withdrawal request for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi tạo yêu cầu rút tiền");
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền của user hiện tại
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <param name="pageNumber">Số trang (default: 1)</param>
        /// <param name="pageSize">Kích thước trang (default: 10)</param>
        /// <returns>Danh sách yêu cầu rút tiền với pagination</returns>
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyWithdrawalRequests(
            [FromQuery] WithdrawalStatus? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Getting withdrawal requests for user: {UserId}, Status: {Status}, Page: {PageNumber}", 
                    userId, status, pageNumber);

                var result = await _withdrawalRequestService.GetByUserIdAsync(userId, status, pageNumber, pageSize);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting withdrawal requests for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi lấy danh sách yêu cầu rút tiền");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một yêu cầu rút tiền
        /// </summary>
        /// <param name="id">ID của yêu cầu rút tiền</param>
        /// <returns>Thông tin chi tiết yêu cầu rút tiền</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWithdrawalRequestById(Guid id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Getting withdrawal request {WithdrawalRequestId} for user: {UserId}", id, userId);

                var result = await _withdrawalRequestService.GetByIdAsync(id, userId);
                
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
                _logger.LogError(ex, "Error getting withdrawal request {WithdrawalRequestId}", id);
                return StatusCode(500, "Lỗi server khi lấy thông tin yêu cầu rút tiền");
            }
        }

        /// <summary>
        /// Hủy yêu cầu rút tiền (chỉ khi status = Pending)
        /// </summary>
        /// <param name="id">ID của yêu cầu rút tiền</param>
        /// <param name="request">Thông tin hủy yêu cầu</param>
        /// <returns>Kết quả hủy yêu cầu</returns>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelWithdrawalRequest(Guid id, [FromBody] CancelWithdrawalRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                _logger.LogInformation("Cancelling withdrawal request {WithdrawalRequestId} for user: {UserId}", id, userId);

                var result = await _withdrawalRequestService.CancelRequestAsync(id, userId, request.Reason);
                
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
                _logger.LogError(ex, "Error cancelling withdrawal request {WithdrawalRequestId}", id);
                return StatusCode(500, "Lỗi server khi hủy yêu cầu rút tiền");
            }
        }

        /// <summary>
        /// Kiểm tra user có thể tạo yêu cầu rút tiền mới không
        /// </summary>
        /// <returns>True nếu có thể tạo yêu cầu mới</returns>
        [HttpGet("can-create")]
        public async Task<IActionResult> CanCreateNewRequest()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                var result = await _withdrawalRequestService.CanCreateNewRequestAsync(userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking can create new request for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi kiểm tra điều kiện tạo yêu cầu");
            }
        }

        /// <summary>
        /// Lấy yêu cầu rút tiền gần nhất của user
        /// </summary>
        /// <returns>Yêu cầu rút tiền gần nhất</returns>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestRequest()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                var result = await _withdrawalRequestService.GetLatestRequestAsync(userId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest request for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi lấy yêu cầu rút tiền gần nhất");
            }
        }

        /// <summary>
        /// Validate yêu cầu rút tiền trước khi tạo
        /// </summary>
        /// <param name="request">Thông tin validation</param>
        /// <returns>Kết quả validation</returns>
        [HttpPost("validate")]
        public async Task<IActionResult> ValidateWithdrawalRequest([FromBody] ValidateWithdrawalRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                var result = await _withdrawalRequestService.ValidateWithdrawalRequestAsync(
                    userId, request.Amount, request.BankAccountId);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating withdrawal request for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi validate yêu cầu rút tiền");
            }
        }

        /// <summary>
        /// Lấy thống kê yêu cầu rút tiền của user
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu lọc (yyyy-MM-dd) - tùy chọn</param>
        /// <param name="endDate">Ngày kết thúc lọc (yyyy-MM-dd) - tùy chọn</param>
        /// <returns>Thống kê yêu cầu rút tiền</returns>
        [HttpGet("stats")]
        public async Task<IActionResult> GetMyStats(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định user");
                }

                // Validate date range
                if (startDate.HasValue && endDate.HasValue && startDate > endDate)
                {
                    return BadRequest(new { 
                        Success = false, 
                        Message = "Ngày bắt đầu không thể lớn hơn ngày kết thúc" 
                    });
                }

                var result = await _withdrawalRequestService.GetStatsForUserAsync(userId, startDate, endDate);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for user: {UserId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi lấy thống kê");
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

    /// <summary>
    /// DTO cho hủy yêu cầu rút tiền
    /// </summary>
    public class CancelWithdrawalRequestDto
    {
        /// <summary>
        /// Lý do hủy yêu cầu
        /// </summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// DTO cho validate yêu cầu rút tiền
    /// </summary>
    public class ValidateWithdrawalRequestDto
    {
        /// <summary>
        /// Số tiền muốn rút
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// ID của tài khoản ngân hàng
        /// </summary>
        public Guid BankAccountId { get; set; }
    }
}
