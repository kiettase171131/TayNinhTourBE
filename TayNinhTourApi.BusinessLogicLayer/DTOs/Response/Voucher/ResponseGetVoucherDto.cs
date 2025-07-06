using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Voucher
{
    public class ResponseGetVoucherDto
    {
        public int StatusCode { get; set; }
        public string? Message { get; set; }
        public VoucherDto? Data { get; set; }
    }

}
