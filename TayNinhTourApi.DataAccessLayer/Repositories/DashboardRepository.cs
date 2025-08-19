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

        // Repository (EF Core)

        public Task<int> GetNewAccountsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Users.AsQueryable();
            if (startDate.HasValue) query = query.Where(u => u.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(u => u.CreatedAt < endDate.Value);
            return query.CountAsync();
        }

        public Task<int> GetBookingsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.TourBookings.AsQueryable();
            if (startDate.HasValue) query = query.Where(b => b.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(b => b.CreatedAt < endDate.Value);
            return query.CountAsync();
        }

        public Task<int> GetOrdersAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders.AsQueryable();
            if (startDate.HasValue) query = query.Where(o => o.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(o => o.CreatedAt < endDate.Value);
            return query.CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Orders
                .Where(o => o.Status == OrderStatus.Paid)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(o => o.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(o => o.CreatedAt < endDate.Value);

            return await query.SumAsync(o => (decimal?)o.TotalAfterDiscount) ?? 0m;
        }

        public async Task<decimal> GetWithdrawRequestsTotalAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.WithdrawalRequests.AsQueryable();
            if (startDate.HasValue) query = query.Where(w => w.RequestedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(w => w.RequestedAt < endDate.Value);

            return await query.SumAsync(w => (decimal?)w.Amount) ?? 0m;
        }

        public async Task<decimal> GetWithdrawRequestsAcceptAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.WithdrawalRequests
                .Where(w => w.Status == WithdrawalStatus.Approved)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(w => w.RequestedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(w => w.RequestedAt < endDate.Value);

            return await query.SumAsync(w => (decimal?)w.Amount) ?? 0m;
        }

        public Task<int> GetNewCVsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.TourGuideApplications.AsQueryable();
            if (startDate.HasValue) query = query.Where(c => c.SubmittedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(c => c.SubmittedAt < endDate.Value);
            return query.CountAsync();
        }

        public Task<int> GetNewShopsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.SpecialtyShopApplications.AsQueryable();
            if (startDate.HasValue) query = query.Where(s => s.SubmittedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(s => s.SubmittedAt < endDate.Value);
            return query.CountAsync();
        }

        public Task<int> GetPostsAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.Blogs.AsQueryable();
            if (startDate.HasValue) query = query.Where(b => b.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(b => b.CreatedAt < endDate.Value);
            return query.CountAsync();
        }

        public async Task<List<(Guid ShopId, decimal Revenue, decimal RevenueTax)>> GetTotalRevenueByShopAsync(
            DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.OrderDetails
                .Where(od => od.Order.Status == OrderStatus.Paid)
                .AsQueryable();

            if (startDate.HasValue) query = query.Where(od => od.Order.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(od => od.Order.CreatedAt < endDate.Value);

            // Ví dụ giữ nguyên công thức của bạn: 90% trước thuế, 80% sau thuế
            return await query
                .GroupBy(od => od.Product.ShopId)
                .Select(g => new ValueTuple<Guid, decimal, decimal>(
                    g.Key,
                    g.Sum(x => x.Order.TotalAfterDiscount * 0.9m),
                    g.Sum(x => x.Order.TotalAfterDiscount * 0.8m)
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
        private static IQueryable<T> FilterByDateRange<T>(IQueryable<T> query, DateTime? startDate, DateTime? endDate)
    where T : BaseEntity
        {
            if (startDate.HasValue) query = query.Where(x => x.CreatedAt >= startDate.Value);
            if (endDate.HasValue) query = query.Where(x => x.CreatedAt < endDate.Value);
            return query;
        }

        public Task<int> GetTotalPostsAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate)
    => FilterByDateRange(_context.Blogs.Where(b => b.UserId == bloggerId), startDate, endDate).CountAsync();

        public Task<int> GetApprovedPostsAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate)
            => FilterByDateRange(_context.Blogs.Where(b => b.UserId == bloggerId && b.Status == 1), startDate, endDate).CountAsync();

        public Task<int> GetRejectedPostsAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate)
            => FilterByDateRange(_context.Blogs.Where(b => b.UserId == bloggerId && b.Status == 2), startDate, endDate).CountAsync();

        public Task<int> GetPendingPostsAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate)
            => FilterByDateRange(_context.Blogs.Where(b => b.UserId == bloggerId && b.Status == 0), startDate, endDate).CountAsync();

        public Task<int> GetTotalLikesAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate)
            => FilterByDateRange(
                _context.BlogReactions.Where(r => r.Reaction == BlogStatusEnum.Like && r.Blog.UserId == bloggerId),
                startDate, endDate).CountAsync();

        public Task<int> GetTotalCommentsAsync(Guid bloggerId, DateTime? startDate, DateTime? endDate)
            => FilterByDateRange(
                _context.BlogComments.Where(c => c.Blog.UserId == bloggerId),
                startDate, endDate).CountAsync();

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

        public async Task<(decimal Revenue, decimal RevenueTax)> GetTotalRevenueAsync(Guid shopId, DateTime startDate, DateTime endDate)
        {
            var productIds = await _context.Products
                .Where(p => p.ShopId == shopId)
                .Select(p => p.Id)
                .ToListAsync();

            var totalRevenue90 = await _context.OrderDetails
                .Where(od =>
                    productIds.Contains(od.ProductId) &&
                    od.Order.Status == OrderStatus.Paid &&
                    od.Order.CreatedAt >= startDate &&
                    od.Order.CreatedAt < endDate)
                .SumAsync(od => (decimal?)(od.Order.TotalAfterDiscount * 0.9m)) ?? 0;

            var totalRevenue80 = await _context.OrderDetails
                .Where(od =>
                    productIds.Contains(od.ProductId) &&
                    od.Order.Status == OrderStatus.Paid &&
                    od.Order.CreatedAt >= startDate &&
                    od.Order.CreatedAt < endDate)
                .SumAsync(od => (decimal?)(od.Order.TotalAfterDiscount * 0.8m)) ?? 0;

            return (totalRevenue90, totalRevenue80);
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
        //tourcompany
        public async Task<int> CountConfirmedBookingsByUserIdAsync(Guid userId)
        {
            return await _context.TourBookings
                .Where(b => b.Status == BookingStatus.Confirmed &&
                            b.TourOperation.TourDetails.TourTemplate.CreatedById == userId)
                .CountAsync();
        }

        public async Task<(decimal RevenueHold, decimal Wallet)> GetWalletInfoAsync(Guid userId)
        {
            var company = await _context.TourCompanies
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (company == null)
                return (0, 0);

            return (company.RevenueHold, company.Wallet);
        }
    }
}
