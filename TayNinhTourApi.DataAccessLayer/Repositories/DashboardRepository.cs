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
                .Where(w => w.RequestedAt >= startDate && w.RequestedAt < endDate)
                .SumAsync(w => (decimal?)w.Amount) ?? 0;

        public Task<int> GetNewCVsAsync(DateTime startDate, DateTime endDate)
            => _context.TourGuideApplications.CountAsync(c => c.SubmittedAt >= startDate && c.SubmittedAt < endDate);

        public Task<int> GetNewShopsAsync(DateTime startDate, DateTime endDate)
            => _context.SpecialtyShopApplications.CountAsync(s => s.SubmittedAt >= startDate && s.SubmittedAt < endDate);

        public Task<int> GetPostsAsync(DateTime startDate, DateTime endDate)
            => _context.Blogs.CountAsync(b => b.CreatedAt >= startDate && b.CreatedAt < endDate);
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

    }
}
