using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;

using TayNinhTourApi.BusinessLogicLayer.DTOs.ApplicationDTO;

using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Constants.RoleTourCompanyName)]
    public class TourCompanyController : ControllerBase
    {
        private readonly ITourCompanyService _tourCompanyService;
        private readonly ITourGuideApplicationService _tourGuideApplicationService;

        public TourCompanyController(ITourCompanyService tourCompanyService, ITourGuideApplicationService tourGuideApplicationService)
        {
            _tourCompanyService = tourCompanyService;
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
    }
}

