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

        public async Task<SupportTicket?> GetDetail(Guid ticketId)
        {
            return await _context.SupportTickets
             .Where(t => t.Id == ticketId && !t.IsDeleted)
             .Include(t => t.Images)
             .Include(t => t.Comments)
             .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SupportTicket>> ListByAdminAsync(Guid adminId)
        {
            return await _context.SupportTickets
         .Where(t => t.AdminId == adminId && !t.IsDeleted)
         .Include(t => t.User)
         .Include(t => t.Images)
         .Include(t => t.Comments)
         .ToListAsync();
        }

        public async Task<IEnumerable<SupportTicket>> ListByUserAsync(Guid userId)
        {
            return await _context.SupportTickets
        .Where(t => t.UserId == userId && !t.IsDeleted)
        .Include(t => t.User)
        .Include(t => t.Images)
        .Include(t => t.Comments)
        .ToListAsync();
        }
    }
}
