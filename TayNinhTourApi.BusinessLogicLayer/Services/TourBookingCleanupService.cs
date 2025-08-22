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
    /// Background service để tự động cleanup các booking expired và ẩn các booking cũ
    /// 1. Cleanup expired bookings (quá thời gian ReservedUntil) - chạy mỗi 5 phút
    /// 2. Hide old bookings (Pending/Cancelled sau 3 ngày) - chạy mỗi 6 giờ
    /// </summary>
    public class TourBookingCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TourBookingCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Chạy mỗi 5 phút
        private readonly TimeSpan _hideOldBookingsInterval = TimeSpan.FromHours(6); // Chạy mỗi 6 giờ
        private readonly TimeSpan _hideAfterDays = TimeSpan.FromDays(3); // Ẩn sau 3 ngày

        private DateTime _lastHideOldBookingsRun = DateTime.MinValue;

        public TourBookingCleanupService(
            IServiceProvider serviceProvider,
            ILogger<TourBookingCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TourBookingCleanupService started - Cleanup expired bookings (5min) + Hide old bookings (6h)");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Job 1: Cleanup expired bookings (chạy mỗi 5 phút)
                    await CleanupExpiredBookingsAsync();

                    // Job 2: Hide old bookings (chạy mỗi 6 giờ)
                    var now = DateTime.UtcNow;
                    if (now - _lastHideOldBookingsRun >= _hideOldBookingsInterval)
                    {
                        await HideOldBookingsAsync();
                        _lastHideOldBookingsRun = now;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in TourBookingCleanupService");
                }

                // Đợi 5 phút trước khi chạy lần tiếp theo
                try
                {
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("TourBookingCleanupService stopped");
        }

        /// <summary>
        /// Cleanup các booking expired (quá thời gian ReservedUntil)
        /// </summary>
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

                // ✅ Sử dụng execution strategy để handle retry logic với transactions
                var executionStrategy = unitOfWork.GetExecutionStrategy();

                await executionStrategy.ExecuteAsync(async () =>
                {
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
                });

                _logger.LogInformation("Successfully cleaned up {Count} expired bookings", expiredBookings.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during expired bookings cleanup process");
            }
        }

        /// <summary>
        /// Ẩn các TourBooking có status Pending hoặc Cancelled đã quá 3 ngày
        /// Cùng với việc ẩn tất cả TourBookingGuest liên quan
        /// </summary>
        private async Task HideOldBookingsAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            try
            {
                var cutoffDate = DateTime.UtcNow.Subtract(_hideAfterDays);
                
                _logger.LogDebug("Checking for tour bookings to hide - cutoff date: {CutoffDate}", cutoffDate);

                // Tìm các TourBooking cần ẩn:
                // - Status = Pending hoặc CancelledByCustomer hoặc CancelledByCompany
                // - CreatedAt < cutoffDate (đã quá 3 ngày)
                // - Chưa bị ẩn (IsDeleted = false)
                var bookingsToHide = await unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => (b.Status == BookingStatus.Pending || 
                                b.Status == BookingStatus.CancelledByCustomer || 
                                b.Status == BookingStatus.CancelledByCompany)
                             && b.CreatedAt < cutoffDate
                             && !b.IsDeleted)
                    .Include(b => b.Guests.Where(g => !g.IsDeleted)) // Include guests chưa bị ẩn
                    .ToListAsync();

                if (!bookingsToHide.Any())
                {
                    _logger.LogDebug("No old tour bookings found that need to be hidden");
                    return;
                }

                _logger.LogInformation("Found {Count} old tour bookings to hide (Pending/Cancelled bookings older than 3 days)", bookingsToHide.Count);

                // Sử dụng execution strategy để handle retry logic với transactions
                var executionStrategy = unitOfWork.GetExecutionStrategy();

                await executionStrategy.ExecuteAsync(async () =>
                {
                    using var transaction = await unitOfWork.BeginTransactionAsync();

                    int hiddenBookingsCount = 0;
                    int hiddenGuestsCount = 0;

                    foreach (var booking in bookingsToHide)
                    {
                        try
                        {
                            // Ẩn TourBooking
                            booking.IsDeleted = true;
                            booking.UpdatedAt = DateTime.UtcNow;
                            await unitOfWork.TourBookingRepository.UpdateAsync(booking);
                            hiddenBookingsCount++;

                            // Ẩn tất cả TourBookingGuest liên quan
                            foreach (var guest in booking.Guests.Where(g => !g.IsDeleted))
                            {
                                guest.IsDeleted = true;
                                guest.UpdatedAt = DateTime.UtcNow;
                                
                                // Cần update guest thông qua context vì không có GuestRepository
                                unitOfWork.Context.Update(guest);
                                hiddenGuestsCount++;
                            }

                            var daysOld = (DateTime.UtcNow - booking.CreatedAt).Days;
                            _logger.LogDebug("Hidden booking {BookingId} (Code: {BookingCode}, Status: {Status}, Age: {Days} days) with {GuestCount} guests",
                                booking.Id, booking.BookingCode, booking.Status, daysOld, booking.Guests.Count);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error hiding tour booking {BookingId}", booking.Id);
                        }
                    }

                    if (hiddenBookingsCount > 0)
                    {
                        await unitOfWork.SaveChangesAsync();
                        await transaction.CommitAsync();
                        _logger.LogInformation("Successfully hidden {BookingCount} old tour bookings and {GuestCount} guests from frontend display", 
                            hiddenBookingsCount, hiddenGuestsCount);
                    }
                    else
                    {
                        await transaction.RollbackAsync();
                        _logger.LogWarning("No tour bookings were successfully hidden");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during old tour bookings cleanup process");
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TourBookingCleanupService is stopping");
            await base.StopAsync(stoppingToken);
        }

        public override void Dispose()
        {
            _logger.LogInformation("TourBookingCleanupService disposed");
            base.Dispose();
        }
    }
}
