using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public class SupportTicketComment : BaseEntity
    {
        
        public Guid SupportTicketId { get; set; }
        public virtual SupportTicket SupportTicket { get; set; } = null!;
        public new Guid CreatedById { get; set; }
        public virtual User CreatedBy { get; set; } = null!;
        public string CommentText { get; set; } = null!;
    }
}
