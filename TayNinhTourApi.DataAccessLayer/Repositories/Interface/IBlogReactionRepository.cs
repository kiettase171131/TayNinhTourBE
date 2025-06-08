using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IBlogReactionRepository : IGenericRepository<BlogReaction>
    {
        Task<BlogReaction?> GetByBlogAndUserAsync(Guid blogId, Guid userId);
        Task<int> CountByBlogAndReactionAsync(Guid blogId, BlogStatusEnum reactionType);
        Task<int> CountByBlogAsync(Guid blogId);
        Task<Dictionary<Guid, int>> GetLikeCountsAsync(IEnumerable<Guid> blogIds);
        Task<Dictionary<Guid, int>> GetDislikeCountsAsync(IEnumerable<Guid> blogIds);
    }
}
