using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.SupTicketDTO
{
    public class SupportTicketCommentDto
    {
        public Guid Id { get; set; }
        public Guid CreatedById { get; set; }
        public string CommentText { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        
    }
}
