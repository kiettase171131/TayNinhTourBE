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
        /// Lấy các slots template chưa được assign tour details (slots gốc được tạo từ template)
        /// </summary>
        /// <param name="tourTemplateId">ID của TourTemplate</param>
        /// <param name="includeInactive">Có bao gồm slots không active không</param>
        /// <returns>Danh sách slots chưa có tour details</returns>
        Task<IEnumerable<TourSlotDto>> GetUnassignedTemplateSlotsByTemplateAsync(Guid tourTemplateId, bool includeInactive = false);

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

        /// <summary>
        /// Kiểm tra xem có thể booking slot với số lượng khách yêu cầu không
        /// </summary>
        /// <param name="slotId">ID của TourSlot</param>
        /// <param name="requestedGuests">Số lượng khách muốn booking</param>
        /// <returns>True nếu có thể booking</returns>
        Task<bool> CanBookSlotAsync(Guid slotId, int requestedGuests);

        /// <summary>
        /// Reserve capacity cho slot (CHỈ check, không cập nhật CurrentBookings)
        /// Dùng khi tạo booking để kiểm tra capacity
        /// </summary>
        /// <param name="slotId">ID của TourSlot</param>
        /// <param name="guestsToReserve">Số lượng khách cần reserve</param>
        /// <returns>True nếu reserve thành công</returns>
        Task<bool> ReserveSlotCapacityAsync(Guid slotId, int guestsToReserve);

        /// <summary>
        /// Confirm capacity cho slot (CẬP NHẬT CurrentBookings)
        /// CHỈ dùng khi thanh toán thành công
        /// </summary>
        /// <param name="slotId">ID của TourSlot</param>
        /// <param name="guestsToConfirm">Số lượng khách cần confirm</param>
        /// <returns>True nếu confirm thành công</returns>
        Task<bool> ConfirmSlotCapacityAsync(Guid slotId, int guestsToConfirm);

        /// <summary>
        /// Release capacity cho slot (khi hủy booking)
        /// </summary>
        /// <param name="slotId">ID của TourSlot</param>
        /// <param name="guestsToRelease">Số lượng khách cần release</param>
        /// <returns>True nếu release thành công</returns>
        Task<bool> ReleaseSlotCapacityAsync(Guid slotId, int guestsToRelease);

        /// <summary>
        /// Cập nhật capacity cho slot từ TourOperation khi được assign
        /// </summary>
        /// <param name="slotId">ID của TourSlot</param>
        /// <param name="maxGuests">Số lượng khách tối đa từ TourOperation</param>
        /// <returns>True nếu cập nhật thành công</returns>
        Task<bool> UpdateSlotCapacityAsync(Guid slotId, int maxGuests);

        /// <summary>
        /// Sync lại capacity của tất cả slots thuộc TourDetails từ TourOperation
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="maxGuests">Số lượng khách tối đa từ TourOperation</param>
        /// <returns>True nếu sync thành công</returns>
        Task<bool> SyncSlotsCapacityAsync(Guid tourDetailsId, int maxGuests);

        /// <summary>
        /// Get capacity summary for debugging purposes
        /// </summary>
        /// <param name="slotId">ID của slot</param>
        /// <returns>Tuple với thông tin valid và debug info</returns>
        Task<(bool IsValid, string DebugInfo)> GetSlotCapacityDebugInfoAsync(Guid slotId);

        /// <summary>
        /// Lấy chi tiết slot với thông tin tour và danh sách user đã book
        /// </summary>
        /// <param name="slotId">ID của TourSlot</param>
        /// <returns>Chi tiết slot với thông tin tour và danh sách user đã book</returns>
        Task<TourSlotWithBookingsDto?> GetSlotWithTourDetailsAndBookingsAsync(Guid slotId);
    }
}
