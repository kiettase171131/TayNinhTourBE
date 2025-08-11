using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourFeedback
{
    public class ResponseCreateFeedBackDto : BaseResposeDto
    {
        public Guid FeedBackId { get; set; }
        public Guid BookingId { get; set; }
        public Guid SlotId { get; set; }
    }
}
