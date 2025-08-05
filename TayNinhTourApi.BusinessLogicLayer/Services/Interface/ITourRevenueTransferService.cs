using TayNinhTourApi.BusinessLogicLayer.DTOs;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Interface for tour revenue transfer service for testing purposes
    /// </summary>
    public interface ITourRevenueTransferService
    {
        /// <summary>
        /// Transfer revenue from hold to wallet for a specific booking manually for testing
        /// </summary>
        /// <param name="bookingId">ID c?a booking c?n transfer revenue</param>
        /// <returns>K?t qu? transfer</returns>
        Task<BaseResposeDto> TransferBookingRevenueAsync(Guid bookingId);

        /// <summary>
        /// Transfer all eligible booking revenues for a tour slot
        /// </summary>
        /// <param name="tourSlotId">ID c?a tour slot</param>
        /// <returns>K?t qu? transfer v?i s? l??ng bookings ?ã x? lý</returns>
        Task<(bool Success, string Message, int BookingsProcessed, decimal TotalTransferred)> TransferSlotRevenueAsync(Guid tourSlotId);

        /// <summary>
        /// Check if a booking is eligible for revenue transfer
        /// </summary>
        /// <param name="bookingId">ID c?a booking</param>
        /// <returns>True if eligible for revenue transfer</returns>
        Task<bool> IsEligibleForRevenueTransferAsync(Guid bookingId);
    }
}