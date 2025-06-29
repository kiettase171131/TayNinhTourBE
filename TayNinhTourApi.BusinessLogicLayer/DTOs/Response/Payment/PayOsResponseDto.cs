using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment
{
    public class PayOsResponseDto : BaseResposeDto
    {
        public string code { get; set; } = null!;
        public string desc { get; set; } = null!;
        public string checkoutUrl { get; set; } = null!;
    }

}
