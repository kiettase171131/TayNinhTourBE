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
        private const int BatchSize = 50; // Process tours in batches to avoid timeout

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
                    await CheckAndCancelUnderBookedToursAsync(scope.ServiceProvider, stoppingToken);
                    
                    // Job 2: Transfer matured revenue from hold to wallet
                    await TransferMaturedRevenueAsync(scope.ServiceProvider, stoppingToken);

                    _logger.LogInformation("Completed tour auto-cancel and revenue transfer jobs");
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("TourAutoCancelService operation cancelled");
                    break;
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
        private async Task CheckAndCancelUnderBookedToursAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            try
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                var revenueService = serviceProvider.GetRequiredService<ITourRevenueService>();
                var notificationService = serviceProvider.GetRequiredService<ITourCompanyNotificationService>();

                var twoDaysFromNow = DateTime.UtcNow.AddDays(2);
                var currentTime = DateTime.UtcNow;

                // First, get tour IDs that match our criteria (more efficient)
                var eligibleTourIds = await unitOfWork.TourOperationRepository.GetQueryable()
                    .Where(to => to.IsActive && !to.IsDeleted)
                    .Where(to => to.TourDetails.Status == TourDetailsStatus.Public)
                    .Select(to => new { to.Id, to.TourDetailsId })
                    .ToListAsync(cancellationToken);

                if (!eligibleTourIds.Any())
                {
                    _logger.LogInformation("No eligible tours found for cancellation check");
                    return;
                }

                // Process tours in batches to avoid timeout
                for (int i = 0; i < eligibleTourIds.Count; i += BatchSize)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var batch = eligibleTourIds.Skip(i).Take(BatchSize);
                    await ProcessTourBatchForCancellationAsync(batch, twoDaysFromNow, currentTime, serviceProvider, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("CheckAndCancelUnderBookedToursAsync operation cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CheckAndCancelUnderBookedToursAsync");
            }
        }

        private async Task ProcessTourBatchForCancellationAsync(
            IEnumerable<object> tourBatch,
            DateTime twoDaysFromNow,
            DateTime currentTime,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

            var tourIds = tourBatch.Cast<dynamic>().Select(t => (Guid)t.Id).ToList();
            
            // Get tour operations with minimal data first
            var tours = await unitOfWork.TourOperationRepository.GetQueryable()
                .Where(to => tourIds.Contains(to.Id))
                .Include(to => to.TourDetails)
                .ToListAsync(cancellationToken);

            foreach (var tourOperation in tours)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    // Check if tour has slots in the target date range
                    var hasEligibleSlots = await unitOfWork.TourSlotRepository.GetQueryable()
                        .Where(slot => slot.TourDetailsId == tourOperation.TourDetailsId)
                        .Where(slot => slot.TourDate.ToDateTime(TimeOnly.MinValue) <= twoDaysFromNow 
                                    && slot.TourDate.ToDateTime(TimeOnly.MinValue) > currentTime)
                        .AnyAsync(cancellationToken);

                    if (!hasEligibleSlots) continue;

                    // UPDATED: Calculate guest booking rate instead of booking count rate
                    // Compare number of guests booked vs. maximum capacity
                    var guestBookingRate = (double)tourOperation.CurrentBookings / tourOperation.MaxGuests;
                    
                    if (guestBookingRate < 0.5) // < 50% capacity by guest count
                    {
                        _logger.LogInformation("Cancelling under-booked tour: {TourTitle} (Guest booking rate: {BookingRate:P}, {CurrentGuests}/{MaxGuests} guests)", 
                            tourOperation.TourDetails?.Title, guestBookingRate, tourOperation.CurrentBookings, tourOperation.MaxGuests);

                        await CancelTourAndRefundAsync(tourOperation, serviceProvider, cancellationToken);
                    }
                }                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing tour {TourId} for cancellation", tourOperation.Id);
                }
            }
        }

        /// <summary>
        /// Hủy tour và hoàn tiền cho khách hàng
        /// </summary>
        private async Task CancelTourAndRefundAsync(
            DataAccessLayer.Entities.TourOperation tourOperation,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken = default)
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();

            // ✅ Sử dụng execution strategy để handle retry logic với transactions
            var executionStrategy = unitOfWork.GetExecutionStrategy();

            await executionStrategy.ExecuteAsync(async () =>
            {
                using var transaction = await unitOfWork.BeginTransactionAsync();
                try
                {
                var revenueService = serviceProvider.GetRequiredService<ITourRevenueService>();
                var notificationService = serviceProvider.GetRequiredService<ITourCompanyNotificationService>();

                // 1. Lấy tất cả bookings của tour này - query đơn giản hơn
                var bookings = await unitOfWork.TourBookingRepository.GetQueryable()
                    .Where(b => b.TourOperationId == tourOperation.Id && 
                               b.Status == BookingStatus.Confirmed && 
                               !b.IsDeleted)
                    .ToListAsync(cancellationToken);

                if (!bookings.Any())
                {
                    _logger.LogInformation("No confirmed bookings found for tour: {TourTitle}", 
                        tourOperation.TourDetails?.Title);
                    return;
                }

                // 2. Get user details separately to avoid complex joins
                var userIds = bookings.Select(b => b.UserId).Distinct().ToList();
                var users = await unitOfWork.UserRepository.GetQueryable()
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync(cancellationToken);
                
                var userDict = users.ToDictionary(u => u.Id);

                // 3. Update booking status to cancelled
                foreach (var booking in bookings)
                {
                    booking.Status = BookingStatus.CancelledByCompany;
                    booking.CancelledDate = DateTime.UtcNow;
                    booking.CancellationReason = "Tour bị hủy tự động do không đủ khách (< 50% capacity)";
                    booking.UpdatedAt = DateTime.UtcNow;
                    
                    await unitOfWork.TourBookingRepository.UpdateAsync(booking);
                }

                // 4. Update tour operation status
                tourOperation.Status = TourOperationStatus.Cancelled;
                tourOperation.UpdatedAt = DateTime.UtcNow;
                await unitOfWork.TourOperationRepository.UpdateAsync(tourOperation);

                // 5. Calculate amounts for refund
                var totalBookingAmount = bookings.Sum(b => b.TotalPrice);
                var commissionRate = 0.10m;
                var amountInRevenueHold = totalBookingAmount * (1 - commissionRate); // 90% in revenue hold

                // 6. Deduct from booking revenue holds (100% that's actually there)
                foreach (var booking in bookings)
                {
                    if (booking.RevenueHold > 0)
                    {
                        await revenueService.RefundFromRevenueHoldAsync(
                            tourOperation.CreatedById, 
                            booking.RevenueHold, // Full amount (100%)
                            booking.Id);
                    }
                }

                // Note: The system will handle full customer refund (100%) from the revenue hold
                // since we now keep 100% of customer payments in revenue hold

                // 7. Send notifications
                var bookingDtos = bookings.Select(b => new TourBookingDto
                {
                    Id = b.Id,
                    BookingCode = b.BookingCode,
                    NumberOfGuests = b.NumberOfGuests,
                    TotalPrice = b.TotalPrice,
                    BookingDate = b.BookingDate,
                    User = userDict.ContainsKey(b.UserId) ? new UserSummaryDto
                    {
                        Id = userDict[b.UserId].Id,
                        Name = userDict[b.UserId].Name,
                        Email = userDict[b.UserId].Email,
                        PhoneNumber = userDict[b.UserId].PhoneNumber
                    } : null
                }).ToList();

                // Get tour start date separately to avoid complex query
                var tourStartDate = await unitOfWork.TourSlotRepository.GetQueryable()
                    .Where(s => s.TourDetailsId == tourOperation.TourDetailsId)
                    .Select(s => s.TourDate.ToDateTime(TimeOnly.MinValue))
                    .OrderBy(d => d)
                    .FirstOrDefaultAsync(cancellationToken);

                if (tourStartDate == default(DateTime))
                {
                    tourStartDate = DateTime.UtcNow;
                }

                await notificationService.NotifyTourCancellationAsync(
                    tourOperation.CreatedById,
                    bookingDtos,
                    tourOperation.TourDetails?.Title ?? "Unknown Tour",
                    tourStartDate,
                    $"Không đủ khách (chỉ có {tourOperation.CurrentBookings}/{tourOperation.MaxGuests} khách, < 50% capacity)");

                // 8. Send cancellation emails to customers
                // This would be implemented separately or integrated with existing email service

                await unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                var totalRefundAmount = bookings.Sum(b => b.TotalPrice);
                _logger.LogInformation("Successfully cancelled tour: {TourTitle} with {BookingCount} bookings, total refund amount: {RefundAmount} VNĐ (100% to customers)",
                    tourOperation.TourDetails?.Title, bookings.Count, totalRefundAmount);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error cancelling tour: {TourTitle}", tourOperation.TourDetails?.Title);
                    throw;
                }
            });
        }

        /// <summary>
        /// Chuyển tiền từ revenue hold sang wallet cho các tours đã hoàn thành 3 ngày
        /// UPDATED: Now works with booking-level revenue hold
        /// </summary>
        private async Task TransferMaturedRevenueAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            try
            {
                var revenueService = serviceProvider.GetRequiredService<ITourRevenueService>();
                var notificationService = serviceProvider.GetRequiredService<ITourCompanyNotificationService>();

                // Get eligible booking IDs for revenue transfer
                var eligibleBookings = await revenueService.GetBookingsEligibleForRevenueTransferAsync();

                if (!eligibleBookings.Any())
                {
                    _logger.LogInformation("No eligible bookings found for revenue transfer");
                    return;
                }

                _logger.LogInformation("Found {Count} bookings eligible for revenue transfer", eligibleBookings.Count);

                // Process bookings in batches - extract IDs from booking entities
                var eligibleBookingIds = eligibleBookings.Select(b => b.Id).ToList();
                
                for (int i = 0; i < eligibleBookingIds.Count; i += BatchSize)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var batch = eligibleBookingIds.Skip(i).Take(BatchSize);
                    await ProcessBookingBatchForRevenueTransferAsync(batch, serviceProvider, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("TransferMaturedRevenueAsync operation cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TransferMaturedRevenueAsync");
            }
        }

        /// <summary>
        /// Process booking batch for revenue transfer
        /// NEW: Process individual bookings instead of tour operations
        /// </summary>
        private async Task ProcessBookingBatchForRevenueTransferAsync(
            IEnumerable<Guid> bookingBatch,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var revenueService = serviceProvider.GetRequiredService<ITourRevenueService>();
            var notificationService = serviceProvider.GetRequiredService<ITourCompanyNotificationService>();

            foreach (var bookingId in bookingBatch)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    // Transfer revenue for this specific booking
                    var transferResult = await revenueService.TransferBookingRevenueToWalletAsync(bookingId);

                    if (transferResult.success)
                    {
                        // Get booking details for notification
                        var booking = await unitOfWork.TourBookingRepository.GetQueryable()
                            .Where(b => b.Id == bookingId)
                            .Include(b => b.TourOperation)
                                .ThenInclude(to => to.TourDetails)
                            .Include(b => b.TourSlot)
                            .FirstOrDefaultAsync(cancellationToken);

                        if (booking != null)
                        {
                            var tourTitle = booking.TourOperation?.TourDetails?.Title ?? "Unknown Tour";
                            var tourCompletedDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? DateTime.UtcNow;
                            var transferredAmount = booking.RevenueHold; // Amount that was transferred

                            // Send notification to tour company
                            await notificationService.NotifyRevenueTransferAsync(
                                booking.TourOperation.CreatedById,
                                transferredAmount,
                                tourTitle,
                                tourCompletedDate);

                            _logger.LogInformation("Transferred revenue for booking: {BookingCode}, amount: {Amount}, tour: {TourTitle}", 
                                booking.BookingCode, transferredAmount, tourTitle);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Failed to transfer revenue for booking {BookingId}: {Reason}", 
                            bookingId, transferResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing booking {BookingId} for revenue transfer", bookingId);
                }
            }
        }
    }
}
