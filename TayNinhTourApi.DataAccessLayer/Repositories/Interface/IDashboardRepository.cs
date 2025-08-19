using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Enums;
using static System.Net.Mime.MediaTypeNames;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IDashboardRepository
    {
        Task<int> GetTotalAccountsAsync();
        Task<int> GetNewAccountsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetBookingsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetOrdersAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetWithdrawRequestsTotalAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<decimal> GetWithdrawRequestsAcceptAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetNewCVsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetNewShopsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<int> GetPostsAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<List<(Guid ShopId, decimal Revenue, decimal RevenueTax)>> GetTotalRevenueByShopAsync(DateTime? startDate = null, DateTime? endDate = null);
        


        Task<List<(TourDetailsStatus Status, int Count)>> GetGroupedTourDetailsAsync();
        Task<int> GetTotalPostsAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate);
        Task<int> GetApprovedPostsAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate);
        Task<int> GetRejectedPostsAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate);
        Task<int> GetPendingPostsAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate);
        Task<int> GetTotalLikesAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate);
        Task<int> GetTotalCommentsAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate);
        Task<int> GetTotalProductsAsync(Guid shopId);
        Task<int> GetTotalOrdersAsync(Guid shopId, DateTime startDate, DateTime endDate);
        Task<(decimal Revenue, decimal RevenueTax)> GetTotalRevenueAsync(Guid shopId, DateTime startDate, DateTime endDate);
        Task<decimal> GetWalletAsync(Guid shopId);
        Task<(decimal averageRating, int totalRatings)> GetProductRatingsAsync(Guid shopId, DateTime startDate, DateTime endDate);
        Task<decimal?> GetShopRatingAsync(Guid shopId);
        Task<int> CountConfirmedBookingsByUserIdAsync(Guid userId);
        Task<(decimal RevenueHold, decimal Wallet)> GetWalletInfoAsync(Guid userId);
    }

}
