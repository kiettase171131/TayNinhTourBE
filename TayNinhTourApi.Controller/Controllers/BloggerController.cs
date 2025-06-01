using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
    [Authorize(Roles = Constants.RoleBloggerName)]
    public class BloggerController : ControllerBase
    {
        private readonly IBlogService _blogService;
        

        public BloggerController(IBlogService blogService)
        {
            _blogService = blogService;
            
        }
        [HttpGet("blog")]
        public async Task<ActionResult<ResponseGetBlogsDto>> GetBlogs(int? pageIndex, int? pageSize, string? textSearch, bool? status)
        {
            var response = await _blogService.GetBlogsAsync(pageIndex, pageSize, textSearch, status);
            return StatusCode(response.StatusCode, response);
        }

        [HttpGet("blog/{id}")]
        public async Task<ActionResult<ResponseGetBlogByIdDto>> GetBlogById(Guid id)
        {
            var response = await _blogService.GetBlogByIdAsync(id);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPost("blog")]
        public async Task<ActionResult<ResponseCreateBlogDto>> Create([FromForm] RequestCreateBlogDto dto)
        {
            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var response = await _blogService.CreateBlogAsync(dto, currentUserObject);
            return StatusCode(response.StatusCode, response);
        }
        [HttpDelete("blog/{id}")]
        public async Task<ActionResult<BaseResposeDto>> DeleteBlog(Guid id)
        {
            var response = await _blogService.DeleteBlogAsync(id);
            return StatusCode(response.StatusCode, response);
        }
        [HttpPut("blog/{id:guid}")]
        public async Task<ActionResult<BaseResposeDto>> UpdateBlog([FromRoute] Guid id,[FromForm] RequestUpdateBlogDto dto)
        {

            CurrentUserObject currentUserObject = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
            var response = await _blogService.UpdateBlogAsync(dto,id , currentUserObject);
            return StatusCode(response.StatusCode, response);
        }
    }
}
