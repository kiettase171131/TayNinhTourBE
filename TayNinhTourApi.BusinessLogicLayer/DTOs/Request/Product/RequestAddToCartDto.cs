using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Product
{
    public class RequestAddToCartDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

}
