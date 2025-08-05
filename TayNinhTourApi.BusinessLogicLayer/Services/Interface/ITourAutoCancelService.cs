using TayNinhTourApi.BusinessLogicLayer.DTOs;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Interface for tour auto-cancel service for testing purposes
    /// </summary>
    public interface ITourAutoCancelService
    {
        /// <summary>
        /// Auto-cancel a tour slot manually for testing purposes
        /// </summary>
        /// <param name="tourSlotId">ID c?a tour slot c?n cancel</param>
        /// <param name="reason">Lý do cancel</param>
        /// <returns>K?t qu? cancel v?i thông tin customers ???c thông báo</returns>
        Task<(bool Success, string Message, int CustomersNotified)> AutoCancelTourSlotAsync(Guid tourSlotId, string reason);

        /// <summary>
        /// Check if a tour slot is eligible for auto-cancel based on booking rate
        /// </summary>
        /// <param name="tourSlotId">ID c?a tour slot</param>
        /// <returns>True if eligible for auto-cancel</returns>
        Task<bool> IsEligibleForAutoCancelAsync(Guid tourSlotId);
    }
}