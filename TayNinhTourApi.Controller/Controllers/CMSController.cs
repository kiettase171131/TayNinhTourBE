using Azure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.ApplicationDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.SupTicketDTO;
using TayNinhTourApi.BusinessLogicLayer.Services;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Constants.RoleAdminName)]
    public class CmsController : ControllerBase
    {
        private readonly ICmsService _cmsService;
        private readonly ITourGuideApplicationService _tourGuideApplicationService;
        private readonly ISupportTicketService _supportTicketService;

        public CmsController(ICmsService cmsService, ITourGuideApplicationService tourGuideApplicationService, ISupportTicketService supportTicketService)
        {
            _cmsService = cmsService;
            _tourGuideApplicationService = tourGuideApplicationService;
            _supportTicketService = supportTicketService;
        }

        [HttpGet("tour")]
        public async Task<ActionResult<ResponseGetToursDto>> GetTours(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var response = await _cmsService.GetToursAsync(pageIndex, pageSize, textSearch, status);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("tour/{id}")]
        public async Task<ActionResult<ResponseGetTourDto>> GetTourById(Guid id)
        {
            var response = await _cmsService.GetTourByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("user")]
        public async Task<ActionResult<ResponseGetUsersCmsDto>> GetUser(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var response = await _cmsService.GetUserAsync(pageIndex, pageSize, textSearch, status);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("user/{id}")]
        public async Task<ActionResult<ResponseGetUserByIdCmsDto>> GetUserById(Guid id)
        {
            var response = await _cmsService.GetUserByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPatch("user/{id}")]
        public async Task<ActionResult<BaseResposeDto>> UpdateUser(RequestUpdateUserCmsDto request, Guid id)
        {
            var response = await _cmsService.UpdateUserAsync(request, id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpDelete("user/{id}")]
        public async Task<ActionResult<BaseResposeDto>> DeleteUser(Guid id)
        {
            var response = await _cmsService.DeleteUserAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPatch("tour/{id}")]
        public async Task<ActionResult<BaseResposeDto>> UpdateTour(RequestUpdateTourCmsDto request, Guid id)
        {
            // Get current user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            var response = await _cmsService.UpdateTourAsync(request, id, Guid.Parse(userId));
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("tour-guide-application")]
        public async Task<IActionResult> GetPending()
        {
            var list = await _tourGuideApplicationService.GetPendingAsync();
            return Ok(list);
        }
        [HttpPut("{id:guid}/approve-application")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var response = await _tourGuideApplicationService.ApproveAsync(id);
            return StatusCode(response.StatusCode, response);
        }

        [HttpPut("{id:guid}/reject-application")]

        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectApplicationDto dto)
        {
            var response = await _tourGuideApplicationService.RejectAsync(id, dto.Reason);

            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("SupportTicket")]
        public async Task<IActionResult> ListForAdmin()
        {
            try
            {
                CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var tickets = await _supportTicketService.GetTicketsForAdminAsync(currentUserObject.Id);
                return Ok(tickets);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [HttpPost("{id:guid}/Reply-SupportTicket")]
        public async Task<IActionResult> Reply(Guid id, [FromBody] CreateCommentDto dto)
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var response = await _supportTicketService.ReplyAsync(id, currentUserObject.Id, dto.CommentText);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("Update-blog/{id}")]
        public async Task<ActionResult<BaseResposeDto>> UpdateBlog(RequestUpdateBlogCmsDto request, Guid id)
        {
            // Get current user id from claims
            var userId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found in claims.");
            }

            var response = await _cmsService.UpdateBlogAsync(request, id, Guid.Parse(userId));
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("Blog")]    
        public async Task<ActionResult<ResponseGetBlogsDto>> GetBlogs(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var response = await _cmsService.GetBlogsAsync(pageIndex, pageSize, textSearch, status);
            return StatusCode(response.StatusCode, response);
        }
    }
}

