using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.AccountDTO;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Blog;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class BlogReactionService : IBlogReactionService
    {
        private readonly IBlogReactionRepository _reactionRepo;
        private readonly IBlogRepository _blog;
        public BlogReactionService(IBlogReactionRepository reactionRepo, IBlogRepository blog)
        {
            _reactionRepo = reactionRepo;
            _blog = blog;
        }
        public async Task<ResponseBlogReactionDto> ToggleReactionAsync(RequestBlogReactionDto request, CurrentUserObject currentUserObject)
        {
            // 1. Kiểm tra Blog tồn tại chưa, và chưa bị xóa
            var blog = await _blog.GetByIdAsync(request.BlogId);

            if (blog == null || blog.IsDeleted)
            {
                return new ResponseBlogReactionDto
                {
                    StatusCode = 404,
                    Message = "Blog not found"
                };
            }

            // 2. Lấy bản ghi reaction hiện tại (nếu có)
            var existingReaction = await _reactionRepo.GetByBlogAndUserAsync(request.BlogId, currentUserObject.Id);

            if (existingReaction == null)
            {
                // Chưa có reaction: thêm mới
                var newReaction = new BlogReaction
                {
                    Id = Guid.NewGuid(),
                    BlogId = request.BlogId,
                    UserId = currentUserObject.Id,
                    Reaction = request.Reaction,
                    CreatedAt = DateTime.UtcNow,
                    CreatedById = currentUserObject.Id,
                    IsDeleted = false,
                    IsActive = true
                };

                await _reactionRepo.AddAsync(newReaction);
                await _reactionRepo.SaveChangesAsync();
            }
            else
            {
                if (existingReaction.Reaction == request.Reaction)
                {
                    // Nếu click lại cùng loại => xoá reaction (toggling off)
                    await _reactionRepo.DeleteAsync(existingReaction.Id);
                    await _reactionRepo.SaveChangesAsync();
                }
                else
                {
                    // Đổi từ Like sang Dislike hoặc ngược lại
                    existingReaction.Reaction = request.Reaction;
                    existingReaction.UpdatedAt = DateTime.UtcNow;
                    existingReaction.UpdatedById = currentUserObject.Id;

                    await _reactionRepo.UpdateAsync(existingReaction);
                    await _reactionRepo.SaveChangesAsync();
                }
            }

            // 3. Đếm lại tổng like và dislike
            var totalLikes = await _reactionRepo.CountByBlogAndReactionAsync(request.BlogId, BlogStatusEnum.Like);
            var totalDislikes = await _reactionRepo.CountByBlogAndReactionAsync(request.BlogId, BlogStatusEnum.Dislike);

            // 4. Lấy reaction hiện tại của user (nếu còn)
            var currentUserReaction = await _reactionRepo.GetByBlogAndUserAsync(request.BlogId, currentUserObject.Id);
            BlogStatusEnum? reactionForUser = currentUserReaction?.Reaction;

            return new ResponseBlogReactionDto
            {
                StatusCode = 200,
                Message = "Operation successful",
                TotalLikes = totalLikes,
                TotalDislikes = totalDislikes,
                CurrentUserReaction = reactionForUser
            };
        }
    }
}
