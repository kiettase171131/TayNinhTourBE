using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.SupTicketDTO
{
    public class CreateTicketDto
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
    }
}
