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
    public class BlogCommentRepository : GenericRepository<BlogComment>, IBlogCommentRepository
    {
        public BlogCommentRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<BlogComment>> ListByBlogAsync(Guid blogId)
        {
            return await _context.BlogComments
               .Where(bc => bc.BlogId == blogId)
               .Include(bc => bc.User)         // load thông tin user (nếu cần)
               .Include(bc => bc.Replies)      // load replies cấp 1 (nếu cần)
               .AsNoTracking()
               .ToListAsync();
        }
    }
}
