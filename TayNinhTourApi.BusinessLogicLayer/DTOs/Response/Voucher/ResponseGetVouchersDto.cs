using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Voucher
{
    public class ResponseGetVouchersDto
    {
        public int StatusCode { get; set; }
        public List<VoucherDto> Data { get; set; } = new();
        public int TotalRecord { get; set; }
        public int TotalPages { get; set; }
    }

}
