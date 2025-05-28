using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class SupportTicketImage : BaseEntity
    {
        public Guid SupportTicketId { get; set; }
        public virtual SupportTicket SupportTicket { get; set; } = null!;
        public string Url { get; set; } = null!;
    }
}
