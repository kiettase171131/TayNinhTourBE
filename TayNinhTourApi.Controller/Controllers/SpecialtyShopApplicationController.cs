using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpecialtyShop;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;

namespace TayNinhTourApi.Controller.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecialtyShopApplicationController : ControllerBase
    {
        private readonly ISpecialtyShopApplicationService _specialtyShopApplicationService;
        private readonly ILogger<SpecialtyShopApplicationController> _logger;

        public SpecialtyShopApplicationController(
            ISpecialtyShopApplicationService specialtyShopApplicationService,
            ILogger<SpecialtyShopApplicationController> logger)
        {
            _specialtyShopApplicationService = specialtyShopApplicationService;
            _logger = logger;
        }

        /// <summary>
        /// Admin lấy danh sách tất cả đơn đăng ký Specialty Shop với phân trang và filter
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllApplications(
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? status = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                var statusEnum = status.HasValue ? (SpecialtyShopApplicationStatus)status.Value : (SpecialtyShopApplicationStatus?)null;
                var result = await _specialtyShopApplicationService.GetApplicationsAsync(page, pageSize, statusEnum, searchTerm);
                
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    Message = "Applications retrieved successfully",
                    Data = new
                    {
                        Applications = result.Applications,
                        TotalCount = result.TotalCount,
                        PageIndex = page,
                        PageSize = pageSize
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving specialty shop applications");
                return StatusCode(500, new { Error = "An error occurred while retrieving applications", Details = ex.Message });
            }
        }

        /// <summary>
        /// Admin xem chi tiết một đơn đăng ký Specialty Shop
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplicationById(Guid id)
        {
            try
            {
                var application = await _specialtyShopApplicationService.GetApplicationByIdAsync(id);
                
                if (application == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        success = false,
                        Message = "Specialty shop application not found",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<SpecialtyShopApplicationDto>
                {
                    success = true,
                    Message = "Specialty shop application retrieved successfully",
                    Data = application
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving specialty shop application with ID: {Id}", id);
                return StatusCode(500, new { Error = "An error occurred while retrieving application", Details = ex.Message });
            }
        }

        /// <summary>
        /// Admin phê duyệt đơn đăng ký Specialty Shop
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveApplication(Guid id)
        {
            try
            {
                CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var result = await _specialtyShopApplicationService.ApproveApplicationAsync(id, currentUser);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving specialty shop application with ID: {Id}", id);
                return StatusCode(500, new { Error = "An error occurred while approving application", Details = ex.Message });
            }
        }

        /// <summary>
        /// Admin từ chối đơn đăng ký Specialty Shop
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> RejectApplication(Guid id, [FromBody] RejectApplicationDto dto)
        {
            try
            {
                CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var rejectDto = new RejectSpecialtyShopApplicationDto { Reason = dto.RejectionReason };
                var result = await _specialtyShopApplicationService.RejectApplicationAsync(id, rejectDto, currentUser);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting specialty shop application with ID: {Id}", id);
                return StatusCode(500, new { Error = "An error occurred while rejecting application", Details = ex.Message });
            }
        }

        /// <summary>
        /// Admin lấy thống kê đơn đăng ký Specialty Shop
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpGet("statistics")]
        public async Task<IActionResult> GetApplicationStatistics()
        {
            try
            {
                var pendingCount = await _specialtyShopApplicationService.CountApplicationsByStatusAsync(SpecialtyShopApplicationStatus.Pending);
                var approvedCount = await _specialtyShopApplicationService.CountApplicationsByStatusAsync(SpecialtyShopApplicationStatus.Approved);
                var rejectedCount = await _specialtyShopApplicationService.CountApplicationsByStatusAsync(SpecialtyShopApplicationStatus.Rejected);
                
                var stats = new
                {
                    PendingCount = pendingCount,
                    ApprovedCount = approvedCount,
                    RejectedCount = rejectedCount,
                    TotalCount = pendingCount + approvedCount + rejectedCount
                };
                
                return Ok(new ApiResponse<object>
                {
                    success = true,
                    Message = "Application statistics retrieved successfully",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving application statistics");
                return StatusCode(500, new { Error = "An error occurred while retrieving statistics", Details = ex.Message });
            }
        }
    }
}
