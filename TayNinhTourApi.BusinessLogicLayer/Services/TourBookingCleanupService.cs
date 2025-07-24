using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Background service để tự động cleanup các booking expired (quá thời gian ReservedUntil)
    /// </summary>
    public class TourBookingCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TourBookingCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Chạy mỗi 5 phút

        public TourBookingCleanupService(
            IServiceProvider serviceProvider,
            ILogger<TourBookingCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TourBookingCleanupService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredBookingsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while cleaning up expired bookings");
                }

                // Đợi 5 phút trước khi chạy lần tiếp theo
                await Task.Delay(_cleanupInterval, stoppingToken);
            }

            _logger.LogInformation("TourBookingCleanupService stopped");
        }

        private async Task CleanupExpiredBookingsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                var now = DateTime.UtcNow;
                
                // Tìm các booking expired:
                // - Status = Pending
                // - ReservedUntil < now (đã hết hạn)
                // - Chưa bị xóa
                var expiredBookings = await unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.Status == BookingStatus.Pending 
                             && b.ReservedUntil.HasValue 
                             && b.ReservedUntil.Value < now
                             && !b.IsDeleted)
                    .Include(b => b.TourOperation)
                    .ToListAsync();

                if (!expiredBookings.Any())
                {
                    _logger.LogDebug("No expired bookings found");
                    return;
                }

                _logger.LogInformation("Found {Count} expired bookings to cleanup", expiredBookings.Count);

                using var transaction = await unitOfWork.BeginTransactionAsync();
                
                foreach (var booking in expiredBookings)
                {
                    try
                    {
                        // Cancel booking
                        booking.Status = BookingStatus.CancelledByCustomer;
                        booking.CancelledDate = DateTime.UtcNow;
                        booking.CancellationReason = "Tự động hủy do hết thời gian thanh toán";
                        booking.UpdatedAt = DateTime.UtcNow;
                        booking.ReservedUntil = null;

                        await unitOfWork.TourBookingRepository.UpdateAsync(booking);

                        // ❌ KHÔNG trừ TourOperation.CurrentBookings vì chưa từng được cộng
                        // Chỉ release TourSlot capacity
                        // Release capacity
                        // if (booking.TourOperation != null)
                        // {
                        //     booking.TourOperation.CurrentBookings = Math.Max(0, 
                        //         booking.TourOperation.CurrentBookings - booking.NumberOfGuests);
                        //     await unitOfWork.TourOperationRepository.UpdateAsync(booking.TourOperation);
                        // }

                        // ✅ CHỈ release TourSlot capacity
                        if (booking.TourSlotId.HasValue)
                        {
                            var tourSlotService = scope.ServiceProvider.GetRequiredService<ITourSlotService>();
                            await tourSlotService.ReleaseSlotCapacityAsync(booking.TourSlotId.Value, booking.NumberOfGuests);
                        }

                        _logger.LogInformation("Cancelled expired booking {BookingCode} for {Guests} guests", 
                            booking.BookingCode, booking.NumberOfGuests);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error cancelling expired booking {BookingId}", booking.Id);
                    }
                }

                await unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Successfully cleaned up {Count} expired bookings", expiredBookings.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cleanup process");
            }
        }
    }
}
