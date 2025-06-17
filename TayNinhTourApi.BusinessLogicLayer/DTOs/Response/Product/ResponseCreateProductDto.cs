using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product
{
    public class ResponseCreateProductDto : BaseResposeDto
    {
        public Guid ProductId { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
