using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Interface cho TourSlot service
    /// </summary>
    public interface ITourSlotService
    {
        /// <summary>
        /// Lấy danh sách TourSlots theo filter
        /// </summary>
        /// <param name="tourTemplateId">ID của TourTemplate (optional)</param>
        /// <param name="tourDetailsId">ID của TourDetails (optional)</param>
        /// <param name="fromDate">Từ ngày (optional)</param>
        /// <param name="toDate">Đến ngày (optional)</param>
        /// <param name="scheduleDay">Ngày trong tuần (optional)</param>
        /// <param name="includeInactive">Có bao gồm slots không active không</param>
        /// <returns>Danh sách TourSlots</returns>
        Task<IEnumerable<TourSlotDto>> GetSlotsAsync(
            Guid? tourTemplateId = null,
            Guid? tourDetailsId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            ScheduleDay? scheduleDay = null,
            bool includeInactive = false);

        /// <summary>
        /// Lấy chi tiết một TourSlot theo ID
        /// </summary>
        /// <param name="id">ID của TourSlot</param>
        /// <returns>Chi tiết TourSlot</returns>
        Task<TourSlotDto?> GetSlotByIdAsync(Guid id);

        /// <summary>
        /// Lấy TourSlots của một TourDetails cụ thể
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Danh sách TourSlots của TourDetails</returns>
        Task<IEnumerable<TourSlotDto>> GetSlotsByTourDetailsAsync(Guid tourDetailsId);

        /// <summary>
        /// Lấy TourSlots của một TourTemplate cụ thể
        /// </summary>
        /// <param name="tourTemplateId">ID của TourTemplate</param>
        /// <returns>Danh sách TourSlots của TourTemplate</returns>
        Task<IEnumerable<TourSlotDto>> GetSlotsByTourTemplateAsync(Guid tourTemplateId);

        /// <summary>
        /// Lấy các slots available cho booking
        /// </summary>
        /// <param name="tourTemplateId">ID của TourTemplate (optional)</param>
        /// <param name="fromDate">Từ ngày (optional)</param>
        /// <param name="toDate">Đến ngày (optional)</param>
        /// <returns>Danh sách slots available</returns>
        Task<IEnumerable<TourSlotDto>> GetAvailableSlotsAsync(
            Guid? tourTemplateId = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null);
    }
}
