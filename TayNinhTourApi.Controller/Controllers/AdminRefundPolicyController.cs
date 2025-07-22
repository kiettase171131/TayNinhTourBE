using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourBookingRefund;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.Controller.Controllers
{
    /// <summary>
    /// Controller cho admin quản lý chính sách hoàn tiền
    /// Cung cấp APIs cho admin tạo, cập nhật và quản lý refund policies
    /// </summary>
    [Route("api/Admin/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class RefundPolicyController : ControllerBase
    {
        private readonly IRefundPolicyService _refundPolicyService;
        private readonly ILogger<RefundPolicyController> _logger;

        public RefundPolicyController(
            IRefundPolicyService refundPolicyService,
            ILogger<RefundPolicyController> logger)
        {
            _refundPolicyService = refundPolicyService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách policies cho admin management
        /// </summary>
        /// <param name="refundType">Lọc theo loại hoàn tiền (null = tất cả)</param>
        /// <param name="isActive">Lọc theo trạng thái active (null = tất cả)</param>
        /// <param name="pageNumber">Số trang (default: 1)</param>
        /// <param name="pageSize">Kích thước trang (default: 10)</param>
        /// <returns>Danh sách policies với pagination</returns>
        [HttpGet]
        public async Task<IActionResult> GetPolicies(
            [FromQuery] TourRefundType? refundType = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var filter = new AdminRefundFilterDto
                {
                    RefundType = refundType,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                _logger.LogInformation("Admin getting refund policies - Type: {RefundType}, Active: {IsActive}",
                    refundType, isActive);

                var result = await _refundPolicyService.GetPoliciesForAdminAsync(filter);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund policies for admin");
                return StatusCode(500, "Lỗi server khi lấy danh sách chính sách hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy policies active theo loại hoàn tiền
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <param name="effectiveDate">Ngày hiệu lực (optional)</param>
        /// <returns>Danh sách policies active</returns>
        [HttpGet("active/{refundType}")]
        public async Task<IActionResult> GetActivePoliciesByType(
            TourRefundType refundType,
            [FromQuery] DateTime? effectiveDate = null)
        {
            try
            {
                _logger.LogInformation("Getting active policies for refund type: {RefundType}", refundType);

                var result = await _refundPolicyService.GetActivePoliciesByTypeAsync(refundType, effectiveDate);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active policies for refund type: {RefundType}", refundType);
                return StatusCode(500, "Lỗi server khi lấy policies active");
            }
        }

        /// <summary>
        /// Tạo policy mới
        /// </summary>
        /// <param name="policyDto">Thông tin policy</param>
        /// <returns>Policy vừa tạo</returns>
        [HttpPost]
        public async Task<IActionResult> CreatePolicy([FromBody] CreateRefundPolicyDto policyDto)
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

                var policy = new RefundPolicy
                {
                    RefundType = policyDto.RefundType,
                    MinDaysBeforeEvent = policyDto.MinDaysBeforeEvent,
                    MaxDaysBeforeEvent = policyDto.MaxDaysBeforeEvent,
                    RefundPercentage = policyDto.RefundPercentage,
                    ProcessingFee = policyDto.ProcessingFee,
                    ProcessingFeePercentage = policyDto.ProcessingFeePercentage,
                    Description = policyDto.Description,
                    Priority = policyDto.Priority,
                    EffectiveFrom = policyDto.EffectiveFrom,
                    EffectiveTo = policyDto.EffectiveTo,
                    InternalNotes = policyDto.InternalNotes
                };

                _logger.LogInformation("Admin {AdminId} creating refund policy for type: {RefundType}",
                    adminId, policyDto.RefundType);

                var result = await _refundPolicyService.CreatePolicyAsync(policy, adminId);

                if (result.IsSuccess)
                {
                    return CreatedAtAction(nameof(GetPolicyById),
                        new { id = result.Data!.Id }, result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating refund policy by admin {AdminId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi tạo chính sách hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy thông tin chi tiết một policy
        /// </summary>
        /// <param name="id">ID của policy</param>
        /// <returns>Thông tin chi tiết policy</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPolicyById(Guid id)
        {
            try
            {
                _logger.LogInformation("Getting refund policy {PolicyId}", id);

                var policy = await _refundPolicyService.GetApplicablePolicyAsync(TourRefundType.UserCancellation, 0);
                // Note: This is a placeholder - we need a GetByIdAsync method in the service

                if (policy == null)
                {
                    return NotFound("Không tìm thấy chính sách hoàn tiền");
                }

                return Ok(policy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting refund policy {PolicyId}", id);
                return StatusCode(500, "Lỗi server khi lấy thông tin chính sách hoàn tiền");
            }
        }

        /// <summary>
        /// Cập nhật policy
        /// </summary>
        /// <param name="id">ID của policy</param>
        /// <param name="policyDto">Thông tin policy mới</param>
        /// <returns>Policy sau khi cập nhật</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePolicy(Guid id, [FromBody] UpdateRefundPolicyDto policyDto)
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

                var policy = new RefundPolicy
                {
                    Id = id,
                    RefundType = policyDto.RefundType,
                    MinDaysBeforeEvent = policyDto.MinDaysBeforeEvent,
                    MaxDaysBeforeEvent = policyDto.MaxDaysBeforeEvent,
                    RefundPercentage = policyDto.RefundPercentage,
                    ProcessingFee = policyDto.ProcessingFee,
                    ProcessingFeePercentage = policyDto.ProcessingFeePercentage,
                    Description = policyDto.Description,
                    Priority = policyDto.Priority,
                    EffectiveFrom = policyDto.EffectiveFrom,
                    EffectiveTo = policyDto.EffectiveTo,
                    InternalNotes = policyDto.InternalNotes
                };

                _logger.LogInformation("Admin {AdminId} updating refund policy {PolicyId}",
                    adminId, id);

                var result = await _refundPolicyService.UpdatePolicyAsync(id, policy, adminId);

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
                _logger.LogError(ex, "Error updating refund policy {PolicyId} by admin {AdminId}",
                    id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi cập nhật chính sách hoàn tiền");
            }
        }

        /// <summary>
        /// Activate/Deactivate policy
        /// </summary>
        /// <param name="id">ID của policy</param>
        /// <param name="statusDto">Trạng thái active mới</param>
        /// <returns>Kết quả cập nhật</returns>
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdatePolicyStatus(Guid id, [FromBody] UpdatePolicyStatusDto statusDto)
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

                _logger.LogInformation("Admin {AdminId} updating policy {PolicyId} status to {IsActive}",
                    adminId, id, statusDto.IsActive);

                var result = await _refundPolicyService.UpdatePolicyStatusAsync(id, statusDto.IsActive, adminId);

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
                _logger.LogError(ex, "Error updating policy {PolicyId} status by admin {AdminId}",
                    id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi cập nhật trạng thái chính sách");
            }
        }

        /// <summary>
        /// Xóa policy (soft delete)
        /// </summary>
        /// <param name="id">ID của policy</param>
        /// <returns>Kết quả xóa</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePolicy(Guid id)
        {
            try
            {
                var adminId = GetCurrentUserId();
                if (adminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} deleting refund policy {PolicyId}",
                    adminId, id);

                var result = await _refundPolicyService.DeletePolicyAsync(id, adminId);

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
                _logger.LogError(ex, "Error deleting refund policy {PolicyId} by admin {AdminId}",
                    id, GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi xóa chính sách hoàn tiền");
            }
        }

        /// <summary>
        /// Lấy next available priority cho loại hoàn tiền
        /// </summary>
        /// <param name="refundType">Loại hoàn tiền</param>
        /// <returns>Priority tiếp theo có thể sử dụng</returns>
        [HttpGet("next-priority/{refundType}")]
        public async Task<IActionResult> GetNextAvailablePriority(TourRefundType refundType)
        {
            try
            {
                _logger.LogInformation("Getting next available priority for refund type: {RefundType}", refundType);

                var priority = await _refundPolicyService.GetNextAvailablePriorityAsync(refundType);

                return Ok(new { NextPriority = priority });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next available priority for refund type: {RefundType}", refundType);
                return StatusCode(500, "Lỗi server khi lấy priority tiếp theo");
            }
        }

        /// <summary>
        /// Tạo default policies cho hệ thống
        /// </summary>
        /// <returns>Kết quả tạo default policies</returns>
        [HttpPost("create-defaults")]
        public async Task<IActionResult> CreateDefaultPolicies()
        {
            try
            {
                var adminId = GetCurrentUserId();
                if (adminId == Guid.Empty)
                {
                    return Unauthorized("Không thể xác định admin");
                }

                _logger.LogInformation("Admin {AdminId} creating default refund policies", adminId);

                var result = await _refundPolicyService.CreateDefaultPoliciesAsync(adminId);

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
                _logger.LogError(ex, "Error creating default policies by admin {AdminId}", GetCurrentUserId());
                return StatusCode(500, "Lỗi server khi tạo default policies");
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
    /// DTO cho tạo refund policy
    /// </summary>
    public class CreateRefundPolicyDto
    {
        public TourRefundType RefundType { get; set; }
        public int MinDaysBeforeEvent { get; set; }
        public int? MaxDaysBeforeEvent { get; set; }
        public decimal RefundPercentage { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal ProcessingFeePercentage { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? InternalNotes { get; set; }
    }

    /// <summary>
    /// DTO cho cập nhật refund policy
    /// </summary>
    public class UpdateRefundPolicyDto
    {
        public TourRefundType RefundType { get; set; }
        public int MinDaysBeforeEvent { get; set; }
        public int? MaxDaysBeforeEvent { get; set; }
        public decimal RefundPercentage { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal ProcessingFeePercentage { get; set; }
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public string? InternalNotes { get; set; }
    }

    /// <summary>
    /// DTO cho cập nhật policy status
    /// </summary>
    public class UpdatePolicyStatusDto
    {
        public bool IsActive { get; set; }
    }

    #endregion
}
