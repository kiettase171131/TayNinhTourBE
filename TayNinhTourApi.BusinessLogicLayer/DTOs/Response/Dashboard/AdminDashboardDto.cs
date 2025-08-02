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
        public int NewTourGuidesThisMonth { get; set; }
        public int NewShopsThisMonth { get; set; }
        public int NewPostsThisMonth { get; set; }
    }
}
