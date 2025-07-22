using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourBooking;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Background service xử lý auto-cancel tours và revenue transfer
    /// </summary>
    public class TourAutoCancelService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TourAutoCancelService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(6); // Chạy mỗi 6 giờ

        public TourAutoCancelService(
            IServiceProvider serviceProvider,
            ILogger<TourAutoCancelService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TourAutoCancelService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Starting tour auto-cancel and revenue transfer jobs");

                    using var scope = _serviceProvider.CreateScope();
                    
                    // Job 1: Check and cancel under-booked tours
                    await CheckAndCancelUnderBookedToursAsync(scope.ServiceProvider);
                    
                    // Job 2: Transfer matured revenue from hold to wallet
                    await TransferMaturedRevenueAsync(scope.ServiceProvider);

                    _logger.LogInformation("Completed tour auto-cancel and revenue transfer jobs");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in tour auto-cancel and revenue transfer jobs");
                }

                // Wait for next execution
                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("TourAutoCancelService stopped");
        }

        /// <summary>
        /// Kiểm tra và hủy các tours không đủ khách (< 50% capacity) 2 ngày trước khởi hành
        /// </summary>
        private async Task CheckAndCancelUnderBookedToursAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                var revenueService = serviceProvider.GetRequiredService<ITourRevenueService>();
                var notificationService = serviceProvider.GetRequiredService<ITourCompanyNotificationService>();

                var twoDaysFromNow = DateTime.UtcNow.AddDays(2);

                // Tìm các tours khởi hành trong 2 ngày tới
                var upcomingTours = await unitOfWork.TourOperationRepository.GetQueryable()
                    .Where(to => to.IsActive && !to.IsDeleted)
                    .Include(to => to.TourDetails)
                        .ThenInclude(td => td.AssignedSlots)
                    .Where(to => to.TourDetails.Status == TourDetailsStatus.Public)
                    .Where(to => to.TourDetails.AssignedSlots.Any(slot => 
                        slot.TourDate.ToDateTime(TimeOnly.MinValue) <= twoDaysFromNow &&
                        slot.TourDate.ToDateTime(TimeOnly.MinValue) > DateTime.UtcNow))
                    .ToListAsync();

                foreach (var tourOperation in upcomingTours)
                {
                    var bookingRate = (double)tourOperation.CurrentBookings / tourOperation.MaxGuests;
                    
                    if (bookingRate < 0.5) // < 50% capacity
                    {
                        _logger.LogInformation("Cancelling under-booked tour: {TourTitle} (Booking rate: {BookingRate:P})", 
                            tourOperation.TourDetails?.Title, bookingRate);

                        await CancelTourAndRefundAsync(tourOperation, serviceProvider);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAndCancelUnderBookedToursAsync");
            }
        }

        /// <summary>
        /// Hủy tour và hoàn tiền cho khách hàng
        /// </summary>
        private async Task CancelTourAndRefundAsync(
            DataAccessLayer.Entities.TourOperation tourOperation, 
            IServiceProvider serviceProvider)
        {
            using var transaction = await serviceProvider.GetRequiredService<IUnitOfWork>().BeginTransactionAsync();
            try
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                var revenueService = serviceProvider.GetRequiredService<ITourRevenueService>();
                var notificationService = serviceProvider.GetRequiredService<ITourCompanyNotificationService>();

                // 1. Lấy tất cả bookings của tour này
                var bookings = await unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.TourOperationId == tourOperation.Id && 
                               b.Status == BookingStatus.Confirmed && 
                               !b.IsDeleted)
                    .Include(b => b.User)
                    .ToListAsync();

                if (!bookings.Any())
                {
                    _logger.LogInformation("No confirmed bookings found for tour: {TourTitle}", 
                        tourOperation.TourDetails?.Title);
                    return;
                }

                // 2. Update booking status to cancelled
                foreach (var booking in bookings)
                {
                    booking.Status = BookingStatus.CancelledByCompany;
                    booking.CancelledDate = DateTime.UtcNow;
                    booking.CancellationReason = "Tour bị hủy tự động do không đủ khách (< 50% capacity)";
                    booking.UpdatedAt = DateTime.UtcNow;
                    
                    await unitOfWork.TourBookingRepository.UpdateAsync(booking);
                }

                // 3. Update tour operation status
                tourOperation.Status = TourOperationStatus.Cancelled;
                tourOperation.UpdatedAt = DateTime.UtcNow;
                await unitOfWork.TourOperationRepository.UpdateAsync(tourOperation);

                // 4. Calculate amounts for refund
                var totalBookingAmount = bookings.Sum(b => b.TotalPrice);
                var commissionRate = 0.10m;
                var amountInRevenueHold = totalBookingAmount * (1 - commissionRate); // 90% in revenue hold

                // 5. Deduct from TourCompany revenue hold (only the 90% that's actually there)
                await revenueService.RefundFromRevenueHoldAsync(
                    tourOperation.CreatedById, 
                    amountInRevenueHold, 
                    tourOperation.Id);

                // Note: The system will handle full customer refund (100%) from other sources
                // while only deducting the 90% from tour company's revenue hold

                // 6. Send notifications
                var bookingDtos = bookings.Select(b => new TourBookingDto
                {
                    Id = b.Id,
                    BookingCode = b.BookingCode,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.TotalPrice,
                    BookingDate = b.BookingDate,
                    User = new UserSummaryDto
                    {
                        Id = b.User.Id,
                        Name = b.User.Name,
                        Email = b.User.Email,
                        PhoneNumber = b.User.PhoneNumber
                    }
                }).ToList();

                var tourStartDate = tourOperation.TourDetails?.AssignedSlots.Any() == true ? 
                    tourOperation.TourDetails.AssignedSlots.Min(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : 
                    DateTime.UtcNow;

                await notificationService.NotifyTourCancellationAsync(
                    tourOperation.CreatedById,
                    bookingDtos,
                    tourOperation.TourDetails?.Title ?? "Unknown Tour",
                    tourStartDate,
                    "Không đủ khách (< 50% capacity)");

                // 7. Send cancellation emails to customers
                // This would be implemented separately or integrated with existing email service

                await unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully cancelled tour: {TourTitle} with {BookingCount} bookings, deducted {DeductedAmount} from revenue hold (after 10% commission)", 
                    tourOperation.TourDetails?.Title, bookings.Count, amountInRevenueHold);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling tour: {TourTitle}", tourOperation.TourDetails?.Title);
            }
        }

        /// <summary>
        /// Chuyển tiền từ revenue hold sang wallet cho các tours đã hoàn thành 3 ngày
        /// </summary>
        private async Task TransferMaturedRevenueAsync(IServiceProvider serviceProvider)
        {
            try
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                var revenueService = serviceProvider.GetRequiredService<ITourRevenueService>();
                var notificationService = serviceProvider.GetRequiredService<ITourCompanyNotificationService>();

                var threeDaysAgo = DateTime.UtcNow.AddDays(-3);

                // Tìm các tours đã hoàn thành 3 ngày trước
                var completedTours = await unitOfWork.TourOperationRepository.GetQueryable()
                    .Where(to => to.Status == TourOperationStatus.Completed && !to.IsDeleted)
                    .Include(to => to.TourDetails)
                        .ThenInclude(td => td.AssignedSlots)
                    .Where(to => to.TourDetails.AssignedSlots.Any(slot => 
                        slot.TourDate.ToDateTime(TimeOnly.MinValue) <= threeDaysAgo))
                    .ToListAsync();

                foreach (var tourOperation in completedTours)
                {
                    // Tính tổng tiền từ các bookings confirmed của tour này
                    var confirmedBookings = await unitOfWork.TourBookingRepository.GetQueryable()
                        .Where(b => b.TourOperationId == tourOperation.Id && 
                                   b.Status == BookingStatus.Confirmed && 
                                   !b.IsDeleted)
                        .ToListAsync();

                    if (!confirmedBookings.Any()) continue;

                    var totalBookingAmount = confirmedBookings.Sum(b => b.TotalPrice);
                    var tourCompletedDate = tourOperation.TourDetails?.AssignedSlots.Any() == true ? 
                        tourOperation.TourDetails.AssignedSlots.Max(s => s.TourDate).ToDateTime(TimeOnly.MinValue) : 
                        DateTime.UtcNow;

                    // Calculate the amount in revenue hold (90% of total booking amount due to 10% commission)
                    var commissionRate = 0.10m;
                    var availableAmountInHold = totalBookingAmount * (1 - commissionRate);

                    // Transfer from revenue hold to wallet (only the 90% that's actually in the hold)
                    var transferResult = await revenueService.TransferFromHoldToWalletAsync(
                        tourOperation.CreatedById, 
                        availableAmountInHold);

                    if (transferResult.success)
                    {
                        // Send notification with the amount actually transferred (90% of booking)
                        await notificationService.NotifyRevenueTransferAsync(
                            tourOperation.CreatedById,
                            availableAmountInHold,
                            tourOperation.TourDetails?.Title ?? "Unknown Tour",
                            tourCompletedDate);

                        _logger.LogInformation("Transferred revenue for tour: {TourTitle}, amount: {Amount} (after 10% commission deduction)", 
                            tourOperation.TourDetails?.Title, availableAmountInHold);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to transfer revenue for tour: {TourTitle}, reason: {Reason}", 
                            tourOperation.TourDetails?.Title, transferResult.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TransferMaturedRevenueAsync");
            }
        }
    }
}
