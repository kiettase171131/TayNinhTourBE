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

        // Updated revenue calculation rates
        private const decimal PLATFORM_COMMISSION_RATE = 0.10m; // 10% deduction for before tax revenue

        /// <summary>
        /// Tính doanh thu trước thuế theo công thức: giá - (10% × giá)
        /// </summary>
        private decimal CalculateBeforeTaxRevenue(decimal totalPrice)
        {
            return totalPrice - (totalPrice * PLATFORM_COMMISSION_RATE);
        }

        /// <summary>
        /// Tính doanh thu sau thuế theo công thức: giá - (giá/11) - (10% × giá)
        /// </summary>
        private decimal CalculateAfterTaxRevenue(decimal totalPrice)
        {
            return totalPrice - (totalPrice / 11m) - (totalPrice * PLATFORM_COMMISSION_RATE);
        }

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

        /// <summary>
        /// Admin lấy thống kê thu nhập của tất cả tour companies
        /// </summary>
        public async Task<AdminTourCompanyRevenueOverviewDto> GetTourCompanyRevenueStatsAsync(
            int? year = null, 
            int? month = null, 
            int pageIndex = 0, 
            int pageSize = 10, 
            string? searchTerm = null, 
            bool? isActive = null)
        {
            // Set default values if not provided
            var currentDate = DateTime.UtcNow;
            var effectiveYear = year ?? currentDate.Year;
            var effectiveMonth = month ?? currentDate.Month;

            var startDate = new DateTime(effectiveYear, effectiveMonth, 1);
            var endDate = startDate.AddMonths(1);

            // Get tour companies with pagination
            var (companies, totalCount) = await _dashboardRepository.GetTourCompaniesAsync(
                pageIndex, pageSize, searchTerm, isActive);

            var tourCompanyStats = new List<TourCompanyRevenueStatsDto>();

            // Process each tour company
            foreach (var company in companies)
            {
                // Get revenue hold
                var revenueHold = await _dashboardRepository.GetTourCompanyRevenueHoldAsync(company.UserId);

                // Get monthly stats
                var (totalRevenueBeforeTax, totalRevenueAfterTax, confirmedBookings, bookedSlots) =
                    await _dashboardRepository.GetTourCompanyMonthlyStatsAsync(company.UserId, startDate, endDate);

                // Get pending transfer stats
                var (pendingTransferCount, pendingTransferAmount) =
                    await _dashboardRepository.GetTourCompanyPendingTransferStatsAsync(company.UserId);

                // Get monthly bookings details
                var monthlyBookings = await _dashboardRepository.GetTourCompanyMonthlyBookingsAsync(
                    company.UserId, startDate, endDate);

                var bookingDetails = monthlyBookings.Select(b => new TourBookingDetailDto
                {
                    BookingId = b.BookingId,
                    BookingCode = b.BookingCode,
                    TourTitle = b.TourTitle,
                    TourDate = b.TourDate,
                    GuestCount = b.GuestCount,
                    TotalPayment = b.TotalPayment,
                    RevenueHold = b.RevenueHold,
                    IsTransferredToWallet = b.IsTransferred,
                    RevenueTransferredDate = b.TransferredDate,
                    BookingStatus = b.BookingStatus,
                    BookingCreatedAt = b.BookingCreatedAt
                }).ToList();

                var stats = new TourCompanyRevenueStatsDto
                {
                    TourCompanyId = company.TourCompanyId,
                    UserId = company.UserId,
                    CompanyName = company.CompanyName,
                    Email = company.Email,
                    CurrentWalletBalance = company.Wallet,
                    TotalRevenueHold = revenueHold,
                    MonthlyRevenueBeforeTax = totalRevenueBeforeTax,
                    MonthlyRevenueAfterTax = totalRevenueAfterTax,
                    MonthlyConfirmedBookings = confirmedBookings,
                    PendingTransferBookings = pendingTransferCount,
                    PendingTransferAmount = pendingTransferAmount,
                    MonthlyBookedSlots = bookedSlots,
                    MonthlyBookings = bookingDetails,
                    IsActive = company.IsActive,
                    CreatedAt = company.CreatedAt,
                    LastUpdated = company.UpdatedAt
                };

                tourCompanyStats.Add(stats);
            }

            // Get system overview stats
            var (totalSystemRevenueHold, totalSystemWallet, totalSystemBookings) = 
                await _dashboardRepository.GetSystemTourRevenueOverviewAsync(startDate, endDate);

            // Calculate totals with updated formula
            var totalBeforeTax = tourCompanyStats.Sum(tc => tc.MonthlyRevenueBeforeTax);
            var totalAfterTax = tourCompanyStats.Sum(tc => tc.MonthlyRevenueAfterTax);
            
            // Calculate total deductions based on the new formula
            var totalDeductionsBeforeTax = totalBeforeTax * PLATFORM_COMMISSION_RATE / (1 - PLATFORM_COMMISSION_RATE); // Calculate original payments back
            var totalDeductionsAfterTax = totalAfterTax - totalBeforeTax; // Additional deductions in after-tax calculation

            // Calculate pagination info
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            return new AdminTourCompanyRevenueOverviewDto
            {
                TourCompanies = tourCompanyStats,
                TotalTourCompanies = totalCount,
                ActiveTourCompanies = tourCompanyStats.Count(tc => tc.IsActive),
                TotalSystemRevenueBeforeTax = totalBeforeTax,
                TotalSystemRevenueAfterTax = totalAfterTax,
                TotalPlatformCommission = totalDeductionsBeforeTax, // 10% deduction for before tax
                TotalVAT = Math.Abs(totalDeductionsAfterTax), // Additional deductions for after tax
                TotalMonthlyBookings = totalSystemBookings,
                TotalRevenueHold = totalSystemRevenueHold,
                TotalWalletBalance = totalSystemWallet,
                Month = effectiveMonth,
                Year = effectiveYear,
                GeneratedAt = DateTime.UtcNow,
                Pagination = new PaginationInfo
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = totalPages,
                    HasNext = pageIndex < totalPages - 1,
                    HasPrevious = pageIndex > 0
                }
            };
        }

        /// <summary>
        /// Admin lấy thống kê chi tiết của một tour company cụ thể
        /// </summary>
        public async Task<TourCompanyRevenueStatsDto?> GetTourCompanyRevenueDetailAsync(
            Guid tourCompanyId, 
            int? year = null, 
            int? month = null)
        {
            // Set default values if not provided
            var currentDate = DateTime.UtcNow;
            var effectiveYear = year ?? currentDate.Year;
            var effectiveMonth = month ?? currentDate.Month;

            var startDate = new DateTime(effectiveYear, effectiveMonth, 1);
            var endDate = startDate.AddMonths(1);

            // Get tour company info
            var companyInfo = await _dashboardRepository.GetTourCompanyByIdAsync(tourCompanyId);
            if (companyInfo == null)
                return null;

            // Get revenue hold
            var revenueHold = await _dashboardRepository.GetTourCompanyRevenueHoldAsync(companyInfo.Value.UserId);

            // Get monthly stats
            var (totalRevenueBeforeTax, totalRevenueAfterTax, confirmedBookings, bookedSlots) = 
                await _dashboardRepository.GetTourCompanyMonthlyStatsAsync(companyInfo.Value.UserId, startDate, endDate);

            // Get pending transfer stats
            var (pendingTransferCount, pendingTransferAmount) = 
                await _dashboardRepository.GetTourCompanyPendingTransferStatsAsync(companyInfo.Value.UserId);

            // Get monthly bookings details
            var monthlyBookings = await _dashboardRepository.GetTourCompanyMonthlyBookingsAsync(
                companyInfo.Value.UserId, startDate, endDate);

            var bookingDetails = monthlyBookings.Select(b => new TourBookingDetailDto
            {
                BookingId = b.BookingId,
                BookingCode = b.BookingCode,
                TourTitle = b.TourTitle,
                TourDate = b.TourDate,
                GuestCount = b.GuestCount,
                TotalPayment = b.TotalPayment,
                RevenueHold = b.RevenueHold,
                IsTransferredToWallet = b.IsTransferred,
                RevenueTransferredDate = b.TransferredDate,
                BookingStatus = b.BookingStatus,
                BookingCreatedAt = b.BookingCreatedAt
            }).ToList();

            // Get tour statistics
            var (totalCreated, approved, publicTours, pending, rejected, withRevenue) = 
                await _dashboardRepository.GetTourCompanyTourStatsAsync(companyInfo.Value.UserId, startDate, endDate);

            var tourStats = new TourStatsDto
            {
                TotalToursCreated = totalCreated,
                ToursApproved = approved,
                ToursPublic = publicTours,
                ToursPending = pending,
                ToursRejected = rejected,
                ToursWithRevenue = withRevenue
            };

            // Get tour revenue details
            var tourRevenueData = await _dashboardRepository.GetTourCompanyTourRevenueDetailsAsync(
                companyInfo.Value.UserId, startDate, endDate);

            var tourRevenueDetails = new List<TourRevenueDetailDto>();

            foreach (var t in tourRevenueData)
            {
                // Get detailed transfer information for this tour
                var (transferredCount, pendingCount, eligibleCount, earliestTransfer, latestTransfer, 
                    averageTransferDays, transferRate, bookingTransfers) = 
                    await _dashboardRepository.GetTourRevenueTransferDetailsAsync(t.TourDetailsId, startDate, endDate);

                var transferInfo = new TourRevenueTransferInfoDto
                {
                    EarliestTransferDate = earliestTransfer,
                    LatestTransferDate = latestTransfer,
                    AverageTransferDays = averageTransferDays,
                    TransferCompletionRate = transferRate,
                    BookingTransfers = bookingTransfers.Select(bt => new BookingTransferDetailDto
                    {
                        BookingId = bt.BookingId,
                        BookingCode = bt.BookingCode,
                        TourDate = bt.TourDate,
                        TourCompletionDate = bt.TourCompletionDate,
                        EligibleTransferDate = bt.EligibleTransferDate,
                        ActualTransferDate = bt.ActualTransferDate,
                        IsTransferred = bt.IsTransferred,
                        IsEligibleForTransfer = bt.IsEligible,
                        DaysSinceTourCompletion = bt.DaysSinceCompletion,
                        RevenueAmount = bt.RevenueAmount,
                        TransferStatus = bt.TransferStatus
                    }).ToList()
                };

                var tourRevenueDetail = new TourRevenueDetailDto
                {
                    TourDetailsId = t.TourDetailsId,
                    TourTitle = t.TourTitle,
                    TourStatus = t.TourStatus,
                    TourCreatedAt = t.TourCreatedAt,
                    IsPublic = t.IsPublic,
                    MonthlyBookingCount = t.MonthlyBookingCount,
                    MonthlyRevenueBeforeTax = t.MonthlyRevenueBeforeTax,
                    MonthlyRevenueAfterTax = t.MonthlyRevenueAfterTax,
                    RevenueHold = t.RevenueHold,
                    RevenueTransferred = t.RevenueTransferred,
                    TransferredBookingCount = transferredCount,
                    PendingTransferBookingCount = pendingCount,
                    EligibleForTransferCount = eligibleCount,
                    TotalGuestsCount = t.TotalGuestsCount,
                    CurrentPrice = t.CurrentPrice,
                    AvailableSlots = t.AvailableSlots,
                    TransferInfo = transferInfo
                };

                tourRevenueDetails.Add(tourRevenueDetail);
            }

            return new TourCompanyRevenueStatsDto
            {
                TourCompanyId = companyInfo.Value.TourCompanyId,
                UserId = companyInfo.Value.UserId,
                CompanyName = companyInfo.Value.CompanyName,
                Email = companyInfo.Value.Email,
                CurrentWalletBalance = companyInfo.Value.Wallet,
                TotalRevenueHold = revenueHold,
                MonthlyRevenueBeforeTax = totalRevenueBeforeTax,
                MonthlyRevenueAfterTax = totalRevenueAfterTax,
                MonthlyConfirmedBookings = confirmedBookings,
                PendingTransferBookings = pendingTransferCount,
                PendingTransferAmount = pendingTransferAmount,
                MonthlyBookedSlots = bookedSlots,
                TourStats = tourStats,
                TourRevenueDetails = tourRevenueDetails,
                MonthlyBookings = bookingDetails,
                IsActive = companyInfo.Value.IsActive,
                CreatedAt = companyInfo.Value.CreatedAt,
                LastUpdated = companyInfo.Value.UpdatedAt
            };
        }
    }
}
