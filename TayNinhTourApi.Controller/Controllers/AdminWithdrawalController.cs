using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho admin quản lý yêu cầu rút tiền
    /// Cung cấp APIs cho admin duyệt/từ chối withdrawal requests
    /// </summary>
    [Route("api/Admin/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class WithdrawalController : ControllerBase
    {
        private readonly IWithdrawalRequestService _withdrawalRequestService;
        private readonly ILogger<WithdrawalController> _logger;

        public WithdrawalController(
            IWithdrawalRequestService withdrawalRequestService, 
            ILogger<WithdrawalController> logger)
        {
            _withdrawalRequestService = withdrawalRequestService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền cho admin
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <param name="pageNumber">Số trang (default: 1)</param>
        /// <param name="pageSize">Kích thước trang (default: 10)</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <returns>Danh sách yêu cầu rút tiền cho admin</returns>
        [HttpGet]
        public async Task<IActionResult> GetWithdrawalRequests(
            [FromQuery] WithdrawalStatus? status = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                _logger.LogInformation("Admin getting withdrawal requests - Status: {Status}, Page: {PageNumber}, Search: {SearchTerm}", 
                    status, pageNumber, searchTerm);

                var result = await _withdrawalRequestService.GetForAdminAsync(status, pageNumber, pageSize, searchTerm);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting withdrawal requests for admin");
                return StatusCode(500, "Lỗi server khi lấy danh sách yêu cầu rút tiền");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một yêu cầu rút tiền cho admin
        /// </summary>
        /// <param name="id">ID của yêu cầu rút tiền</param>
        /// <returns>Thông tin chi tiết yêu cầu rút tiền</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetWithdrawalRequestById(Guid id)
        {
            try
            {
                _logger.LogInformation("Admin getting withdrawal request {WithdrawalRequestId}", id);

                // Admin có thể xem tất cả withdrawal requests, không cần check userId
                var result = await _withdrawalRequestService.GetByIdAsync(id, Guid.Empty);
                
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
                _logger.LogError(ex, "Error getting withdrawal request {WithdrawalRequestId} for admin", id);
                return StatusCode(500, "Lỗi server khi lấy thông tin yêu cầu rút tiền");
            }
        }

        /// <summary>
        /// Admin duyệt yêu cầu rút tiền
        /// </summary>
        /// <param name="id">ID của yêu cầu rút tiền</param>
        /// <param name="request">Thông tin duyệt yêu cầu</param>
        /// <returns>Kết quả duyệt yêu cầu</returns>
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveWithdrawalRequest(Guid id, [FromBody] ApproveWithdrawalRequestDto request)
        {
            try
            {
                var adminId = GetCurrentUserId();
                if (adminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} approving withdrawal request {WithdrawalRequestId}", 
                    adminId, id);

                var result = await _withdrawalRequestService.ApproveRequestAsync(
                    id, adminId, request.AdminNotes, request.TransactionReference);
                
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
                _logger.LogError(ex, "Error approving withdrawal request {WithdrawalRequestId}", id);
                return StatusCode(500, "Lỗi server khi duyệt yêu cầu rút tiền");
            }
        }

        /// <summary>
        /// Admin từ chối yêu cầu rút tiền
        /// </summary>
        /// <param name="id">ID của yêu cầu rút tiền</param>
        /// <param name="request">Thông tin từ chối yêu cầu</param>
        /// <returns>Kết quả từ chối yêu cầu</returns>
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectWithdrawalRequest(Guid id, [FromBody] RejectWithdrawalRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Reason))
                {
                    return BadRequest("Lý do từ chối là bắt buộc");
                }

                var adminId = GetCurrentUserId();
                if (adminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} rejecting withdrawal request {WithdrawalRequestId}", 
                    adminId, id);

                var result = await _withdrawalRequestService.RejectRequestAsync(id, adminId, request.Reason);
                
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
                _logger.LogError(ex, "Error rejecting withdrawal request {WithdrawalRequestId}", id);
                return StatusCode(500, "Lỗi server khi từ chối yêu cầu rút tiền");
            }
        }

        /// <summary>
        /// Lấy thống kê tổng quan yêu cầu rút tiền cho admin
        /// </summary>
        /// <returns>Thống kê yêu cầu rút tiền</returns>
        [HttpGet("stats")]
        public async Task<IActionResult> GetWithdrawalStats()
        {
            try
            {
                _logger.LogInformation("Admin getting withdrawal stats");

                var result = await _withdrawalRequestService.GetStatsAsync();
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting withdrawal stats for admin");
                return StatusCode(500, "Lỗi server khi lấy thống kê");
            }
        }

        /// <summary>
        /// Lấy danh sách yêu cầu rút tiền đang chờ xử lý (priority cao)
        /// </summary>
        /// <param name="limit">Số lượng yêu cầu cần lấy (default: 10)</param>
        /// <returns>Danh sách yêu cầu pending với priority cao</returns>
        [HttpGet("pending-priority")]
        public async Task<IActionResult> GetPendingPriorityRequests([FromQuery] int limit = 10)
        {
            try
            {
                _logger.LogInformation("Admin getting pending priority withdrawal requests");

                var result = await _withdrawalRequestService.GetForAdminAsync(
                    WithdrawalStatus.Pending, 1, limit);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending priority requests for admin");
                return StatusCode(500, "Lỗi server khi lấy danh sách yêu cầu ưu tiên");
            }
        }

        /// <summary>
        /// Bulk approve multiple withdrawal requests
        /// </summary>
        /// <param name="request">Danh sách ID yêu cầu cần duyệt</param>
        /// <returns>Kết quả bulk approve</returns>
        [HttpPut("bulk-approve")]
        public async Task<IActionResult> BulkApproveRequests([FromBody] BulkApproveRequestDto request)
        {
            try
            {
                var adminId = GetCurrentUserId();
                if (adminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} bulk approving {Count} withdrawal requests", 
                    adminId, request.WithdrawalRequestIds.Count);

                var results = new List<BulkOperationResult>();

                foreach (var withdrawalRequestId in request.WithdrawalRequestIds)
                {
                    try
                    {
                        var result = await _withdrawalRequestService.ApproveRequestAsync(
                            withdrawalRequestId, adminId, request.AdminNotes);

                        results.Add(new BulkOperationResult
                        {
                            WithdrawalRequestId = withdrawalRequestId,
                            Success = result.IsSuccess,
                            Message = result.Message
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in bulk approve for request {WithdrawalRequestId}", withdrawalRequestId);
                        results.Add(new BulkOperationResult
                        {
                            WithdrawalRequestId = withdrawalRequestId,
                            Success = false,
                            Message = "Lỗi server khi xử lý yêu cầu"
                        });
                    }
                }

                var successCount = results.Count(r => r.Success);
                var totalCount = results.Count;

                return Ok(new
                {
                    Success = true,
                    Message = $"Đã xử lý {successCount}/{totalCount} yêu cầu thành công",
                    Results = results,
                    SuccessCount = successCount,
                    TotalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk approve withdrawal requests");
                return StatusCode(500, "Lỗi server khi duyệt hàng loạt");
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

    #region DTOs

    /// <summary>
    /// DTO cho duyệt yêu cầu rút tiền
    /// </summary>
    public class ApproveWithdrawalRequestDto
    {
        /// <summary>
        /// Ghi chú từ admin
        /// </summary>
        public string? AdminNotes { get; set; }

        /// <summary>
        /// Mã tham chiếu giao dịch
        /// </summary>
        public string? TransactionReference { get; set; }
    }

    /// <summary>
    /// DTO cho từ chối yêu cầu rút tiền
    /// </summary>
    public class RejectWithdrawalRequestDto
    {
        /// <summary>
        /// Lý do từ chối (bắt buộc)
        /// </summary>
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho bulk approve
    /// </summary>
    public class BulkApproveRequestDto
    {
        /// <summary>
        /// Danh sách ID yêu cầu rút tiền cần duyệt
        /// </summary>
        public List<Guid> WithdrawalRequestIds { get; set; } = new();

        /// <summary>
        /// Ghi chú chung cho tất cả yêu cầu
        /// </summary>
        public string? AdminNotes { get; set; }
    }

    /// <summary>
    /// Kết quả bulk operation
    /// </summary>
    public class BulkOperationResult
    {
        /// <summary>
        /// ID yêu cầu rút tiền
        /// </summary>
        public Guid WithdrawalRequestId { get; set; }

        /// <summary>
        /// Thành công hay không
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Thông báo kết quả
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
