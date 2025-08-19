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

        // New methods for tour company revenue statistics

        /// <summary>
        /// Lấy danh sách tour companies với thông tin cơ bản và phân trang
        /// </summary>
        public async Task<(List<(Guid TourCompanyId, Guid UserId, string CompanyName, string Email, decimal Wallet, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt)> companies, int totalCount)> 
            GetTourCompaniesAsync(int pageIndex, int pageSize, string? searchTerm = null, bool? isActive = null)
        {
            var query = _context.TourCompanies
                .Include(tc => tc.User)
                .Where(tc => !tc.IsDeleted);

            // Apply active filter
            if (isActive.HasValue)
            {
                query = query.Where(tc => tc.IsActive == isActive.Value);
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(tc => 
                    tc.CompanyName.ToLower().Contains(searchLower) ||
                    tc.User.Email.ToLower().Contains(searchLower));
            }

            var totalCount = await query.CountAsync();

            var companiesData = await query
                .OrderByDescending(tc => tc.CreatedAt)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .Select(tc => new
                {
                    TourCompanyId = tc.Id,
                    UserId = tc.UserId,
                    CompanyName = tc.CompanyName,
                    Email = tc.User.Email,
                    Wallet = tc.Wallet,
                    IsActive = tc.IsActive,
                    CreatedAt = tc.CreatedAt,
                    UpdatedAt = tc.UpdatedAt ?? tc.CreatedAt
                })
                .ToListAsync();

            var companies = companiesData.Select(c => (
                TourCompanyId: c.TourCompanyId,
                UserId: c.UserId,
                CompanyName: c.CompanyName,
                Email: c.Email,
                Wallet: c.Wallet,
                IsActive: c.IsActive,
                CreatedAt: c.CreatedAt,
                UpdatedAt: c.UpdatedAt
            )).ToList();

            return (companies, totalCount);
        }

        /// <summary>
        /// Lấy tổng revenue hold của tour company từ các bookings chưa transfer
        /// </summary>
        public async Task<decimal> GetTourCompanyRevenueHoldAsync(Guid tourCompanyUserId)
        {
            return await _context.TourBookings
                .Where(b => b.TourOperation.TourDetails.CreatedById == tourCompanyUserId
                    && !b.IsDeleted
                    && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    && b.RevenueHold > 0
                    && !b.RevenueTransferredDate.HasValue)
                .SumAsync(b => (decimal?)b.RevenueHold) ?? 0;
        }

        /// <summary>
        /// Lấy thống kê bookings trong tháng của tour company
        /// </summary>
        public async Task<(decimal totalRevenueBeforeTax, decimal totalRevenueAfterTax, int confirmedBookings, int bookedSlots)> 
            GetTourCompanyMonthlyStatsAsync(Guid tourCompanyUserId, DateTime startDate, DateTime endDate)
        {
            var monthlyBookings = await _context.TourBookings
                .Where(b => b.TourOperation.TourDetails.CreatedById == tourCompanyUserId
                    && !b.IsDeleted
                    && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    && b.CreatedAt >= startDate
                    && b.CreatedAt < endDate)
                .ToListAsync();

            var totalRevenueBeforeTax = monthlyBookings.Sum(b => b.TotalPrice);
            var totalRevenueAfterTax = totalRevenueBeforeTax * 0.8m; // 80% after 20% deduction (10% commission + 10% VAT)
            var confirmedBookings = monthlyBookings.Count;
            var bookedSlots = monthlyBookings.Sum(b => b.NumberOfGuests);

            return (totalRevenueBeforeTax, totalRevenueAfterTax, confirmedBookings, bookedSlots);
        }

        /// <summary>
        /// Lấy số booking đang chờ transfer (đủ điều kiện chuyển tiền)
        /// </summary>
        public async Task<(int count, decimal amount)> GetTourCompanyPendingTransferStatsAsync(Guid tourCompanyUserId)
        {
            var currentTime = DateTime.UtcNow;
            
            var pendingBookings = await _context.TourBookings
                .Where(b => b.TourOperation.TourDetails.CreatedById == tourCompanyUserId
                    && !b.IsDeleted
                    && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    && b.RevenueHold > 0
                    && !b.RevenueTransferredDate.HasValue
                    && b.TourSlot != null
                    && b.TourSlot.TourDate.ToDateTime(TimeOnly.MinValue).AddDays(3) <= currentTime)
                .ToListAsync();

            return (pendingBookings.Count, pendingBookings.Sum(b => b.RevenueHold));
        }

        /// <summary>
        /// Lấy danh sách bookings chi tiết trong tháng của tour company
        /// </summary>
        public async Task<List<(Guid BookingId, string BookingCode, string TourTitle, DateOnly TourDate, int GuestCount, 
            decimal TotalPayment, decimal RevenueHold, bool IsTransferred, DateTime? TransferredDate, 
            string BookingStatus, DateTime BookingCreatedAt)>> 
            GetTourCompanyMonthlyBookingsAsync(Guid tourCompanyUserId, DateTime startDate, DateTime endDate)
        {
            var bookingsData = await _context.TourBookings
                .Include(b => b.TourOperation)
                    .ThenInclude(to => to.TourDetails)
                .Include(b => b.TourSlot)
                .Where(b => b.TourOperation.TourDetails.CreatedById == tourCompanyUserId
                    && !b.IsDeleted
                    && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    && b.CreatedAt >= startDate
                    && b.CreatedAt < endDate)
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    BookingId = b.Id,
                    BookingCode = b.BookingCode,
                    TourTitle = b.TourOperation.TourDetails.Title,
                    TourDate = b.TourSlot != null ? b.TourSlot.TourDate : DateOnly.MinValue,
                    GuestCount = b.NumberOfGuests,
                    TotalPayment = b.TotalPrice,
                    RevenueHold = b.RevenueHold,
                    IsTransferred = b.RevenueTransferredDate.HasValue,
                    TransferredDate = b.RevenueTransferredDate,
                    BookingStatus = b.Status.ToString(),
                    BookingCreatedAt = b.CreatedAt
                })
                .ToListAsync();

            return bookingsData.Select(b => (
                BookingId: b.BookingId,
                BookingCode: b.BookingCode,
                TourTitle: b.TourTitle,
                TourDate: b.TourDate,
                GuestCount: b.GuestCount,
                TotalPayment: b.TotalPayment,
                RevenueHold: b.RevenueHold,
                IsTransferred: b.IsTransferred,
                TransferredDate: b.TransferredDate,
                BookingStatus: b.BookingStatus,
                BookingCreatedAt: b.BookingCreatedAt
            )).ToList();
        }

        /// <summary>
        /// Lấy thống kê tổng hệ thống cho admin overview
        /// </summary>
        public async Task<(decimal totalRevenueHold, decimal totalWalletBalance, int totalBookings)> 
            GetSystemTourRevenueOverviewAsync(DateTime startDate, DateTime endDate)
        {
            // Total revenue hold across all tour companies
            var totalRevenueHold = await _context.TourBookings
                .Where(b => !b.IsDeleted
                    && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    && b.RevenueHold > 0
                    && !b.RevenueTransferredDate.HasValue)
                .SumAsync(b => (decimal?)b.RevenueHold) ?? 0;

            // Total wallet balance across all tour companies
            var totalWalletBalance = await _context.TourCompanies
                .Where(tc => !tc.IsDeleted && tc.IsActive)
                .SumAsync(tc => (decimal?)tc.Wallet) ?? 0;

            // Total bookings in the month
            var totalBookings = await _context.TourBookings
                .Where(b => !b.IsDeleted
                    && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    && b.CreatedAt >= startDate
                    && b.CreatedAt < endDate)
                .CountAsync();

            return (totalRevenueHold, totalWalletBalance, totalBookings);
        }

        /// <summary>
        /// Lấy thông tin chi tiết một tour company
        /// </summary>
        public async Task<(Guid TourCompanyId, Guid UserId, string CompanyName, string Email, decimal Wallet, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt)?> 
            GetTourCompanyByIdAsync(Guid tourCompanyId)
        {
            var companyData = await _context.TourCompanies
                .Include(tc => tc.User)
                .Where(tc => tc.Id == tourCompanyId && !tc.IsDeleted)
                .Select(tc => new
                {
                    TourCompanyId = tc.Id,
                    UserId = tc.UserId,
                    CompanyName = tc.CompanyName,
                    Email = tc.User.Email,
                    Wallet = tc.Wallet,
                    IsActive = tc.IsActive,
                    CreatedAt = tc.CreatedAt,
                    UpdatedAt = tc.UpdatedAt ?? tc.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (companyData == null)
                return null;

            return (
                TourCompanyId: companyData.TourCompanyId,
                UserId: companyData.UserId,
                CompanyName: companyData.CompanyName,
                Email: companyData.Email,
                Wallet: companyData.Wallet,
                IsActive: companyData.IsActive,
                CreatedAt: companyData.CreatedAt,
                UpdatedAt: companyData.UpdatedAt
            );
        }

        // New methods for enhanced tour company statistics

        /// <summary>
        /// Lấy thống kê tours của tour company
        /// </summary>
        public async Task<(int totalCreated, int approved, int publicTours, int pending, int rejected, int withRevenue)> 
            GetTourCompanyTourStatsAsync(Guid tourCompanyUserId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var tourDetailsQuery = _context.TourDetails
                .Where(td => td.CreatedById == tourCompanyUserId && !td.IsDeleted);

            var totalCreated = await tourDetailsQuery.CountAsync();
            
            // Count tours by status
            var approved = await tourDetailsQuery.CountAsync(td => td.Status == TourDetailsStatus.Approved || td.Status == TourDetailsStatus.Public);
            var pending = await tourDetailsQuery.CountAsync(td => 
                td.Status == TourDetailsStatus.Pending || 
                td.Status == TourDetailsStatus.AwaitingAdminApproval ||
                td.Status == TourDetailsStatus.AwaitingGuideAssignment ||
                td.Status == TourDetailsStatus.WaitToPublic);
            var rejected = await tourDetailsQuery.CountAsync(td => td.Status == TourDetailsStatus.Rejected);

            // Public tours are those with Public status (can be booked by customers)
            var publicTours = await tourDetailsQuery.CountAsync(td => td.Status == TourDetailsStatus.Public);

            // Tours with revenue in the specified period
            int withRevenue = 0;
            if (startDate.HasValue && endDate.HasValue)
            {
                withRevenue = await _context.TourBookings
                    .Where(b => b.TourOperation.TourDetails.CreatedById == tourCompanyUserId
                        && !b.IsDeleted
                        && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                        && b.CreatedAt >= startDate.Value
                        && b.CreatedAt < endDate.Value)
                    .Select(b => b.TourOperation.TourDetailsId)
                    .Distinct()
                    .CountAsync();
            }

            return (totalCreated, approved, publicTours, pending, rejected, withRevenue);
        }

        /// <summary>
        /// Lấy danh sách tours với revenue details trong tháng
        /// </summary>
        public async Task<List<(Guid TourDetailsId, string TourTitle, string TourStatus, DateTime TourCreatedAt, bool IsPublic,
            int MonthlyBookingCount, decimal MonthlyRevenueBeforeTax, decimal MonthlyRevenueAfterTax,
            decimal RevenueHold, decimal RevenueTransferred, int TotalGuestsCount, decimal? CurrentPrice, int AvailableSlots)>>
            GetTourCompanyTourRevenueDetailsAsync(Guid tourCompanyUserId, DateTime startDate, DateTime endDate)
        {
            // Get all tour details for this company
            var tourDetails = await _context.TourDetails
                .Include(td => td.TourOperation)
                .Where(td => td.CreatedById == tourCompanyUserId && !td.IsDeleted)
                .ToListAsync();

            var result = new List<(Guid, string, string, DateTime, bool, int, decimal, decimal, decimal, decimal, int, decimal?, int)>();

            foreach (var tour in tourDetails)
            {
                // Check if tour is public (has Public status and can be booked)
                var isPublic = tour.Status == TourDetailsStatus.Public;

                // Get bookings for this tour in the specified month
                var monthlyBookings = await _context.TourBookings
                    .Where(b => b.TourOperation.TourDetailsId == tour.Id
                        && !b.IsDeleted
                        && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                        && b.CreatedAt >= startDate
                        && b.CreatedAt < endDate)
                    .ToListAsync();

                var monthlyBookingCount = monthlyBookings.Count;
                var monthlyRevenueBeforeTax = monthlyBookings.Sum(b => b.TotalPrice);
                var monthlyRevenueAfterTax = monthlyRevenueBeforeTax * 0.8m; // 80% after deductions
                var revenueHold = monthlyBookings.Where(b => !b.RevenueTransferredDate.HasValue).Sum(b => b.RevenueHold);
                var revenueTransferred = monthlyBookings.Where(b => b.RevenueTransferredDate.HasValue).Sum(b => b.TotalPrice * 0.8m);
                var totalGuestsCount = monthlyBookings.Sum(b => b.NumberOfGuests);

                // Get current price and available slots from active operations
                var activeOperation = tour.TourOperation;
                var currentPrice = activeOperation?.Price;
                
                // Count available slots
                var availableSlots = 0;
                if (activeOperation != null && activeOperation.IsActive && !activeOperation.IsDeleted)
                {
                    var bookedSlots = await _context.TourBookings
                        .Where(b => b.TourOperationId == activeOperation.Id 
                            && !b.IsDeleted 
                            && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed))
                        .SumAsync(b => b.NumberOfGuests);
                    
                    availableSlots = Math.Max(0, activeOperation.MaxGuests - bookedSlots);
                }

                result.Add((
                    tour.Id,
                    tour.Title,
                    tour.Status.ToString(),
                    tour.CreatedAt,
                    isPublic,
                    monthlyBookingCount,
                    monthlyRevenueBeforeTax,
                    monthlyRevenueAfterTax,
                    revenueHold,
                    revenueTransferred,
                    totalGuestsCount,
                    currentPrice,
                    availableSlots
                ));
            }

            return result.OrderByDescending(r => r.Item7).ToList(); // Sort by MonthlyRevenueBeforeTax (7th item)
        }

        /// <summary>
        /// Lấy thông tin chi tiết về revenue transfer của một tour cụ thể
        /// </summary>
        public async Task<(int transferredCount, int pendingCount, int eligibleCount, DateTime? earliestTransfer, 
            DateTime? latestTransfer, double averageTransferDays, double transferRate,
            List<(Guid BookingId, string BookingCode, DateOnly TourDate, DateTime TourCompletionDate, 
                DateTime EligibleTransferDate, DateTime? ActualTransferDate, bool IsTransferred, 
                bool IsEligible, int DaysSinceCompletion, decimal RevenueAmount, string TransferStatus)> bookingTransfers)>
            GetTourRevenueTransferDetailsAsync(Guid tourDetailsId, DateTime startDate, DateTime endDate)
        {
            var bookings = await _context.TourBookings
                .Include(b => b.TourSlot)
                .Where(b => b.TourOperation.TourDetailsId == tourDetailsId
                    && !b.IsDeleted
                    && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Completed)
                    && b.CreatedAt >= startDate
                    && b.CreatedAt < endDate)
                .ToListAsync();

            var currentTime = DateTime.UtcNow;
            var bookingTransfers = new List<(Guid, string, DateOnly, DateTime, DateTime, DateTime?, bool, bool, int, decimal, string)>();

            foreach (var booking in bookings)
            {
                var tourDate = booking.TourSlot?.TourDate ?? DateOnly.FromDateTime(booking.CreatedAt);
                var tourCompletionDate = tourDate.ToDateTime(TimeOnly.MinValue).AddDays(1); // Tour completion = tour date + 1 day
                var eligibleTransferDate = tourCompletionDate.AddDays(3); // Eligible after 3 days
                var isTransferred = booking.RevenueTransferredDate.HasValue;
                var isEligible = currentTime >= eligibleTransferDate;
                var daysSinceCompletion = (int)(currentTime - tourCompletionDate).TotalDays;
                var revenueAmount = booking.TotalPrice * 0.8m; // 80% after deductions

                // Determine transfer status
                string transferStatus;
                if (isTransferred)
                {
                    transferStatus = "Transferred";
                }
                else if (isEligible)
                {
                    transferStatus = "Eligible";
                }
                else if (daysSinceCompletion >= 0)
                {
                    transferStatus = "Pending";
                }
                else
                {
                    transferStatus = "NotEligible";
                }

                bookingTransfers.Add((
                    booking.Id,
                    booking.BookingCode,
                    tourDate,
                    tourCompletionDate,
                    eligibleTransferDate,
                    booking.RevenueTransferredDate,
                    isTransferred,
                    isEligible,
                    daysSinceCompletion,
                    revenueAmount,
                    transferStatus
                ));
            }

            // Calculate statistics
            var transferredCount = bookingTransfers.Count(bt => bt.Item7); // IsTransferred
            var pendingCount = bookingTransfers.Count(bt => !bt.Item7); // Not transferred
            var eligibleCount = bookingTransfers.Count(bt => bt.Item8 && !bt.Item7); // Eligible but not transferred

            var transferredBookings = bookingTransfers.Where(bt => bt.Item7 && bt.Item6.HasValue).ToList();
            var earliestTransfer = transferredBookings.Any() ? transferredBookings.Min(bt => bt.Item6) : null;
            var latestTransfer = transferredBookings.Any() ? transferredBookings.Max(bt => bt.Item6) : null;

            // Calculate average transfer days (from tour completion to actual transfer)
            var averageTransferDays = 0.0;
            if (transferredBookings.Any())
            {
                var transferDays = transferredBookings
                    .Where(bt => bt.Item6.HasValue)
                    .Select(bt => (bt.Item6!.Value - bt.Item4).TotalDays)
                    .ToList();
                
                averageTransferDays = transferDays.Any() ? transferDays.Average() : 0.0;
            }

            // Calculate transfer completion rate
            var transferRate = bookingTransfers.Any() ? (double)transferredCount / bookingTransfers.Count * 100 : 0.0;

            return (transferredCount, pendingCount, eligibleCount, earliestTransfer, latestTransfer, 
                averageTransferDays, transferRate, bookingTransfers);
        }
    }
}
