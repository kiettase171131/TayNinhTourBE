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
    public class SupportTicketRepository : GenericRepository<SupportTicket>, ISupportTicketRepository
    {
        public SupportTicketRepository(TayNinhTouApiDbContext context) : base(context)
        {
        }

        public async Task<SupportTicket?> GetWithCommentsAsync(Guid ticketId)
        {
           return await _context.SupportTickets
                  .Include(t => t.Comments)
                    .ThenInclude(c => c.CreatedBy)
                  .SingleOrDefaultAsync(t => t.Id == ticketId);
        }

        public Task<IEnumerable<SupportTicket>> ListByAdminAsync(Guid adminId)
        {
           return Task.FromResult(_context.SupportTickets.Where(t => t.AdminId == adminId).AsEnumerable());
        }

        public Task<IEnumerable<SupportTicket>> ListByUserAsync(Guid userId)
        {
            return Task.FromResult(_context.SupportTickets.Where(t => t.UserId == userId).AsEnumerable());
        }
    }
}
