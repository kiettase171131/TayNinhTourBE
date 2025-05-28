using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    
    public interface ISupportTicketRepository : IGenericRepository<SupportTicket>
    {
        Task<SupportTicket?> GetWithCommentsAsync(Guid ticketId);
        Task<IEnumerable<SupportTicket>> ListByUserAsync(Guid userId);
        Task<IEnumerable<SupportTicket>> ListByAdminAsync(Guid adminId);
    }

}
