using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard
{
    public class ShopRevenueDto
    {
        public Guid ShopId { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
