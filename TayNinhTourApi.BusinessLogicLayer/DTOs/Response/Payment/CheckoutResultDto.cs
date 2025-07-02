using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment
{
    public class CheckoutResultDto
    {
        public string CheckoutUrl { get; set; }
        public Guid OrderId { get; set; }
    }

}
