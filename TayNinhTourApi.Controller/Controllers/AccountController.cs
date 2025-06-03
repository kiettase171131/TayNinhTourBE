using Azure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.ApplicationDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ITourGuideApplicationService _tourGuideApplicationService;
        private readonly IBlogReactionService _reactionService;

        public AccountController(IAccountService accountService, ITourGuideApplicationService tourGuideApplicationService, IBlogReactionService blogReactionService)
        {
            _accountService = accountService;
            _tourGuideApplicationService = tourGuideApplicationService;
            _reactionService = blogReactionService;
        }
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword(PasswordDTO password)
        {      
                CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var result = await _accountService.ChangePassword(password, currentUserObject);
                return StatusCode(result.StatusCode, result);

        }
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit-profile")]
        public async Task<IActionResult> UpdateProfile(EditAccountProfileDTO editAccountProfileDTO)
        {
                CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var result = await _accountService.UpdateProfile(editAccountProfileDTO, currentUserObject);
                 return StatusCode(result.StatusCode, result);

        }
        
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
                CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                var result = await _accountService.GetProfile(currentUserObject);
                return StatusCode(result.StatusCode, result);

        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles ="User")]
        [HttpPost("tourguide-application")]
       
        public async Task<IActionResult> Submit([FromForm] SubmitApplicationDto submitApplicationDto)
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _tourGuideApplicationService.SubmitAsync(submitApplicationDto,currentUserObject);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("View-tourguideapplication")]
        public async Task<IActionResult> ListMyApplications()
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var list = await _tourGuideApplicationService.ListByUserAsync(currentUserObject.Id);
            return Ok(list);
        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPut("edit-Avatar")]
        
        public async Task<IActionResult> UpdateAvatar([FromForm] AvatarDTO avatarDTO)
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _accountService.UpdateAvatar(avatarDTO, currentUserObject);
            return StatusCode(result.StatusCode, result);

        }
        [HttpPost("{blogId}/reaction")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ReactToBlog(Guid blogId, [FromBody] RequestBlogReactionDto dto)
        {
            // 1. Kiểm tra DTO hợp lệ
            if (dto == null || (dto.Reaction != BlogStatusEnum.Like && dto.Reaction != BlogStatusEnum.Dislike))
            {
                return BadRequest(new { Message = "Invalid upload data" });
            }
            // đảm bảo blogId trong route khớp dto.BlogId hoặc ignore dto.BlogId và gán:
            dto.BlogId = blogId;
            // 2. Lấy thông tin user hiện tại từ JWT
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var result = await _reactionService.ToggleReactionAsync(dto, currentUserObject);
            return StatusCode(result.StatusCode, result);
        }
    }
}
