using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Request.Voucher
{
    public class CreateVoucherDto
    {
        public string Code { get; set; } = null!;
        public decimal DiscountAmount { get; set; }
        public int? DiscountPercent { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}
