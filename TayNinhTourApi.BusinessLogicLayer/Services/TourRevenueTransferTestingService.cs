using Microsoft.EntityFrameworkCore;
using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.Services.Interface;
using TayNinhTourApi.DataAccessLayer.Enums;
using TayNinhTourApi.DataAccessLayer.UnitOfWork.Interface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace TayNinhTourApi.BusinessLogicLayer.Services
{
    /// <summary>
    /// Service for manual tour revenue transfer operations for testing
    /// </summary>
    public class TourRevenueTransferTestingService : ITourRevenueTransferService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TourRevenueTransferTestingService> _logger;

        public TourRevenueTransferTestingService(
            IUnitOfWork unitOfWork,
            IServiceProvider serviceProvider,
            ILogger<TourRevenueTransferTestingService> logger)
        {
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Transfer revenue from hold to wallet for a specific booking manually for testing
        /// </summary>
        public async Task<BaseResposeDto> TransferBookingRevenueAsync(Guid bookingId)
        {
            try
            {
                _logger.LogInformation("Manual revenue transfer requested for booking {BookingId}", bookingId);

                var revenueService = _serviceProvider.GetRequiredService<ITourRevenueService>();
                
                // Use existing TransferBookingRevenueToWalletAsync method
                var result = await revenueService.TransferBookingRevenueToWalletAsync(bookingId);

                _logger.LogInformation("Revenue transfer result for booking {BookingId}: Success={Success}", 
                    bookingId, result.success);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual revenue transfer for booking {BookingId}", bookingId);
                return new BaseResposeDto
                {
                    StatusCode = 500,
                    Message = $"L?i khi transfer revenue: {ex.Message}",
                    success = false
                };
            }
        }

        /// <summary>
        /// Transfer all eligible booking revenues for a tour slot
        /// </summary>
        public async Task<(bool Success, string Message, int BookingsProcessed, decimal TotalTransferred)> TransferSlotRevenueAsync(Guid tourSlotId)
        {
            try
            {
                _logger.LogInformation("Manual slot revenue transfer requested for tour slot {SlotId}", tourSlotId);

                // Get all completed bookings for this slot
                var bookings = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.TourSlotId == tourSlotId && !b.IsDeleted)
                    .Where(b => b.Status == BookingStatus.Completed && b.RevenueHold > 0)
                    .Where(b => b.RevenueTransferredDate == null) // Not yet transferred
                    .ToListAsync();

                if (!bookings.Any())
                {
                    return (true, "Không có booking nào c?n transfer revenue", 0, 0);
                }

                var revenueService = _serviceProvider.GetRequiredService<ITourRevenueService>();
                var processedCount = 0;
                var totalTransferred = 0m;

                foreach (var booking in bookings)
                {
                    try
                    {
                        var result = await revenueService.TransferBookingRevenueToWalletAsync(booking.Id);
                        if (result.success)
                        {
                            processedCount++;
                            totalTransferred += booking.RevenueHold;
                        }
                        else
                        {
                            _logger.LogWarning("Failed to transfer revenue for booking {BookingId}: {Message}", 
                                booking.Id, result.Message);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error transferring revenue for booking {BookingId}", booking.Id);
                    }
                }

                var message = $"?ã x? lý {processedCount}/{bookings.Count} bookings. T?ng s? ti?n transfer: {totalTransferred:N0} VN?";
                
                _logger.LogInformation("Slot revenue transfer completed for {SlotId}: {Message}", tourSlotId, message);

                return (true, message, processedCount, totalTransferred);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during slot revenue transfer for slot {SlotId}", tourSlotId);
                return (false, $"L?i khi transfer slot revenue: {ex.Message}", 0, 0);
            }
        }

        /// <summary>
        /// Check if a booking is eligible for revenue transfer
        /// </summary>
        public async Task<bool> IsEligibleForRevenueTransferAsync(Guid bookingId)
        {
            try
            {
                var booking = await _unitOfWork.TourBookingRepository.GetQueryable()
                    .Include(b => b.TourSlot)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null)
                {
                    return false;
                }

                // Check eligibility criteria
                var isCompleted = booking.Status == BookingStatus.Completed;
                var hasRevenueHold = booking.RevenueHold > 0;
                var notYetTransferred = booking.RevenueTransferredDate == null;
                
                // Check if tour was completed at least 3 days ago (for standard rule)
                var tourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue;
                var threeDaysAfterTour = tourDate.AddDays(3);
                var isMatured = DateTime.UtcNow >= threeDaysAfterTour;

                return isCompleted && hasRevenueHold && notYetTransferred && isMatured;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking revenue transfer eligibility for booking {BookingId}", bookingId);
                return false;
            }
        }
    }
}