using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation;
using TayNinhTourApi.BusinessLogicLayer.DTOs;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho quản lý TourOperation
    /// TourOperation chứa thông tin vận hành cụ thể cho mỗi TourSlot
    /// </summary>
    public interface ITourOperationService
    {
        /// <summary>
        /// Tạo operation mới cho TourSlot
        /// Business Rules:
        /// - TourSlot phải tồn tại và chưa có Operation
        /// - MaxSeats <= Template.MaxGuests
        /// - GuideId phải valid (nếu có)
        /// - Price >= 0
        /// </summary>
        Task<ResponseCreateOperationDto> CreateOperationAsync(RequestCreateOperationDto request);

        /// <summary>
        /// Lấy operation theo TourSlot ID
        /// Return null nếu slot chưa có operation
        /// </summary>
        Task<TourOperationDto?> GetOperationBySlotAsync(Guid slotId);

        /// <summary>
        /// Lấy operation theo Operation ID
        /// </summary>
        Task<TourOperationDto?> GetOperationByIdAsync(Guid operationId);

        /// <summary>
        /// Cập nhật operation
        /// Business Rules:
        /// - Không được update nếu có booking active
        /// - MaxSeats >= BookedSeats hiện tại
        /// - GuideId phải valid (nếu thay đổi)
        /// </summary>
        Task<ResponseUpdateOperationDto> UpdateOperationAsync(Guid id, RequestUpdateOperationDto request);

        /// <summary>
        /// Xóa operation
        /// Business Rules:
        /// - Không được xóa nếu có booking
        /// - Soft delete (set IsActive = false)
        /// </summary>
        Task<BaseResposeDto> DeleteOperationAsync(Guid id);

        /// <summary>
        /// Lấy danh sách operations với filtering
        /// </summary>
        Task<List<TourOperationDto>> GetOperationsAsync(
            Guid? tourTemplateId = null,
            Guid? guideId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool includeInactive = false);

        /// <summary>
        /// Validate business rules cho operation
        /// </summary>
        Task<(bool IsValid, string ErrorMessage)> ValidateOperationAsync(RequestCreateOperationDto request);

        /// <summary>
        /// Check xem slot có thể tạo operation không
        /// </summary>
        Task<bool> CanCreateOperationForSlotAsync(Guid slotId);
    }
}
