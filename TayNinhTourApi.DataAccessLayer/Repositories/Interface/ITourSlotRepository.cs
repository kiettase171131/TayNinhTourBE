using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourSlot entity
    /// Kế thừa từ IGenericRepository và thêm các methods specific cho TourSlot
    /// </summary>
    public interface ITourSlotRepository : IGenericRepository<TourSlot>
    {
        /// <summary>
        /// Lấy danh sách tour slots theo tour template
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <returns>Danh sách tour slots</returns>
        Task<IEnumerable<TourSlot>> GetByTourTemplateAsync(Guid tourTemplateId);

        /// <summary>
        /// Lấy danh sách tour slots theo tour details
        /// </summary>
        /// <param name="tourDetailsId">ID của tour details</param>
        /// <returns>Danh sách tour slots</returns>
        Task<IEnumerable<TourSlot>> GetByTourDetailsAsync(Guid tourDetailsId);

        /// <summary>
        /// Lấy tour slot theo ngày cụ thể
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="date">Ngày tour</param>
        /// <returns>Tour slot nếu tồn tại</returns>
        Task<TourSlot?> GetByDateAsync(Guid tourTemplateId, DateOnly date);

        /// <summary>
        /// Lấy danh sách slots available với filtering
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template (optional)</param>
        /// <param name="scheduleDay">Ngày trong tuần (optional)</param>
        /// <param name="fromDate">Từ ngày (optional)</param>
        /// <param name="toDate">Đến ngày (optional)</param>
        /// <param name="includeInactive">Có bao gồm slots không active không</param>
        /// <returns>Danh sách tour slots</returns>
        Task<IEnumerable<TourSlot>> GetAvailableSlotsAsync(
            Guid? tourTemplateId = null,
            ScheduleDay? scheduleDay = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            bool includeInactive = false);

        /// <summary>
        /// Kiểm tra xem slot có tồn tại không
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="date">Ngày tour</param>
        /// <returns>True nếu slot tồn tại</returns>
        Task<bool> SlotExistsAsync(Guid tourTemplateId, DateOnly date);

        /// <summary>
        /// Cập nhật trạng thái của nhiều slots
        /// </summary>
        /// <param name="slotIds">Danh sách ID của slots</param>
        /// <param name="status">Trạng thái mới</param>
        /// <returns>Số lượng slots được cập nhật</returns>
        Task<int> BulkUpdateStatusAsync(IEnumerable<Guid> slotIds, TourSlotStatus status);
    }
}
