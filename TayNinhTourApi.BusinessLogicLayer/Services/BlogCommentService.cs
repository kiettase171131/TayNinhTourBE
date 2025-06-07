using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class BlogCommentService : IBlogCommentService
    {
        private readonly IBlogCommentRepository _commentRepo;
        private readonly IBlogRepository _blog;
        private readonly IUserRepository _userRepository;
        public BlogCommentService(IBlogCommentRepository blogCommentRepository, IBlogRepository blog, IUserRepository userRepository)
        {
            _commentRepo = blogCommentRepository;
            _blog = blog;
            _userRepository = userRepository;
        }

        public async Task<ResponseCommentDto> CreateCommentAsync(Guid blogId, Guid userId, RequestCreateCommentDto request)
        {
            var blog = await _blog.GetByIdAsync(blogId);
            if (blog == null || blog.IsDeleted)
            {
                return new ResponseCommentDto
                {
                    StatusCode = 404,
                    Message = "Blog not found"
                };
            }
            if (blog.Status != (byte)BlogStatus.Accepted)
            {
                return new ResponseCommentDto
                {
                    StatusCode = 404,
                    Message = "Blog isn't acceepted, cannot comment"
                };
            }

            // 2. Kiểm tra User tồn tại không
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new ResponseCommentDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }

            // 3. Tạo BlogComment mới (ParentCommentId = null)
            var comment = new BlogComment
            {
                Id = Guid.NewGuid(),
                BlogId = blogId,
                UserId = userId,
                Content = request.Content,
                ParentCommentId = null,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
                IsDeleted = false,
                IsActive = true
            };

            await _commentRepo.AddAsync(comment);
            await _commentRepo.SaveChangesAsync();

            // 4. Trả về DTO
            return new ResponseCommentDto
            {
                StatusCode = 200,
                Message = "Comment successfully",
                Id = comment.Id,
                BlogId = blogId,
                ParentCommentId = null,
                UserId = userId,
                UserName = user.Name,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                Replies = new List<ResponseCommentDto>()
            };
        }

        public async Task<ResponseCommentDto> CreateReplyAsync(Guid blogId, Guid parentCommentId, Guid userId, RequestCreateCommentDto request)
        {
            var blog = await _blog.GetByIdAsync(blogId);
            if (blog == null || blog.IsDeleted)
            {
                return new ResponseCommentDto
                {
                    StatusCode = 404,
                    Message = "Blog not found"
                };
            }

            // 2. Kiểm tra User tồn tại không
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                return new ResponseCommentDto
                {
                    StatusCode = 404,
                    Message = "User not found"
                };
            }
            // 3. Kiểm tra ParentComment tồn tại và thuộc cùng Blog
            var parentComment = await _commentRepo.GetByIdAsync(parentCommentId);
            if (parentComment == null || parentComment.BlogId != blogId)
            {
               return new ResponseCommentDto
               {
                   StatusCode = 404,
                   Message = "Parent comment not found or does not belong to this blog"
               };
            }

            // 4. Tạo BlogComment (reply)
            var reply = new BlogComment
            {
                Id = Guid.NewGuid(),
                BlogId = blogId,
                UserId = userId,
                Content = request.Content,
                ParentCommentId = parentCommentId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
                IsDeleted = false,
                IsActive = true
            };

            await _commentRepo.AddAsync(reply);
            await _commentRepo.SaveChangesAsync();

            return new ResponseCommentDto
            {
                StatusCode = 200,
                Message = "Reply successfully",
                Id = reply.Id,
                BlogId = blogId,
                ParentCommentId = parentCommentId,
                UserId = userId,
                UserName = user.Name,
                Content = reply.Content,
                CreatedAt = reply.CreatedAt,
                Replies = new List<ResponseCommentDto>() // reply con tạm thời để rỗng
            };
        }

        public async Task<List<ResponseCommentDto>> GetCommentsByBlogAsync(Guid blogId)
        {
            var allComments = (await _commentRepo.ListByBlogAsync(blogId)).ToList();
            // 2. Lọc ra các top-level comment (ParentCommentId == null)
            var topLevel = allComments
                .Where(c => c.ParentCommentId == null)
                .OrderBy(c => c.CreatedAt)
                .ToList();

            // 3. Với mỗi top-level, xây dựng cây replies
            List<ResponseCommentDto> result = new();
            foreach (var comment in topLevel)
            {
                var dto = MapEntityToDto(comment, allComments);
                result.Add(dto);
            }

            return result;
        }
        private ResponseCommentDto MapEntityToDto(BlogComment entity, List<BlogComment> allComments)
        {
            // Lấy tên user (nếu entity.User đã được include, dùng entity.User.Name, 
            // ngược lại truy vấn lại)
            string userName = entity.User.Name;

            var dto = new ResponseCommentDto
            {
                Id = entity.Id,
                BlogId = entity.BlogId,
                ParentCommentId = entity.ParentCommentId,
                UserId = entity.UserId,
                UserName = userName,
                Content = entity.Content,
                CreatedAt = entity.CreatedAt,
                Replies = new List<ResponseCommentDto>()
            };

            // Tìm tất cả direct replies
            var directReplies = allComments
                .Where(c => c.ParentCommentId == entity.Id)
                .OrderBy(c => c.CreatedAt)
                .ToList();

            foreach (var child in directReplies)
            {
                var childDto = MapEntityToDto(child, allComments);
                dto.Replies.Add(childDto);
            }

            return dto;
        }
    }
}
