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
    public class SupportTicketCommentRepository : GenericRepository<SupportTicketComment>, ISupportTicketCommentRepository
    {
        public SupportTicketCommentRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public Task<IEnumerable<SupportTicketComment>> ListByTicketAsync(Guid ticketId)
        {
            return Task.FromResult(_context.SupportTicketComments
                            .Where(c => c.SupportTicketId == ticketId)
                            .OrderBy(c => c.CreatedAt)
                            .AsEnumerable());
        }
    }
}
