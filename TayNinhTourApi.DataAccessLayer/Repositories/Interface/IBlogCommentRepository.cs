using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IBlogCommentRepository : IGenericRepository<BlogComment>
    {
        Task<IEnumerable<BlogComment>> ListByBlogAsync(Guid blogId);
        Task<Dictionary<Guid, int>> GetCommentCountsAsync(IEnumerable<Guid> blogIds);
    }
}
