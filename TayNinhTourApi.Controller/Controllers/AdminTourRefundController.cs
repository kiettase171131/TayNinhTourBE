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
    /// Controller cho admin quản lý yêu cầu hoàn tiền tour booking
    /// Cung cấp APIs cho admin duyệt/từ chối refund requests và thống kê
    /// </summary>
    [Route("api/Admin/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class TourRefundController : ControllerBase
    {
        private readonly ITourBookingRefundService _tourBookingRefundService;
        private readonly IRefundPolicyService _refundPolicyService;
        private readonly ILogger<TourRefundController> _logger;

        public TourRefundController(
            ITourBookingRefundService tourBookingRefundService,
            IRefundPolicyService refundPolicyService,
            ILogger<TourRefundController> logger)
        {
            _tourBookingRefundService = tourBookingRefundService;
            _refundPolicyService = refundPolicyService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền cho admin
        /// </summary>
        /// <param name="status">Lọc theo trạng thái (null = tất cả)</param>
        /// <param name="refundType">Lọc theo loại hoàn tiền (null = tất cả)</param>
        /// <param name="pageNumber">Số trang (default: 1)</param>
        /// <param name="pageSize">Kích thước trang (default: 20)</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm</param>
        /// <param name="fromDate">Lọc từ ngày</param>
        /// <param name="toDate">Lọc đến ngày</param>
        /// <param name="customerId">Lọc theo customer ID</param>
        /// <param name="tourCompanyId">Lọc theo tour company ID</param>
        /// <param name="processedById">Lọc theo admin xử lý</param>
        /// <param name="isOverdue">Chỉ hiển thị yêu cầu quá hạn SLA</param>
        /// <param name="priority">Lọc theo độ ưu tiên</param>
        /// <param name="assignedToMe">Chỉ hiển thị yêu cầu được assign cho admin hiện tại</param>
        /// <param name="unassigned">Chỉ hiển thị yêu cầu chưa được assign</param>
        /// <returns>Danh sách yêu cầu hoàn tiền cho admin</returns>
        [HttpGet]
        public async Task<IActionResult> GetRefundRequests(
            [FromQuery] TourRefundStatus? status = null,
            [FromQuery] TourRefundType? refundType = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? searchTerm = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] Guid? customerId = null,
            [FromQuery] Guid? tourCompanyId = null,
            [FromQuery] Guid? processedById = null,
            [FromQuery] bool? isOverdue = null,
            [FromQuery] int? priority = null,
            [FromQuery] bool? assignedToMe = null,
            [FromQuery] bool? unassigned = null)
        {
            try
            {
                var currentAdminId = GetCurrentUserId();
                
                var filter = new AdminRefundFilterDto
                {
                    Status = status,
                    RefundType = refundType,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    SearchTerm = searchTerm,
                    FromDate = fromDate,
                    ToDate = toDate,
                    CustomerId = customerId,
                    TourCompanyId = tourCompanyId,
                    ProcessedById = processedById,
                    IsOverdue = isOverdue,
                    Priority = priority,
                    AssignedToMe = assignedToMe,
                    Unassigned = unassigned
                };

                // Nếu assignedToMe = true, set processedById = currentAdminId
                if (assignedToMe == true)
                {
                    filter.ProcessedById = currentAdminId;
                }

                _logger.LogInformation("Admin {AdminId} getting refund requests - Status: {Status}, Page: {PageNumber}, Search: {SearchTerm}",
                    currentAdminId, status, pageNumber, searchTerm);

                var result = await _tourBookingRefundService.GetAdminRefundRequestsAsync(filter);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund requests for admin");
                return StatusCode(500, "Lỗi server khi lấy danh sách yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một yêu cầu hoàn tiền cho admin
        /// </summary>
        /// <param name="id">ID của yêu cầu hoàn tiền</param>
        /// <returns>Thông tin chi tiết yêu cầu hoàn tiền</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRefundRequestById(Guid id)
        {
            try
            {
                _logger.LogInformation("Admin getting refund request {RefundRequestId}", id);

                var result = await _tourBookingRefundService.GetAdminRefundRequestByIdAsync(id);

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
                _logger.LogError(ex, "Error getting refund request {RefundRequestId} for admin", id);
                return StatusCode(500, "Lỗi server khi lấy thông tin yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Admin approve yêu cầu hoàn tiền
        /// </summary>
        /// <param name="id">ID của yêu cầu hoàn tiền</param>
        /// <param name="approveDto">Thông tin approve</param>
        /// <returns>Kết quả approve</returns>
        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveRefundRequest(Guid id, [FromBody] ApproveRefundDto approveDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = GetCurrentUserId();
                if (adminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} approving refund request {RefundRequestId} with amount {Amount}",
                    adminId, id, approveDto.ApprovedAmount);

                var result = await _tourBookingRefundService.ApproveRefundAsync(id, adminId, approveDto);

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
                _logger.LogError(ex, "Error approving refund request {RefundRequestId} by admin {AdminId}",
                    id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi duyệt yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Admin reject yêu cầu hoàn tiền
        /// </summary>
        /// <param name="id">ID của yêu cầu hoàn tiền</param>
        /// <param name="rejectDto">Thông tin reject</param>
        /// <returns>Kết quả reject</returns>
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectRefundRequest(Guid id, [FromBody] RejectRefundDto rejectDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = GetCurrentUserId();
                if (adminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} rejecting refund request {RefundRequestId} with reason: {Reason}",
                    adminId, id, rejectDto.RejectionReason);

                var result = await _tourBookingRefundService.RejectRefundAsync(id, adminId, rejectDto);

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
                _logger.LogError(ex, "Error rejecting refund request {RefundRequestId} by admin {AdminId}",
                    id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi từ chối yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Admin confirm đã chuyển tiền thủ công
        /// </summary>
        /// <param name="id">ID của yêu cầu hoàn tiền</param>
        /// <param name="confirmDto">Thông tin confirm transfer</param>
        /// <returns>Kết quả confirm</returns>
        [HttpPut("{id}/confirm-transfer")]
        public async Task<IActionResult> ConfirmTransfer(Guid id, [FromBody] ConfirmTransferDto confirmDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = GetCurrentUserId();
                if (adminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} confirming transfer for refund request {RefundRequestId} with reference: {Reference}",
                    adminId, id, confirmDto.TransactionReference);

                var result = await _tourBookingRefundService.ConfirmTransferAsync(id, adminId, confirmDto);

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
                _logger.LogError(ex, "Error confirming transfer for refund request {RefundRequestId} by admin {AdminId}",
                    id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi confirm chuyển tiền");
            }
        }

        /// <summary>
        /// Bulk approve/reject nhiều refund requests
        /// </summary>
        /// <param name="bulkActionDto">Thông tin bulk action</param>
        /// <returns>Kết quả bulk action</returns>
        [HttpPost("bulk-process")]
        public async Task<IActionResult> BulkProcessRefundRequests([FromBody] BulkRefundActionDto bulkActionDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = GetCurrentUserId();
                if (adminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} bulk processing {Count} refund requests with action: {Action}",
                    adminId, bulkActionDto.RefundRequestIds.Count, bulkActionDto.Action);

                var result = await _tourBookingRefundService.BulkProcessRefundRequestsAsync(adminId, bulkActionDto);

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
                _logger.LogError(ex, "Error bulk processing refund requests by admin {AdminId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi xử lý bulk refund requests");
            }
        }

        /// <summary>
        /// Điều chỉnh số tiền hoàn
        /// </summary>
        /// <param name="id">ID của yêu cầu hoàn tiền</param>
        /// <param name="adjustDto">Thông tin điều chỉnh</param>
        /// <returns>Kết quả điều chỉnh</returns>
        [HttpPut("{id}/adjust-amount")]
        public async Task<IActionResult> AdjustRefundAmount(Guid id, [FromBody] AdjustRefundAmountDto adjustDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = GetCurrentUserId();
                if (adminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} adjusting refund amount for request {RefundRequestId} to {NewAmount}",
                    adminId, id, adjustDto.NewRefundAmount);

                var result = await _tourBookingRefundService.AdjustRefundAmountAsync(id, adminId, adjustDto);

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
                _logger.LogError(ex, "Error adjusting refund amount for request {RefundRequestId} by admin {AdminId}",
                    id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi điều chỉnh số tiền hoàn");
            }
        }

        /// <summary>
        /// Reassign refund request cho admin khác
        /// </summary>
        /// <param name="id">ID của yêu cầu hoàn tiền</param>
        /// <param name="reassignDto">Thông tin reassign</param>
        /// <returns>Kết quả reassign</returns>
        [HttpPut("{id}/reassign")]
        public async Task<IActionResult> ReassignRefundRequest(Guid id, [FromBody] ReassignRefundDto reassignDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var currentAdminId = GetCurrentUserId();
                if (currentAdminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} reassigning refund request {RefundRequestId} to admin {NewAssigneeId}",
                    currentAdminId, id, reassignDto.NewAssigneeId);

                var result = await _tourBookingRefundService.ReassignRefundRequestAsync(id, currentAdminId, reassignDto);

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
                _logger.LogError(ex, "Error reassigning refund request {RefundRequestId} by admin {AdminId}",
                    id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi reassign yêu cầu hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy dashboard thống kê refund cho admin
        /// </summary>
        /// <param name="fromDate">Từ ngày</param>
        /// <param name="toDate">Đến ngày</param>
        /// <param name="refundType">Lọc theo loại hoàn tiền</param>
        /// <param name="tourCompanyId">Lọc theo tour company</param>
        /// <returns>Dashboard thống kê</returns>
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetRefundDashboard(
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] TourRefundType? refundType = null,
            [FromQuery] Guid? tourCompanyId = null)
        {
            try
            {
                var filter = new RefundStatisticsFilterDto
                {
                    FromDate = fromDate,
                    ToDate = toDate,
                    RefundType = refundType,
                    TourCompanyId = tourCompanyId
                };

                _logger.LogInformation("Admin getting refund dashboard");

                var result = await _tourBookingRefundService.GetRefundDashboardAsync(filter);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund dashboard for admin");
                return StatusCode(500, "Lỗi server khi lấy dashboard thống kê");
            }
        }

        /// <summary>
        /// Lấy thống kê refund theo tháng
        /// </summary>
        /// <param name="year">Năm</param>
        /// <param name="month">Tháng</param>
        /// <param name="refundType">Loại hoàn tiền (null = tất cả)</param>
        /// <returns>Thống kê theo tháng</returns>
        [HttpGet("monthly-stats")]
        public async Task<IActionResult> GetMonthlyRefundStats(
            [FromQuery] int year,
            [FromQuery] int month,
            [FromQuery] TourRefundType? refundType = null)
        {
            try
            {
                if (year < 2020 || year > DateTime.UtcNow.Year + 1)
                {
                    return BadRequest("Năm không hợp lệ");
                }

                if (month < 1 || month > 12)
                {
                    return BadRequest("Tháng không hợp lệ");
                }

                _logger.LogInformation("Admin getting monthly refund stats for {Year}/{Month}", year, month);

                var result = await _tourBookingRefundService.GetMonthlyRefundStatsAsync(year, month, refundType);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly refund stats for admin");
                return StatusCode(500, "Lỗi server khi lấy thống kê theo tháng");
            }
        }

        /// <summary>
        /// Export refund data
        /// </summary>
        /// <param name="exportFilter">Filter criteria cho export</param>
        /// <returns>File data để download</returns>
        [HttpPost("export")]
        public async Task<IActionResult> ExportRefundData([FromBody] ExportRefundFilterDto exportFilter)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Admin exporting refund data with format: {Format}", exportFilter.ExportFormat);

                var result = await _tourBookingRefundService.ExportRefundDataAsync(exportFilter);

                if (result.IsSuccess && result.Data != null)
                {
                    return File(result.Data.FileData, result.Data.ContentType, result.Data.FileName);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting refund data for admin");
                return StatusCode(500, "Lỗi server khi export dữ liệu");
            }
        }

        /// <summary>
        /// Xử lý hoàn tiền cho company cancellation
        /// </summary>
        /// <param name="companyCancellationDto">Thông tin company cancellation</param>
        /// <returns>Kết quả xử lý company cancellation</returns>
        [HttpPost("process-company-cancellation")]
        public async Task<IActionResult> ProcessCompanyCancellation([FromBody] ProcessCompanyCancellationDto companyCancellationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var adminId = GetCurrentUserId();
                if (adminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} processing company cancellation for {Count} bookings",
                    adminId, companyCancellationDto.TourBookingIds.Count);

                var result = await _tourBookingRefundService.ProcessCompanyCancellationAsync(
                    companyCancellationDto.TourBookingIds,
                    companyCancellationDto.CancellationReason,
                    adminId);

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
                _logger.LogError(ex, "Error processing company cancellation by admin {AdminId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi xử lý company cancellation");
            }
        }

        /// <summary>
        /// Xử lý hoàn tiền cho auto cancellation
        /// </summary>
        /// <param name="autoCancellationDto">Thông tin auto cancellation</param>
        /// <returns>Kết quả xử lý auto cancellation</returns>
        [HttpPost("process-auto-cancellation")]
        public async Task<IActionResult> ProcessAutoCancellation([FromBody] ProcessAutoCancellationDto autoCancellationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Processing auto cancellation for {Count} tour operations",
                    autoCancellationDto.TourOperationIds.Count);

                var result = await _tourBookingRefundService.ProcessAutoCancellationAsync(
                    autoCancellationDto.TourOperationIds,
                    autoCancellationDto.CancellationReason);

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
                _logger.LogError(ex, "Error processing auto cancellation");
                return StatusCode(500, "Lỗi server khi xử lý auto cancellation");
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

    #region Additional DTOs

    /// <summary>
    /// DTO cho process company cancellation
    /// </summary>
    public class ProcessCompanyCancellationDto
    {
        /// <summary>
        /// Danh sách ID của tour bookings bị hủy
        /// </summary>
        public List<Guid> TourBookingIds { get; set; } = new();

        /// <summary>
        /// Lý do hủy từ company
        /// </summary>
        public string CancellationReason { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO cho process auto cancellation
    /// </summary>
    public class ProcessAutoCancellationDto
    {
        /// <summary>
        /// Danh sách ID của tour operations bị auto cancel
        /// </summary>
        public List<Guid> TourOperationIds { get; set; } = new();

        /// <summary>
        /// Lý do auto cancel
        /// </summary>
        public string CancellationReason { get; set; } = string.Empty;
    }

    #endregion
}
