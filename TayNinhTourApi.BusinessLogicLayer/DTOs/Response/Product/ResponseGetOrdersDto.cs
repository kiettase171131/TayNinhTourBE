using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.Common.ResponseDTOs;
using TayNinhTourApi.DataAccessLayer.Repositories;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Product
{
    public class ResponseGetOrdersDto : GenericResponsePagination<OrderDto>
    {
    }
}
