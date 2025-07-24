using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Voucher
{
    public class ResponseCreateVoucher : BaseResposeDto
    {
        public Guid VoucherId { get; set; }
        public List<string> GeneratedCodes { get; set; } = new List<string>();
    }
}
