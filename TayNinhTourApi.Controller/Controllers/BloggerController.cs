using Azure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Xml.Linq;
using TayNinhTourApi.BusinessLogicLayer.Common;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.SupTicketDTO;
using TayNinhTourApi.BusinessLogicLayer.Services;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.Controller.Helper;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.Controller.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class BloggerController : ControllerBase
    {
        private readonly IBlogService _blogService;
        private readonly IBlogCommentService _commentService;
        private readonly IBlogReactionService _reactionService;


        public BloggerController(IBlogService blogService, IBlogCommentService commentService, IBlogReactionService reactionService)
        {
            _blogService = blogService;
            _commentService = commentService;
            _reactionService = reactionService;
        }
        [HttpGet("Blog-Blogger")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Blogger")]
        public async Task<ActionResult<ResponseGetBlogsDto>> GetBlogs(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            CurrentUserObject currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var response = await _blogService.GetBlogsAsync(pageIndex, pageSize, textSearch, status, currentUser);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("Blog-User")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseGetBlogsDto>> GetAcceptedBlogs(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            Guid? currentUserId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                currentUserId = user?.Id;
            }
            var response = await _blogService.GetAcceptedBlogsAsync(pageIndex, pageSize, textSearch, status, currentUserId);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("blog/{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseGetBlogByIdDto>> GetBlogById(Guid id)
        {
            Guid? currentUserId = null;
            if (HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
                currentUserId = currentUser?.Id;
            }
            var response = await _blogService.GetBlogByIdAsync(id, currentUserId);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("blog")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Blogger")]
        public async Task<ActionResult<ResponseCreateBlogDto>> Create([FromForm] RequestCreateBlogDto dto)
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var response = await _blogService.CreateBlogAsync(dto, currentUserObject);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("blog/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Blogger")]
        public async Task<ActionResult<BaseResposeDto>> DeleteBlog(Guid id)
        {
            var response = await _blogService.DeleteBlogAsync(id);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("blog/{id:guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Blogger")]
        public async Task<ActionResult<BaseResposeDto>> UpdateBlog([FromRoute] Guid id,[FromForm] RequestUpdateBlogDto dto)
        {

            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var response = await _blogService.UpdateBlogAsync(dto,id , currentUserObject);
            return StatusCode(response.StatusCode, response);
        }
        [HttpGet("{blogId}/comments")]
        public async Task<ActionResult<ResponseCommentDto>> GetComments(Guid blogId)
        {
            var comments = await _commentService.GetCommentsByBlogAsync(blogId);
            return Ok(comments);
        }
        [HttpPost("{blogId}/comments")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CreateComment(Guid blogId, [FromBody] RequestCreateCommentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest(new { Message = "Comment content cannot be left blank" });
            }
            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var response = await _commentService.CreateCommentAsync(blogId, currentUser.Id, dto);
            return StatusCode(response.StatusCode, response);

        }
        [HttpPost("{blogId}/comments/{parentCommentId}/reply")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> ReplyComment(Guid blogId,Guid parentCommentId,[FromBody] RequestCreateCommentDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest(new { Message = "Comment content cannot be left blank" });
            }

            var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var response = await _commentService.CreateReplyAsync(blogId, parentCommentId, currentUser.Id, dto);
            return StatusCode(response.StatusCode, response);

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
