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

        public async Task<BloggerDashboardDto> GetBloggerStatsAsync(Guid bloggerId, int? month = null, int? year = null)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            bool hasValidYear = year is >= 1 and <= 9999;
            bool hasValidMonth = month is >= 1 and <= 12;
            if (hasValidYear && hasValidMonth)
            {
                startDate = new DateTime(year!.Value, month!.Value, 1);
                endDate = startDate.Value.AddMonths(1);
            }
            else if (hasValidYear && !hasValidMonth)
            {
                // lọc theo cả năm
                startDate = new DateTime(year!.Value, 1, 1);
                endDate = startDate.Value.AddYears(1);
            }

            return new BloggerDashboardDto
            {
                TotalPosts = await _dashboardRepository.GetTotalPostsAsync(bloggerId, startDate, endDate),
                ApprovedPosts = await _dashboardRepository.GetApprovedPostsAsync(bloggerId, startDate, endDate),
                RejectedPosts = await _dashboardRepository.GetRejectedPostsAsync(bloggerId, startDate, endDate),
                PendingPosts = await _dashboardRepository.GetPendingPostsAsync(bloggerId, startDate, endDate),
                TotalLikes = await _dashboardRepository.GetTotalLikesAsync(bloggerId, startDate, endDate),
                TotalComments = await _dashboardRepository.GetTotalCommentsAsync(bloggerId, startDate, endDate)
            };
        }

        public async Task<AdminDashboardDto> GetDashboardAsync(int? year = null, int? month = null)
        {
            DateTime? startDate = null;
            DateTime? endDate = null;
            bool hasValidYear = year is >= 1 and <= 9999;
            bool hasValidMonth = month is >= 1 and <= 12;
            if (hasValidYear && hasValidMonth)
            {
                startDate = new DateTime(year!.Value, month!.Value, 1);
                endDate = startDate.Value.AddMonths(1);
            }
            else if (hasValidYear && !hasValidMonth)
            {
                // lọc theo cả năm
                startDate = new DateTime(year!.Value, 1, 1);
                endDate = startDate.Value.AddYears(1);
            }

            // Gọi repository lấy dữ liệu
            var revenueByShopRaw = await _dashboardRepository.GetTotalRevenueByShopAsync(startDate, endDate);

            // Map sang DTO
            var revenueByShop = revenueByShopRaw
                .Select(r => new ShopRevenueDto
                {
                    ShopId = r.ShopId,                 
                    TotalRevenueBeforeTax = r.Revenue,
                    TotalRevenueAfterTax = r.RevenueTax
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
        public async Task<ShopDashboardDto> GetShopStatisticsAsync(Guid shopId, int? year = null, int? month = null)
        {
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MaxValue;

            // Case 1: Chỉ truyền year
            if (year.HasValue && !month.HasValue)
            {
                startDate = new DateTime(year.Value, 1, 1);
                endDate = startDate.AddYears(1);
            }
            // Case 2: Truyền cả year và month
            else if (year.HasValue && month.HasValue)
            {
                if (month.Value < 1 || month.Value > 12)
                {
                    throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
                }

                startDate = new DateTime(year.Value, month.Value, 1);
                endDate = startDate.AddMonths(1);
            }

            var totalProducts = await _dashboardRepository.GetTotalProductsAsync(shopId);
            var totalOrders = await _dashboardRepository.GetTotalOrdersAsync(shopId, startDate, endDate);

            var (revenue, revenueTax) = await _dashboardRepository.GetTotalRevenueAsync(shopId, startDate, endDate);

            var wallet = await _dashboardRepository.GetWalletAsync(shopId);
            var (avgProductRating, totalProductRatings) = await _dashboardRepository.GetProductRatingsAsync(shopId, startDate, endDate);
            var shopRating = await _dashboardRepository.GetShopRatingAsync(shopId);

            return new ShopDashboardDto
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenueBeforeTax = revenue,
                TotalRevenueAfterTax = revenueTax,
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
