﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Payment
{
    public class CheckoutResultDto : BaseResposeDto
    {
        public string CheckoutUrl { get; set; }
        public Guid OrderId { get; set; }
        public decimal TotalOriginal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAfterDiscount { get; set; }
    }

}
