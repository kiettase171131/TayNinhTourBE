using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Common.ResponseDTOs;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product
{
    public class ResponseGetCartDto : GenericResponsePagination<CartItemDto>
    {
        public decimal TotalAmount { get; set; }
    }
}
