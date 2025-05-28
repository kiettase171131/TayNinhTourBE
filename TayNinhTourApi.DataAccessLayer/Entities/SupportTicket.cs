using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.DataAccessLayer.Entities
{
    public enum TicketStatus
    {
        Open ,
        InProgress ,
        Closed
    }

    public class SupportTicket : BaseEntity
    {
        
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public Guid? AdminId { get; set; }
        public virtual User? Admin { get; set; }
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public TicketStatus Status { get; set; } = TicketStatus.Open;
        public virtual ICollection<SupportTicketComment> Comments { get; set; } = new List<SupportTicketComment>();
        public virtual ICollection<SupportTicketImage> Images { get; set; } = new List<SupportTicketImage>();
    }
}
