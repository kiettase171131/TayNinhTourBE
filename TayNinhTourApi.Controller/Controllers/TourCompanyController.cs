using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.ApplicationDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.Services;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class TourCompanyController : ControllerBase
    {
        private readonly ITourCompanyService _tourCompanyService;
        private readonly ITourTemplateService _tourTemplateService;
        private readonly ITourGuideApplicationService _tourGuideApplicationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDashboardService _dashboard;


        public TourCompanyController(
            ITourCompanyService tourCompanyService,
            ITourTemplateService tourTemplateService,
            ITourGuideApplicationService tourGuideApplicationService,
            ICurrentUserService currentUserService,
            IDashboardService dashboard)
        {
            _tourCompanyService = tourCompanyService;
            _tourTemplateService = tourTemplateService;
            _tourGuideApplicationService = tourGuideApplicationService;
            _currentUserService = currentUserService;
            _dashboard = dashboard;
        }

        [HttpGet("tour")]
        public async Task<ActionResult<ResponseGetToursDto>> GetTours(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var response = await _tourCompanyService.GetToursAsync(pageIndex, pageSize, textSearch, status);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("tour/{id}")]
        public async Task<ActionResult<ResponseGetTourDto>> GetTourById(Guid id)
        {
            var response = await _tourCompanyService.GetTourByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("tour")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<ActionResult<BaseResposeDto>> CreateTour(RequestCreateTourCmsDto request)
        {
            // Get current user id from ICurrentUserService
            var userId = _currentUserService.GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID not found in authentication context.");
            }

            var response = await _tourCompanyService.CreateTourAsync(request, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPatch("tour/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<ActionResult<BaseResposeDto>> UpdateTour(RequestUpdateTourDto request, Guid id)
        {
            // Get current user id from ICurrentUserService
            var userId = _currentUserService.GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID not found in authentication context.");
            }

            var response = await _tourCompanyService.UpdateTourAsync(request, id, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("tour/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<ActionResult<BaseResposeDto>> DeleteTour(Guid id)
        {
            var response = await _tourCompanyService.DeleteTourAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        // ===== TOUR TEMPLATE ENDPOINTS =====

        [HttpGet("template")]
        public async Task<ActionResult<ResponseGetTourTemplatesDto>> GetTourTemplates(
            int pageIndex = 0,
            int pageSize = 10,
            string? templateType = null,
            string? startLocation = null,
            bool includeInactive = false)
        {
            // Parse templateType if provided
            TourTemplateType? parsedTemplateType = null;
            if (!string.IsNullOrEmpty(templateType) && Enum.TryParse<TourTemplateType>(templateType, true, out var type))
            {
                parsedTemplateType = type;
            }

            // Use 0-based pageIndex directly
            var response = await _tourTemplateService.GetTourTemplatesPaginatedAsync(
                pageIndex, pageSize, parsedTemplateType, null, null, startLocation, includeInactive);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("template/{id}")]
        public async Task<ActionResult<ResponseGetTourTemplateDto>> GetTourTemplateById(Guid id)
        {
            var response = await _tourTemplateService.GetTourTemplateByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("template")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<ActionResult<ResponseCreateTourTemplateDto>> CreateTourTemplate(RequestCreateTourTemplateDto request)
        {
            // Get current user id from ICurrentUserService
            var userId = _currentUserService.GetCurrentUserId();

            // Debug logging
            Console.WriteLine($"DEBUG: GetCurrentUserId() returned: {userId}");
            Console.WriteLine($"DEBUG: User.Identity.IsAuthenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"DEBUG: NameIdentifier claim: {User.FindFirst(ClaimTypes.NameIdentifier)?.Value}");
            Console.WriteLine($"DEBUG: Id claim: {User.FindFirst("Id")?.Value}");

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID not found in authentication context.");
            }

            var response = await _tourTemplateService.CreateTourTemplateAsync(request, userId);

            // Nếu tạo template thành công, tự động generate slots cho tháng đã chọn
            if (response.StatusCode == 201 && response.Data != null)
            {
                try
                {
                    // Tự động tạo slots cho template vừa tạo
                    var slotsResult = await _tourTemplateService.GenerateSlotsForTemplateAsync(
                        response.Data.Id,
                        request.Month,
                        request.Year,
                        overwriteExisting: false,
                        autoActivate: true);

                    // Thêm thông tin về slots vào response message
                    if (slotsResult.success)
                    {
                        response.Message += $" và đã tạo {slotsResult.CreatedSlotsCount} slots cho tháng {request.Month}/{request.Year}";
                    }
                    else
                    {
                        response.Message += $" nhưng không thể tạo slots: {slotsResult.Message}";
                    }
                }
                catch (Exception ex)
                {
                    // Log error nhưng không fail toàn bộ request
                    response.Message += " nhưng có lỗi khi tạo slots tự động";
                }
            }

            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("template/holiday")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<ActionResult<ResponseCreateTourTemplateDto>> CreateHolidayTourTemplate(RequestCreateHolidayTourTemplateDto request)
        {
            // Get current user id from ICurrentUserService
            var userId = _currentUserService.GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID not found in authentication context.");
            }

            var response = await _tourTemplateService.CreateHolidayTourTemplateAsync(request, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPatch("template/holiday/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<ActionResult<ResponseUpdateTourTemplateDto>> UpdateHolidayTourTemplate(Guid id, RequestUpdateHolidayTourTemplateDto request)
        {
            // Get current user id from ICurrentUserService
            var userId = _currentUserService.GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID not found in authentication context.");
            }

            var response = await _tourTemplateService.UpdateHolidayTourTemplateAsync(id, request, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPatch("template/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<ActionResult<ResponseUpdateTourTemplateDto>> UpdateTourTemplate(Guid id, RequestUpdateTourTemplateDto request)
        {
            // Get current user id from ICurrentUserService
            var userId = _currentUserService.GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID not found in authentication context.");
            }

            var response = await _tourTemplateService.UpdateTourTemplateAsync(id, request, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("template/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<ActionResult<ResponseDeleteTourTemplateDto>> DeleteTourTemplate(Guid id)
        {
            // Get current user id from ICurrentUserService
            var userId = _currentUserService.GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID not found in authentication context.");
            }

            var response = await _tourTemplateService.DeleteTourTemplateAsync(id, userId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("template/{id}/copy")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<ActionResult<ResponseCopyTourTemplateDto>> CopyTourTemplate(Guid id, [FromBody] CopyTourTemplateRequest request)
        {
            // Get current user id from ICurrentUserService
            var userId = _currentUserService.GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID not found in authentication context.");
            }

            var response = await _tourTemplateService.CopyTourTemplateAsync(id, request.NewTitle, userId);
            return StatusCode(response.StatusCode, response);
        }

        // ===== TOUR DETAILS ENDPOINTS =====

        /// <summary>
        /// Tour company kích hoạt public cho TourDetails
        /// Chuyển status từ WaitToPublic sang Public
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Kết quả kích hoạt</returns>
        [HttpPost("tourdetails/{tourDetailsId:guid}/activate-public")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<ActionResult<BaseResposeDto>> ActivatePublicTourDetails(Guid tourDetailsId)
        {
            // Get current user id from ICurrentUserService
            var userId = _currentUserService.GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return BadRequest("User ID not found in authentication context.");
            }

            var response = await _tourCompanyService.ActivatePublicTourDetailsAsync(tourDetailsId, userId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("Dashboard")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<IActionResult> GetTourcompanyStatistic()
        {
            // Giả sử bạn đã có userId từ token:
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);

            var result = await _dashboard.GetStatisticForCompanyAsync(currentUser.Id);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách incidents của tour company
        /// </summary>
        [HttpGet("incidents")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<IActionResult> GetIncidents(
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? severity = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin user"
                    });
                }

                var response = await _tourCompanyService.GetIncidentsAsync(
                    currentUser.UserId, pageIndex, pageSize, severity, status, fromDate, toDate);

                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi lấy danh sách incidents"
                });
            }
        }

        /// <summary>
        /// Lấy danh sách tours đang hoạt động của tour company
        /// </summary>
        [HttpGet("tours/active")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Tour Company")]
        public async Task<IActionResult> GetActiveTours()
        {
            try
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                if (currentUser?.UserId == null)
                {
                    return Unauthorized(new BaseResposeDto
                    {
                        StatusCode = 401,
                        Message = "Không tìm thấy thông tin user"
                    });
                }

                var response = await _tourCompanyService.GetActiveToursAsync(currentUser.UserId);
                return StatusCode(response.StatusCode, response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = "Lỗi server khi lấy danh sách tours đang hoạt động"
                });
            }
        }
    }

    /// <summary>
    /// Request DTO cho copy tour template
    /// </summary>
    public class CopyTourTemplateRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề mới")]
        [StringLength(200, ErrorMessage = "Tiêu đề không được vượt quá 200 ký tự")]
        public string NewTitle { get; set; } = null!;
    }
}

