using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard
{
    public class ShopDashboardDto
    {
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal Wallet { get; set; }
        public decimal AverageProductRating { get; set; }
        public int TotalProductRatings { get; set; }
        public decimal? ShopAverageRating { get; set; }
    }
}
