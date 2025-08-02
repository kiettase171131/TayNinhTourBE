using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<BloggerDashboardDto> GetBloggerStatsAsync(Guid bloggerId, int month, int year)
        {
            return new BloggerDashboardDto
            {
                TotalPosts = await _dashboardRepository.GetTotalPostsAsync(bloggerId, month, year),
                ApprovedPosts = await _dashboardRepository.GetApprovedPostsAsync(bloggerId, month, year),
                RejectedPosts = await _dashboardRepository.GetRejectedPostsAsync(bloggerId, month, year),
                PendingPosts = await _dashboardRepository.GetPendingPostsAsync(bloggerId, month, year),
                TotalLikes = await _dashboardRepository.GetTotalLikesAsync(bloggerId, month, year),
                TotalComments = await _dashboardRepository.GetTotalCommentsAsync(bloggerId, month, year)
            };
        }

        public async Task<AdminDashboardDto> GetDashboardAsync(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var dto = new AdminDashboardDto
            {
                TotalAccounts = await _dashboardRepository.GetTotalAccountsAsync(),
                NewAccountsThisMonth = await _dashboardRepository.GetNewAccountsAsync(startDate, endDate),
                BookingsThisMonth = await _dashboardRepository.GetBookingsAsync(startDate, endDate),
                OrdersThisMonth = await _dashboardRepository.GetOrdersAsync(startDate, endDate),
                TotalRevenue = await _dashboardRepository.GetTotalRevenueAsync(startDate, endDate),
                WithdrawRequestsTotal = await _dashboardRepository.GetWithdrawRequestsTotalAsync(startDate, endDate),
                NewTourGuidesThisMonth = await _dashboardRepository.GetNewCVsAsync(startDate, endDate),
                NewShopsThisMonth = await _dashboardRepository.GetNewShopsAsync(startDate, endDate),
                NewPostsThisMonth = await _dashboardRepository.GetPostsAsync(startDate, endDate)
            };

          return dto;
        }
    }
}
