
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
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;

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


        public TourCompanyController(
            ITourCompanyService tourCompanyService,
            ITourTemplateService tourTemplateService,
            ITourGuideApplicationService tourGuideApplicationService,
            ICurrentUserService currentUserService)
        {
            _tourCompanyService = tourCompanyService;
            _tourTemplateService = tourTemplateService;
            _tourGuideApplicationService = tourGuideApplicationService;
            _currentUserService = currentUserService;
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
            int pageIndex = 1,
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

            // Convert 1-based pageIndex to 0-based for service
            var response = await _tourTemplateService.GetTourTemplatesPaginatedAsync(
                pageIndex - 1, pageSize, parsedTemplateType, null, null, startLocation, includeInactive);
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
                    if (slotsResult.IsSuccess)
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

