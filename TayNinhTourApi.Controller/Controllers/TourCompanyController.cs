using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;

using TayNinhTourApi.BusinessLogicLayer.DTOs.ApplicationDTO;

using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Constants.RoleTourCompanyName)]
    public class TourCompanyController : ControllerBase
    {
        private readonly ITourCompanyService _tourCompanyService;
        private readonly ITourTemplateService _tourTemplateService;
        private readonly ITourGuideApplicationService _tourGuideApplicationService;

        public TourCompanyController(ITourCompanyService tourCompanyService, ITourTemplateService tourTemplateService, ITourGuideApplicationService tourGuideApplicationService)
        {
            _tourCompanyService = tourCompanyService;
            _tourTemplateService = tourTemplateService;
            _tourGuideApplicationService = tourGuideApplicationService;
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
        public async Task<ActionResult<BaseResposeDto>> CreateTour(RequestCreateTourCmsDto request)
        {
            // Get current user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            var response = await _tourCompanyService.CreateTourAsync(request, Guid.Parse(userId));
            return StatusCode(response.StatusCode, response);
        }

        [HttpPatch("tour/{id}")]
        public async Task<ActionResult<BaseResposeDto>> UpdateTour(RequestUpdateTourDto request, Guid id)
        {
            // Get current user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            var response = await _tourCompanyService.UpdateTourAsync(request, id, Guid.Parse(userId));
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("tour/{id}")]
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
            decimal? minPrice = null,
            decimal? maxPrice = null,
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
                pageIndex - 1, pageSize, parsedTemplateType, minPrice, maxPrice, startLocation, includeInactive);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("template/{id}")]
        public async Task<ActionResult<ResponseGetTourTemplateDto>> GetTourTemplateById(Guid id)
        {
            var response = await _tourTemplateService.GetTourTemplateByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("template")]
        public async Task<ActionResult<ResponseCreateTourTemplateDto>> CreateTourTemplate(RequestCreateTourTemplateDto request)
        {
            // Get current user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            var response = await _tourTemplateService.CreateTourTemplateAsync(request, Guid.Parse(userId));
            return StatusCode(response.StatusCode, response);
        }

        [HttpPatch("template/{id}")]
        public async Task<ActionResult<ResponseUpdateTourTemplateDto>> UpdateTourTemplate(Guid id, RequestUpdateTourTemplateDto request)
        {
            // Get current user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            var response = await _tourTemplateService.UpdateTourTemplateAsync(id, request, Guid.Parse(userId));
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("template/{id}")]
        public async Task<ActionResult<ResponseDeleteTourTemplateDto>> DeleteTourTemplate(Guid id)
        {
            // Get current user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            var response = await _tourTemplateService.DeleteTourTemplateAsync(id, Guid.Parse(userId));
            return StatusCode(response.StatusCode, response);
        }

        [HttpPost("template/{id}/copy")]
        public async Task<ActionResult<ResponseCopyTourTemplateDto>> CopyTourTemplate(Guid id, [FromBody] CopyTourTemplateRequest request)
        {
            // Get current user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            var response = await _tourTemplateService.CopyTourTemplateAsync(id, request.NewTitle, Guid.Parse(userId));
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

