using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.SpTicket
{
    public class ResponseSpTicketDto : BaseResposeDto
    {
        public Guid SupportTicketId { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
