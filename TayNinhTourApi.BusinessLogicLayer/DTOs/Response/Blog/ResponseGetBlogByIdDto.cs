using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog
{
    public class ResponseGetBlogByIdDto : BaseResposeDto
    {
        public BlogDto? Data { get; set; }
    }
}
