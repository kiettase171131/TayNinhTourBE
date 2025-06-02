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
        /// <param name="includeInactive">Có bao gồm slots không active không</param>
        /// <returns>Danh sách tour slots</returns>
        Task<IEnumerable<TourSlot>> GetByTourTemplateAsync(Guid tourTemplateId, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour slots trong khoảng thời gian
        /// </summary>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <param name="includeInactive">Có bao gồm slots không active không</param>
        /// <returns>Danh sách tour slots</returns>
        Task<IEnumerable<TourSlot>> GetByDateRangeAsync(DateOnly startDate, DateOnly endDate, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour slots theo ngày trong tuần
        /// </summary>
        /// <param name="scheduleDay">Ngày trong tuần (Saturday/Sunday)</param>
        /// <param name="includeInactive">Có bao gồm slots không active không</param>
        /// <returns>Danh sách tour slots</returns>
        Task<IEnumerable<TourSlot>> GetByScheduleDayAsync(ScheduleDay scheduleDay, bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour slots available (có thể booking)
        /// </summary>
        /// <param name="fromDate">Từ ngày (optional)</param>
        /// <param name="toDate">Đến ngày (optional)</param>
        /// <returns>Danh sách tour slots available</returns>
        Task<IEnumerable<TourSlot>> GetAvailableSlotsAsync(DateOnly? fromDate = null, DateOnly? toDate = null);

        /// <summary>
        /// Lấy tour slot theo ngày cụ thể và tour template
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="tourDate">Ngày tour</param>
        /// <returns>Tour slot nếu tồn tại</returns>
        Task<TourSlot?> GetByTemplateAndDateAsync(Guid tourTemplateId, DateOnly tourDate);

        /// <summary>
        /// Lấy tour slot với đầy đủ thông tin relationships
        /// </summary>
        /// <param name="id">ID của tour slot</param>
        /// <returns>Tour slot với relationships</returns>
        Task<TourSlot?> GetWithDetailsAsync(Guid id);

        /// <summary>
        /// Kiểm tra xem có tour slot nào trong khoảng thời gian không
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="startDate">Ngày bắt đầu</param>
        /// <param name="endDate">Ngày kết thúc</param>
        /// <returns>True nếu có slot trong khoảng thời gian</returns>
        Task<bool> HasSlotsInDateRangeAsync(Guid tourTemplateId, DateOnly startDate, DateOnly endDate);

        /// <summary>
        /// Lấy danh sách tour slots với pagination và filtering
        /// </summary>
        /// <param name="pageIndex">Trang hiện tại</param>
        /// <param name="pageSize">Số items per page</param>
        /// <param name="tourTemplateId">Filter theo tour template (optional)</param>
        /// <param name="scheduleDay">Filter theo ngày trong tuần (optional)</param>
        /// <param name="fromDate">Filter từ ngày (optional)</param>
        /// <param name="toDate">Filter đến ngày (optional)</param>
        /// <param name="includeInactive">Có bao gồm slots không active không</param>
        /// <returns>Tuple chứa danh sách tour slots và tổng số records</returns>
        Task<(IEnumerable<TourSlot> Slots, int TotalCount)> GetPaginatedAsync(
            int pageIndex,
            int pageSize,
            Guid? tourTemplateId = null,
            ScheduleDay? scheduleDay = null,
            DateOnly? fromDate = null,
            DateOnly? toDate = null,
            bool includeInactive = false);

        /// <summary>
        /// Lấy danh sách tour slots sắp tới (từ hôm nay trở đi)
        /// </summary>
        /// <param name="tourTemplateId">Filter theo tour template (optional)</param>
        /// <param name="top">Số lượng slots lấy về</param>
        /// <returns>Danh sách tour slots sắp tới</returns>
        Task<IEnumerable<TourSlot>> GetUpcomingSlotsAsync(Guid? tourTemplateId = null, int top = 10);

        /// <summary>
        /// Kiểm tra xem tour slot có tour operation không
        /// </summary>
        /// <param name="id">ID của tour slot</param>
        /// <returns>True nếu có tour operation</returns>
        Task<bool> HasTourOperationAsync(Guid id);
    }
}
