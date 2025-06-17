using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Blog;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product
{
    public class ResponseGetProductByIdDto : BaseResposeDto
    {
        public ProductDto? Data { get; set; }
    }
}
