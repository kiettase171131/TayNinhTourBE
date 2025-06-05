using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class BlogReactionRepository : GenericRepository<BlogReaction>,IBlogReactionRepository
    {
        public BlogReactionRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<int> CountByBlogAndReactionAsync(Guid blogId, BlogStatusEnum reactionType)
        {
            return await _context.BlogReactions
                .CountAsync(br => br.BlogId == blogId && br.Reaction == reactionType);
        }

        public async Task<int> CountByBlogAsync(Guid blogId)
        {
            return await _context.BlogReactions
               .CountAsync(br => br.BlogId == blogId);
        }

        public async Task<BlogReaction?> GetByBlogAndUserAsync(Guid blogId, Guid userId)
        {
            return await _context.BlogReactions
                 .FirstOrDefaultAsync(br => br.BlogId == blogId && br.UserId == userId);
        }
    }
}
