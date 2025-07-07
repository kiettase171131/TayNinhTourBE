using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Voucher
{
    public class ApplyVoucherResult : BaseResposeDto
    {
        public decimal FinalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
    }

}
