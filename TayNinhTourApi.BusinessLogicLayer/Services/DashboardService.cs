using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.Dashboard;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
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
            

            // Gọi repository lấy dữ liệu
            var revenueByShopRaw = await _dashboardRepository.GetTotalRevenueByShopAsync(startDate, endDate);

            // Map sang DTO
            var revenueByShop = revenueByShopRaw
                .Select(r => new ShopRevenueDto
                {
                    ShopId = r.ShopId,
                   
                    TotalRevenue = r.Revenue
                })
                .ToList();

            var dto = new AdminDashboardDto
            {
                TotalAccounts = await _dashboardRepository.GetTotalAccountsAsync(),
                NewAccountsThisMonth = await _dashboardRepository.GetNewAccountsAsync(startDate, endDate),
                BookingsThisMonth = await _dashboardRepository.GetBookingsAsync(startDate, endDate),
                OrdersThisMonth = await _dashboardRepository.GetOrdersAsync(startDate, endDate),
                TotalRevenue = await _dashboardRepository.GetTotalRevenueAsync(startDate, endDate),
                WithdrawRequestsTotal = await _dashboardRepository.GetWithdrawRequestsTotalAsync(startDate, endDate),
                WithdrawRequestsApprove = await _dashboardRepository.GetWithdrawRequestsAcceptAsync(startDate, endDate),
                NewTourGuidesCVThisMonth = await _dashboardRepository.GetNewCVsAsync(startDate, endDate),
                NewShopsCVThisMonth = await _dashboardRepository.GetNewShopsAsync(startDate, endDate),
                NewPostsThisMonth = await _dashboardRepository.GetPostsAsync(startDate, endDate),
                RevenueByShop = revenueByShop
            };

          return dto;
        }
        public async Task<ShopDashboardDto> GetShopStatisticsAsync(Guid shopId, int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1);
           


            var totalProducts = await _dashboardRepository.GetTotalProductsAsync(shopId);
            var totalOrders = await _dashboardRepository.GetTotalOrdersAsync(shopId, startDate, endDate);
            var totalRevenue = await _dashboardRepository.GetTotalRevenueAsync(shopId, startDate, endDate);
            var wallet = await _dashboardRepository.GetWalletAsync(shopId);
            var (avgProductRating, totalProductRatings) = await _dashboardRepository.GetProductRatingsAsync(shopId, startDate, endDate);
            var shopRating = await _dashboardRepository.GetShopRatingAsync(shopId);

            return new ShopDashboardDto
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                Wallet = wallet,
                AverageProductRating = avgProductRating,
                TotalProductRatings = totalProductRatings,
                ShopAverageRating = shopRating
            };
        }
        public async Task<List<TourDetailsStatisticDto>> GetTourDetailsStatisticsAsync()
        {
            var groupedData = await _dashboardRepository.GetGroupedTourDetailsAsync();

            int pendingCount = groupedData
                .Where(g => g.Status == TourDetailsStatus.Pending)
                .Sum(g => g.Count);

            int approvedCount = groupedData
                .Where(g => g.Status != TourDetailsStatus.Pending)
                .Sum(g => g.Count);

            return new List<TourDetailsStatisticDto>
        {
            new TourDetailsStatisticDto { StatusGroup = "Chờ duyệt", Count = pendingCount },
            new TourDetailsStatisticDto { StatusGroup = "Đã duyệt", Count = approvedCount }
        };
        }
        public async Task<TourCompanyStatisticDto> GetStatisticForCompanyAsync(Guid userId)
        {
            var confirmedCount = await _dashboardRepository.CountConfirmedBookingsByUserIdAsync(userId);
            var (revenueHold, wallet) = await _dashboardRepository.GetWalletInfoAsync(userId);

            return new TourCompanyStatisticDto
            {
                ConfirmedBookings = confirmedCount,
                RevenueHold = revenueHold,
                Wallet = wallet
            };
        }
    }
}
