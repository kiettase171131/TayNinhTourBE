using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.SupTicketDTO
{
    public class SupportTicketDto : BaseResposeDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserEmail { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string Status { get; set; } = null!; // hoặc enum string
        public DateTime CreatedAt { get; set; }

        
        public Guid? AdminId { get; set; }
        public IEnumerable<SupportTicketImageDto> Images { get; set; } = new List<SupportTicketImageDto>();
        public IEnumerable<SupportTicketCommentDto> Comments { get; set; } = new List<SupportTicketCommentDto>();
    }
}
