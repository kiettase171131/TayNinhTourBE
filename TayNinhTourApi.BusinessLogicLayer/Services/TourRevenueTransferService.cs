using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Utilities;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Background service t? ??ng chuy?n ti?n t? booking revenue hold sang wallet sau 3 ngày
    /// Service ch?y m?i 1 gi? ?? ki?m tra và chuy?n ti?n cho các booking ?? ?i?u ki?n
    /// </summary>
    public class TourRevenueTransferService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TourRevenueTransferService> _logger;

        // Check every hour for eligible revenue transfers
        private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

        public TourRevenueTransferService(
            IServiceProvider serviceProvider,
            ILogger<TourRevenueTransferService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TourRevenueTransferService started. Will check for eligible revenue transfers every {Interval}", CheckInterval);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessEligibleRevenueTransfersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing revenue transfers");
                }

                // Wait for next check
                await Task.Delay(CheckInterval, stoppingToken);
            }
        }

        /// <summary>
        /// Process all bookings eligible for revenue transfer (tour completed + 3 days)
        /// </summary>
        private async Task ProcessEligibleRevenueTransfersAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var revenueService = scope.ServiceProvider.GetRequiredService<ITourRevenueService>();
                var tourCompanyNotificationService = scope.ServiceProvider.GetRequiredService<ITourCompanyNotificationService>();

                // Get all bookings eligible for revenue transfer
                var eligibleBookings = await revenueService.GetBookingsEligibleForRevenueTransferAsync();

                if (!eligibleBookings.Any())
                {
                    _logger.LogDebug("No bookings eligible for revenue transfer at {Time}", VietnamTimeZoneUtility.GetVietnamNow());
                    return;
                }

                _logger.LogInformation("Found {Count} bookings eligible for revenue transfer", eligibleBookings.Count);

                int successCount = 0;
                int failureCount = 0;

                foreach (var booking in eligibleBookings)
                {
                    try
                    {
                        // Transfer revenue from booking to tour company wallet
                        var result = await revenueService.TransferBookingRevenueToWalletAsync(booking.Id);

                        if (result.success)
                        {
                            successCount++;
                            _logger.LogInformation("Successfully transferred revenue for booking {BookingCode}: {TransferAmount} VN? (after 10% commission from {FullAmount} VN?)", 
                                booking.BookingCode, booking.RevenueHold * 0.9m, booking.RevenueHold);

                            // Send notification to tour company about revenue transfer
                            if (booking.TourOperation?.TourDetails?.CreatedById != null)
                            {
                                try
                                {
                                    var tourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? VietnamTimeZoneUtility.GetVietnamNow();
                                    var tourTitle = booking.TourOperation?.TourDetails?.Title ?? "Tour";
                                    var transferAmount = booking.RevenueHold * 0.9m; // 90% after commission
                                    
                                    await tourCompanyNotificationService.NotifyRevenueTransferAsync(
                                        booking.TourOperation.TourDetails.CreatedById,
                                        transferAmount,
                                        tourTitle,
                                        tourDate
                                    );
                                    
                                    _logger.LogDebug("Revenue transfer notification sent for booking {BookingCode}", booking.BookingCode);
                                }
                                catch (Exception notifEx)
                                {
                                    _logger.LogWarning(notifEx, "Failed to send revenue transfer notification for booking {BookingCode}", booking.BookingCode);
                                }
                            }
                        }
                        else
                        {
                            failureCount++;
                            _logger.LogWarning("Failed to transfer revenue for booking {BookingCode}: {Message}", 
                                booking.BookingCode, result.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        _logger.LogError(ex, "Error transferring revenue for booking {BookingCode}", booking.BookingCode);
                    }
                }

                if (successCount > 0 || failureCount > 0)
                {
                    _logger.LogInformation("Revenue transfer batch completed. Success: {Success}, Failures: {Failures}", 
                        successCount, failureCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ProcessEligibleRevenueTransfersAsync");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TourRevenueTransferService is stopping");
            await base.StopAsync(stoppingToken);
        }
    }
}