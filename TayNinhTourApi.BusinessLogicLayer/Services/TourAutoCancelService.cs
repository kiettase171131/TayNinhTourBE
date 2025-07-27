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

                    var bookingRate = (double)tourOperation.CurrentBookings / tourOperation.MaxGuests;
                    
                    if (bookingRate < 0.5) // < 50% capacity
                    {
                        _logger.LogInformation("Cancelling under-booked tour: {TourTitle} (Booking rate: {BookingRate:P})", 
                            tourOperation.TourDetails?.Title, bookingRate);

                        await CancelTourAndRefundAsync(tourOperation, serviceProvider, cancellationToken);
                    }
                }
                catch (Exception ex)
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
            using var transaction = await serviceProvider.GetRequiredService<IUnitOfWork>().BeginTransactionAsync();
            try
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
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

                // 6. Deduct from TourCompany revenue hold (only the 90% that's actually there)
                await revenueService.RefundFromRevenueHoldAsync(
                    tourOperation.CreatedById, 
                    amountInRevenueHold, 
                    tourOperation.Id);

                // Note: The system will handle full customer refund (100%) from other sources
                // while only deducting the 90% from tour company's revenue hold

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
                    "Không đủ khách (< 50% capacity)");

                // 8. Send cancellation emails to customers
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
        private async Task TransferMaturedRevenueAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            try
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                var revenueService = serviceProvider.GetRequiredService<ITourRevenueService>();
                var notificationService = serviceProvider.GetRequiredService<ITourCompanyNotificationService>();

                var threeDaysAgo = DateTime.UtcNow.AddDays(-3);

                // Get completed tour IDs first (more efficient)
                var completedTourIds = await unitOfWork.TourOperationRepository.GetQueryable()
                    .Where(to => to.Status == TourOperationStatus.Completed && !to.IsDeleted)
                    .Select(to => new { to.Id, to.TourDetailsId, to.CreatedById })
                    .ToListAsync(cancellationToken);

                if (!completedTourIds.Any())
                {
                    _logger.LogInformation("No completed tours found for revenue transfer");
                    return;
                }

                // Process tours in batches
                for (int i = 0; i < completedTourIds.Count; i += BatchSize)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    var batch = completedTourIds.Skip(i).Take(BatchSize);
                    await ProcessTourBatchForRevenueTransferAsync(batch, threeDaysAgo, serviceProvider, cancellationToken);
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

        private async Task ProcessTourBatchForRevenueTransferAsync(
            IEnumerable<object> tourBatch,
            DateTime threeDaysAgo,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var revenueService = serviceProvider.GetRequiredService<ITourRevenueService>();
            var notificationService = serviceProvider.GetRequiredService<ITourCompanyNotificationService>();

            foreach (var tourObj in tourBatch)
            {
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    dynamic tour = tourObj;
                    var tourDetailsId = (Guid)tour.TourDetailsId;
                    var tourId = (Guid)tour.Id;
                    var createdById = (Guid)tour.CreatedById;

                    // Check if tour has slots completed 3 days ago
                    var hasEligibleSlots = await unitOfWork.TourSlotRepository.GetQueryable()
                        .Where(s => s.TourDetailsId == tourDetailsId)
                        .Where(s => s.TourDate.ToDateTime(TimeOnly.MinValue) <= threeDaysAgo)
                        .AnyAsync(cancellationToken);

                    if (!hasEligibleSlots) continue;

                    // Get tour details separately
                    var tourDetails = await unitOfWork.TourDetailsRepository.GetQueryable()
                        .Where(td => td.Id == tourDetailsId)
                        .FirstOrDefaultAsync(cancellationToken);

                    // Tính tổng tiền từ các bookings confirmed của tour này
                    var confirmedBookings = await unitOfWork.TourBookingRepository.GetQueryable()
                        .Where(b => b.TourOperationId == tourId &&
                                   b.Status == BookingStatus.Confirmed &&
                                   !b.IsDeleted)
                        .Select(b => b.TotalPrice)
                        .ToListAsync(cancellationToken);

                    if (!confirmedBookings.Any()) continue;

                    var totalBookingAmount = confirmedBookings.Sum();

                    // Get tour completion date separately
                    var tourCompletedDate = await unitOfWork.TourSlotRepository.GetQueryable()
                        .Where(s => s.TourDetailsId == tourDetailsId)
                        .Select(s => s.TourDate.ToDateTime(TimeOnly.MinValue))
                        .OrderByDescending(d => d)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (tourCompletedDate == default(DateTime))
                    {
                        tourCompletedDate = DateTime.UtcNow;
                    }

                    // Calculate the amount in revenue hold (90% of total booking amount due to 10% commission)
                    var commissionRate = 0.10m;
                    var availableAmountInHold = totalBookingAmount * (1 - commissionRate);

                    // Transfer from revenue hold to wallet (only the 90% that's actually in the hold)
                    var transferResult = await revenueService.TransferFromHoldToWalletAsync(
                        createdById,
                        availableAmountInHold);

                    if (transferResult.success)
                    {
                        // Send notification with the amount actually transferred (90% of booking)
                        await notificationService.NotifyRevenueTransferAsync(
                            createdById,
                            availableAmountInHold,
                            tourDetails?.Title ?? "Unknown Tour",
                            tourCompletedDate);

                        _logger.LogInformation("Transferred revenue for tour: {TourTitle}, amount: {Amount} (after 10% commission deduction)", 
                            tourDetails?.Title, availableAmountInHold);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to transfer revenue for tour: {TourTitle}, reason: {Reason}", 
                            tourDetails?.Title, transferResult.Message);
                    }
                }
                catch (Exception ex)
                {
                    dynamic tour = tourObj;
                    _logger.LogError(ex, "Error processing tour {TourId} for revenue transfer", (Guid)tour.Id);
                }
            }
        }
    }
}
