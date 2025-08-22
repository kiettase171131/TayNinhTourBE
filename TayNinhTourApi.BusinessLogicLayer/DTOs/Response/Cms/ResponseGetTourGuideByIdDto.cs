using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Cms
{
    public class ResponseGetTourGuideByIdDto : BaseResposeDto
    {
        public TourGuideCmsDto? Data { get; set; }
    }
  
}
