using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TayNinhTourApi.DataAccessLayer.Contexts;
using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.Repositories.Interface;

namespace TayNinhTourApi.DataAccessLayer.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly TayNinhTouApiDbContext _context;

        public DashboardRepository(TayNinhTouApiDbContext context)
        {
            _context = context;
        }

        public Task<int> GetTotalAccountsAsync()
            => _context.Users.CountAsync();

        public Task<int> GetNewAccountsAsync(DateTime startDate, DateTime endDate)
            => _context.Users.CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt < endDate);

        public Task<int> GetBookingsAsync(DateTime startDate, DateTime endDate)
            => _context.TourBookings.CountAsync(b => b.CreatedAt >= startDate && b.CreatedAt < endDate);

        public Task<int> GetOrdersAsync(DateTime startDate, DateTime endDate)
            => _context.Orders.CountAsync(o => o.CreatedAt >= startDate && o.CreatedAt < endDate);

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
            => await _context.Orders
                .Where(o => o.Status == OrderStatus.Paid && o.CreatedAt >= startDate && o.CreatedAt < endDate)
                .SumAsync(o => (decimal?)o.TotalAfterDiscount) ?? 0;

        public async Task<decimal> GetWithdrawRequestsTotalAsync(DateTime startDate, DateTime endDate)
            => await _context.WithdrawalRequests
                .Where(w => w.RequestedAt >= startDate && w.RequestedAt < endDate )
                .SumAsync(w => (decimal?)w.Amount) ?? 0;
        public async Task<decimal> GetWithdrawRequestsAcceptAsync(DateTime startDate, DateTime endDate)
            => await _context.WithdrawalRequests
                .Where(w => w.RequestedAt >= startDate && w.RequestedAt < endDate && w.Status == WithdrawalStatus.Approved)
                .SumAsync(w => (decimal?)w.Amount) ?? 0;

        public Task<int> GetNewCVsAsync(DateTime startDate, DateTime endDate)
            => _context.TourGuideApplications.CountAsync(c => c.SubmittedAt >= startDate && c.SubmittedAt < endDate);

        public Task<int> GetNewShopsAsync(DateTime startDate, DateTime endDate)
            => _context.SpecialtyShopApplications.CountAsync(s => s.SubmittedAt >= startDate && s.SubmittedAt < endDate);

        public Task<int> GetPostsAsync(DateTime startDate, DateTime endDate)
            => _context.Blogs.CountAsync(b => b.CreatedAt >= startDate && b.CreatedAt < endDate);
        public async Task<List<(Guid ShopId, decimal Revenue)>> GetTotalRevenueByShopAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.OrderDetails  
                .Where(od =>
                    od.Order.Status == OrderStatus.Paid &&
                    od.Order.CreatedAt >= startDate &&
                    od.Order.CreatedAt < endDate)
                .GroupBy(od => od.Product.SpecialtyShopId)
                .Select(g => new ValueTuple<Guid, decimal>(
                    g.Key,
                    g.Sum(x => x.Order.TotalAfterDiscount)
                ))
                .ToListAsync();
        }
        public async Task<List<(TourDetailsStatus Status, int Count)>> GetGroupedTourDetailsAsync()
        {
            return await _context.TourDetails
                .GroupBy(td => td.Status)
                .Select(g => new ValueTuple<TourDetailsStatus, int>(g.Key, g.Count()))
                .ToListAsync();
        }
        //Blogger
        private static IQueryable<T> FilterByMonth<T>(IQueryable<T> query, int month, int year)
               where T : BaseEntity
        {
            return query.Where(x => x.CreatedAt.Month == month && x.CreatedAt.Year == year);
        }
        public Task<int> GetTotalPostsAsync(Guid bloggerId, int month, int year)
       => FilterByMonth(_context.Blogs.Where(b => b.UserId == bloggerId), month, year).CountAsync();

        public Task<int> GetApprovedPostsAsync(Guid bloggerId, int month, int year)
            => FilterByMonth(_context.Blogs.Where(b => b.UserId == bloggerId && b.Status == 1), month, year).CountAsync();

        public Task<int> GetRejectedPostsAsync(Guid bloggerId, int month, int year)
            => FilterByMonth(_context.Blogs.Where(b => b.UserId == bloggerId && b.Status == 2), month, year).CountAsync();

        public Task<int> GetPendingPostsAsync(Guid bloggerId, int month, int year)
            => FilterByMonth(_context.Blogs.Where(b => b.UserId == bloggerId && b.Status == 0), month, year).CountAsync();

        public Task<int> GetTotalLikesAsync(Guid bloggerId, int month, int year)
            => FilterByMonth(_context.BlogReactions
                .Where(r => r.Reaction == BlogStatusEnum.Like && r.Blog.UserId == bloggerId), month, year)
                .CountAsync();

        public Task<int> GetTotalCommentsAsync(Guid bloggerId, int month, int year)
            => FilterByMonth(_context.BlogComments
                .Where(c => c.Blog.UserId == bloggerId), month, year)
                .CountAsync();
        //Shop
        public async Task<int> GetTotalProductsAsync(Guid shopId)
    => await _context.Products.CountAsync(p => p.ShopId == shopId);


        public async Task<int> GetTotalOrdersAsync(Guid shopId, DateTime startDate, DateTime endDate)
        {
            var productIds = await _context.Products
                .Where(p => p.ShopId == shopId)
                .Select(p => p.Id)
                .ToListAsync();

            return await _context.OrderDetails
                .Where(od => productIds.Contains(od.ProductId)
                          && od.Order.CreatedAt >= startDate
                          && od.Order.CreatedAt < endDate)
                .Select(od => od.OrderId)
                .Distinct()
                .CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(Guid shopId, DateTime startDate, DateTime endDate)
        {
            var productIds = await _context.Products
                .Where(p => p.ShopId == shopId)
                .Select(p => p.Id)
                .ToListAsync();

            return await _context.OrderDetails
                .Where(od => productIds.Contains(od.ProductId)
                          && od.Order.Status == OrderStatus.Paid
                          && od.Order.CreatedAt >= startDate
                          && od.Order.CreatedAt < endDate)
                .SumAsync(od => (decimal?)od.Order.TotalAfterDiscount) ?? 0;
        }

        public async Task<decimal> GetWalletAsync(Guid shopId)
        {
            var shop = await _context.SpecialtyShops.FirstOrDefaultAsync(s => s.UserId == shopId);
            return shop?.Wallet ?? 0;
        }

        public async Task<(decimal averageRating, int totalRatings)> GetProductRatingsAsync(Guid shopId, DateTime startDate, DateTime endDate)
        {
            var productIds = await _context.Products
                .Where(p => p.ShopId == shopId)
                .Select(p => p.Id)
                .ToListAsync();

            var ratings = await _context.ProductRatings
                .Where(r => productIds.Contains(r.ProductId)
                         && r.CreatedAt >= startDate
                         && r.CreatedAt < endDate)
                .ToListAsync();

            if (!ratings.Any()) return (0, 0);

            return ((decimal)ratings.Average(r => r.Rating), ratings.Count);
        }

        public async Task<decimal?> GetShopRatingAsync(Guid shopId)
        {
            var shop = await _context.SpecialtyShops.FirstOrDefaultAsync(s => s.UserId == shopId);
            return shop?.Rating;
        }

    }
}
