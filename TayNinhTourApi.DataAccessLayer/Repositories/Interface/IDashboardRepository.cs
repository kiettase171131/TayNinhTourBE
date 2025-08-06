using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    public interface IDashboardRepository
    {
        Task<int> GetTotalAccountsAsync();
        Task<int> GetNewAccountsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetBookingsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetOrdersAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetWithdrawRequestsTotalAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetWithdrawRequestsAcceptAsync(DateTime startDate, DateTime endDate);
        Task<int> GetNewCVsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetNewShopsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetPostsAsync(DateTime startDate, DateTime endDate);
        Task<List<(Guid ShopId, decimal Revenue)>> GetTotalRevenueByShopAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTotalPostsAsync(Guid bloggerId,int month, int year);
        Task<int> GetApprovedPostsAsync(Guid bloggerId, int month, int year);
        Task<int> GetRejectedPostsAsync(Guid bloggerId, int month, int year);
        Task<int> GetPendingPostsAsync(Guid bloggerId, int month, int year);
        Task<int> GetTotalLikesAsync(Guid bloggerId, int month, int year);
        Task<int> GetTotalCommentsAsync(Guid bloggerId, int month, int year);
        Task<int> GetTotalProductsAsync(Guid shopId);
        Task<int> GetTotalOrdersAsync(Guid shopId, DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalRevenueAsync(Guid shopId, DateTime startDate, DateTime endDate);
        Task<decimal> GetWalletAsync(Guid shopId);
        Task<(decimal averageRating, int totalRatings)> GetProductRatingsAsync(Guid shopId, DateTime startDate, DateTime endDate);
        Task<decimal?> GetShopRatingAsync(Guid shopId);
    }

}
