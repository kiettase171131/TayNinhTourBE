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
    /// Service for manual tour auto-cancel operations for testing
    /// </summary>
    public class TourAutoCancelTestingService : ITourAutoCancelService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TourAutoCancelTestingService> _logger;

        public TourAutoCancelTestingService(
            IUnitOfWork unitOfWork,
            IServiceProvider serviceProvider,
            ILogger<TourAutoCancelTestingService> logger)
        {
            _unitOfWork = unitOfWork;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Auto-cancel a tour slot manually for testing purposes
        /// </summary>
        public async Task<(bool Success, string Message, int CustomersNotified)> AutoCancelTourSlotAsync(Guid tourSlotId, string reason)
        {
            try
            {
                _logger.LogInformation("Manual auto-cancel requested for tour slot {SlotId}: {Reason}", tourSlotId, reason);

                // Use the existing TourSlotService.CancelPublicTourSlotAsync method
                var tourSlotService = _serviceProvider.GetRequiredService<ITourSlotService>();
                
                // Get the tour company user ID (owner of the tour)
                var tourSlot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .FirstOrDefaultAsync(ts => ts.Id == tourSlotId && !ts.IsDeleted);

                if (tourSlot?.TourDetails?.TourOperation == null)
                {
                    return (false, "Không tìm th?y tour slot ho?c tour operation", 0);
                }

                var tourCompanyUserId = tourSlot.TourDetails.TourOperation.CreatedById;

                // Call the existing cancel method
                var result = await tourSlotService.CancelPublicTourSlotAsync(tourSlotId, reason, tourCompanyUserId);

                _logger.LogInformation("Auto-cancel result for slot {SlotId}: Success={Success}, CustomersNotified={CustomersNotified}", 
                    tourSlotId, result.Success, result.CustomersNotified);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during manual auto-cancel for slot {SlotId}", tourSlotId);
                return (false, $"L?i khi auto-cancel: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Check if a tour slot is eligible for auto-cancel based on booking rate
        /// </summary>
        public async Task<bool> IsEligibleForAutoCancelAsync(Guid tourSlotId)
        {
            try
            {
                var tourSlot = await _unitOfWork.TourSlotRepository.GetQueryable()
                    .Include(ts => ts.TourDetails)
                        .ThenInclude(td => td!.TourOperation)
                    .Include(ts => ts.Bookings.Where(b => !b.IsDeleted))
                    .FirstOrDefaultAsync(ts => ts.Id == tourSlotId && !ts.IsDeleted);

                if (tourSlot?.TourDetails?.TourOperation == null)
                {
                    return false;
                }

                // Check if tour date is within cancellation window (e.g., 2 days before)
                var tourDate = tourSlot.TourDate.ToDateTime(TimeOnly.MinValue);
                var cancellationDeadline = DateTime.UtcNow.AddDays(2);

                if (tourDate > cancellationDeadline)
                {
                    return false; // Too early to cancel
                }

                // Check booking rate (< 50% capacity)
                var confirmedBookings = tourSlot.Bookings.Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.NumberOfGuests);
                var bookingRate = (double)confirmedBookings / tourSlot.TourDetails.TourOperation.MaxGuests;

                return bookingRate < 0.5; // Less than 50% capacity
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking auto-cancel eligibility for slot {SlotId}", tourSlotId);
                return false;
            }
        }
    }
}