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
        Task<int> GetNewAccountsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetBookingsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetOrdersAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetWithdrawRequestsTotalAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetWithdrawRequestsAcceptAsync(DateTime startDate, DateTime endDate);
        Task<int> GetNewCVsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetNewShopsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetPostsAsync(DateTime startDate, DateTime endDate);
        Task<List<(Guid ShopId, decimal Revenue, decimal RevenueTax)>> GetTotalRevenueByShopAsync(DateTime startDate, DateTime endDate);
        


        Task<List<(TourDetailsStatus Status, int Count)>> GetGroupedTourDetailsAsync();
        Task<int> GetTotalPostsAsync(Guid bloggerId,int month, int year);
        Task<int> GetApprovedPostsAsync(Guid bloggerId, int month, int year);
        Task<int> GetRejectedPostsAsync(Guid bloggerId, int month, int year);
        Task<int> GetPendingPostsAsync(Guid bloggerId, int month, int year);
        Task<int> GetTotalLikesAsync(Guid bloggerId, int month, int year);
        Task<int> GetTotalCommentsAsync(Guid bloggerId, int month, int year);
        Task<int> GetTotalProductsAsync(Guid shopId);
        Task<int> GetTotalOrdersAsync(Guid shopId, DateTime startDate, DateTime endDate);
        Task<(decimal Revenue, decimal RevenueTax)> GetTotalRevenueAsync(Guid shopId, DateTime startDate, DateTime endDate);
        Task<decimal> GetWalletAsync(Guid shopId);
        Task<(decimal averageRating, int totalRatings)> GetProductRatingsAsync(Guid shopId, DateTime startDate, DateTime endDate);
        Task<decimal?> GetShopRatingAsync(Guid shopId);
        Task<int> CountConfirmedBookingsByUserIdAsync(Guid userId);
        Task<(decimal RevenueHold, decimal Wallet)> GetWalletInfoAsync(Guid userId);

        // New methods for tour company revenue statistics
        
        /// <summary>
        /// Lấy danh sách tour companies với thông tin cơ bản và phân trang
        /// </summary>
        Task<(List<(Guid TourCompanyId, Guid UserId, string CompanyName, string Email, decimal Wallet, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt)> companies, int totalCount)> 
            GetTourCompaniesAsync(int pageIndex, int pageSize, string? searchTerm = null, bool? isActive = null);

        /// <summary>
        /// Lấy tổng revenue hold của tour company từ các bookings chưa transfer
        /// </summary>
        Task<decimal> GetTourCompanyRevenueHoldAsync(Guid tourCompanyUserId);

        /// <summary>
        /// Lấy thống kê bookings trong tháng của tour company
        /// </summary>
        Task<(decimal totalRevenueBeforeTax, decimal totalRevenueAfterTax, int confirmedBookings, int bookedSlots)> 
            GetTourCompanyMonthlyStatsAsync(Guid tourCompanyUserId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Lấy số booking đang chờ transfer (đủ điều kiện chuyển tiền)
        /// </summary>
        Task<(int count, decimal amount)> GetTourCompanyPendingTransferStatsAsync(Guid tourCompanyUserId);

        /// <summary>
        /// Lấy danh sách bookings chi tiết trong tháng của tour company
        /// </summary>
        Task<List<(Guid BookingId, string BookingCode, string TourTitle, DateOnly TourDate, int GuestCount, 
            decimal TotalPayment, decimal RevenueHold, bool IsTransferred, DateTime? TransferredDate, 
            string BookingStatus, DateTime BookingCreatedAt)>> 
            GetTourCompanyMonthlyBookingsAsync(Guid tourCompanyUserId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Lấy thống kê tổng hệ thống cho admin overview
        /// </summary>
        Task<(decimal totalRevenueHold, decimal totalWalletBalance, int totalBookings)> 
            GetSystemTourRevenueOverviewAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Lấy thông tin chi tiết một tour company
        /// </summary>
        Task<(Guid TourCompanyId, Guid UserId, string CompanyName, string Email, decimal Wallet, bool IsActive, DateTime CreatedAt, DateTime UpdatedAt)?> 
            GetTourCompanyByIdAsync(Guid tourCompanyId);

        // New methods for enhanced tour company statistics

        /// <summary>
        /// Lấy thống kê tours của tour company
        /// </summary>
        Task<(int totalCreated, int approved, int publicTours, int pending, int rejected, int withRevenue)> 
            GetTourCompanyTourStatsAsync(Guid tourCompanyUserId, DateTime? startDate = null, DateTime? endDate = null);

        /// <summary>
        /// Lấy danh sách tours với revenue details trong tháng
        /// </summary>
        Task<List<(Guid TourDetailsId, string TourTitle, string TourStatus, DateTime TourCreatedAt, bool IsPublic,
            int MonthlyBookingCount, decimal MonthlyRevenueBeforeTax, decimal MonthlyRevenueAfterTax,
            decimal RevenueHold, decimal RevenueTransferred, int TotalGuestsCount, decimal? CurrentPrice, int AvailableSlots)>>
            GetTourCompanyTourRevenueDetailsAsync(Guid tourCompanyUserId, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Lấy thông tin chi tiết về revenue transfer của một tour cụ thể
        /// </summary>
        Task<(int transferredCount, int pendingCount, int eligibleCount, DateTime? earliestTransfer, 
            DateTime? latestTransfer, double averageTransferDays, double transferRate,
            List<(Guid BookingId, string BookingCode, DateOnly TourDate, DateTime TourCompletionDate, 
                DateTime EligibleTransferDate, DateTime? ActualTransferDate, bool IsTransferred, 
                bool IsEligible, int DaysSinceCompletion, decimal RevenueAmount, string TransferStatus)> bookingTransfers)>
            GetTourRevenueTransferDetailsAsync(Guid tourDetailsId, DateTime startDate, DateTime endDate);
    }
}
