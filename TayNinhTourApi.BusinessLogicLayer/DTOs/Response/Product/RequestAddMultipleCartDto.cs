using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product
{
    public class RequestAddMultipleCartDto
    {
        public List<RequestAddToCartDto> Items { get; set; } = new();
    }
}