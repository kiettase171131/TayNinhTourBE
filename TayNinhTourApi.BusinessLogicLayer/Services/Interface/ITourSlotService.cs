using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourSlot;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourSlot;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho quản lý TourSlot với tự động scheduling
    /// Cung cấp các chức năng tạo, quản lý và scheduling tour slots
    /// </summary>
    public interface ITourSlotService
    {
        /// <summary>
        /// Tự động tạo tour slots cho một tháng dựa trên tour template
        /// Generate slots cho tất cả weekends trong tháng (Saturday và Sunday)
        /// </summary>
        /// <param name="request">Thông tin để generate slots</param>
        /// <returns>Kết quả tạo slots với thông tin chi tiết</returns>
        Task<ResponseGenerateSlotsDto> GenerateSlotsAsync(RequestGenerateSlotsDto request);

        /// <summary>
        /// Preview các slots sẽ được tạo trước khi commit vào database
        /// Cho phép user xem trước và xác nhận trước khi tạo thực tế
        /// </summary>
        /// <param name="request">Thông tin để preview slots</param>
        /// <returns>Danh sách slots sẽ được tạo</returns>
        Task<ResponsePreviewSlotsDto> PreviewSlotsAsync(RequestPreviewSlotsDto request);

        /// <summary>
        /// Lấy danh sách tour slots với filtering và pagination
        /// Hỗ trợ filter theo template, date range, status, schedule day
        /// </summary>
        /// <param name="request">Criteria để filter slots</param>
        /// <returns>Danh sách slots với pagination</returns>
        Task<ResponseGetSlotsDto> GetSlotsAsync(RequestGetSlotsDto request);

        /// <summary>
        /// Lấy thông tin chi tiết của một tour slot
        /// </summary>
        /// <param name="slotId">ID của tour slot</param>
        /// <returns>Thông tin chi tiết tour slot</returns>
        Task<ResponseGetSlotDetailDto> GetSlotDetailAsync(Guid slotId);

        /// <summary>
        /// Cập nhật thông tin của một tour slot
        /// Cho phép update status, IsActive và các thông tin khác
        /// </summary>
        /// <param name="slotId">ID của slot cần update</param>
        /// <param name="request">Thông tin cập nhật</param>
        /// <returns>Kết quả cập nhật</returns>
        Task<BaseResposeDto> UpdateSlotAsync(Guid slotId, RequestUpdateSlotDto request);

        /// <summary>
        /// Xóa một tour slot (soft delete)
        /// Chỉ cho phép xóa slots chưa có booking
        /// </summary>
        /// <param name="slotId">ID của slot cần xóa</param>
        /// <returns>Kết quả xóa</returns>
        Task<BaseResposeDto> DeleteSlotAsync(Guid slotId);

        /// <summary>
        /// Lấy danh sách slots sắp tới (upcoming)
        /// Dùng cho dashboard và quick access
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template (optional)</param>
        /// <param name="top">Số lượng slots cần lấy</param>
        /// <returns>Danh sách upcoming slots</returns>
        Task<ResponseGetUpcomingSlotsDto> GetUpcomingSlotsAsync(Guid? tourTemplateId = null, int top = 10);

        /// <summary>
        /// Kiểm tra conflicts khi tạo slots mới
        /// Đảm bảo không tạo duplicate slots cho cùng template và date
        /// </summary>
        /// <param name="tourTemplateId">ID của tour template</param>
        /// <param name="dates">Danh sách dates cần kiểm tra</param>
        /// <returns>Danh sách dates bị conflict</returns>
        Task<ResponseCheckSlotConflictsDto> CheckSlotConflictsAsync(Guid tourTemplateId, IEnumerable<DateOnly> dates);

        /// <summary>
        /// Bulk update status cho nhiều slots cùng lúc
        /// Hữu ích cho việc activate/deactivate hàng loạt
        /// </summary>
        /// <param name="request">Thông tin bulk update</param>
        /// <returns>Kết quả bulk update</returns>
        Task<BaseResposeDto> BulkUpdateSlotStatusAsync(RequestBulkUpdateSlotStatusDto request);
    }
}
