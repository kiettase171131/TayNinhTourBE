using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard
{
    public class AdminDashboardDto 
    {
        public int TotalAccounts { get; set; }
        public int NewAccountsThisMonth { get; set; }
        public int BookingsThisMonth { get; set; }
        public int OrdersThisMonth { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal WithdrawRequestsTotal { get; set; }
        public decimal WithdrawRequestsApprove { get; set; }
        public int NewTourGuidesCVThisMonth { get; set; }
        public int NewShopsCVThisMonth { get; set; }
        public int NewPostsThisMonth { get; set; }
        // Thêm danh sách doanh thu theo từng shop
        public List<ShopRevenueDto> RevenueByShop { get; set; } = new();
        public List<CompanyRevenueDto> RevenueByTourCompany { get; set; } = new();
        public List<SlotRevenueDto> RevenueByTourSlot { get; set; } = new();
    }
}
