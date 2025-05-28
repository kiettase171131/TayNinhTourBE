using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Entities;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.SupTicketDTO
{
    public class ChangeStatusDto
    {
        public TicketStatus NewStatus { get; set; }
    }
}
