using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    public interface IBlogCommentService
    {
        // Tạo comment gốc (ParentCommentId = null)
        Task<ResponseCommentDto> CreateCommentAsync(Guid blogId, Guid userId, RequestCreateCommentDto request);

        // Tạo reply cho comment (ParentCommentId = commentId)
        Task<ResponseCommentDto> CreateReplyAsync(Guid blogId, Guid parentCommentId, Guid userId, RequestCreateCommentDto request);

        // Lấy tất cả comment của blog, kèm replies
        Task<List<ResponseCommentDto>> GetCommentsByBlogAsync(Guid blogId);
    }
}
