using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Payment
{
    public class CheckoutSelectedCartItemsDto
    {
        public List<Guid> CartItemIds { get; set; }
    }
}
