using TayNinhTourApi.BusinessLogicLayer.DTOs.Request.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourCompany;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation;
using TayNinhTourApi.BusinessLogicLayer.DTOs;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho quản lý TourOperation
    /// TourOperation chứa thông tin vận hành cụ thể cho mỗi TourDetails
    /// </summary>
    public interface ITourOperationService
    {
        /// <summary>
        /// Tạo operation mới cho TourDetails
        /// Business Rules:
        /// - TourDetails phải tồn tại và chưa có Operation
        /// - MaxSeats <= Template.MaxGuests
        /// - GuideId phải valid (nếu có)
        /// - Price >= 0
        /// </summary>
        Task<ResponseCreateOperationDto> CreateOperationAsync(RequestCreateOperationDto request);

        /// <summary>
        /// Lấy operation theo TourDetails ID
        /// Return null nếu TourDetails chưa có operation
        /// </summary>
        Task<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto?> GetOperationByTourDetailsAsync(Guid tourDetailsId);

        /// <summary>
        /// Lấy operation theo Operation ID
        /// </summary>
        Task<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto?> GetOperationByIdAsync(Guid operationId);

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
        Task<List<TayNinhTourApi.BusinessLogicLayer.DTOs.Response.TourOperation.TourOperationDto>> GetOperationsAsync(
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
        /// Check xem TourDetails có thể tạo operation không
        /// </summary>
        Task<bool> CanCreateOperationForTourDetailsAsync(Guid tourDetailsId);

        /// <summary>
        /// Validate TourDetails readiness cho việc tạo TourOperation (public tour)
        /// Kiểm tra TourGuide assignment và SpecialtyShop participation
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails cần validate</param>
        /// <returns>Tuple với IsReady status và error message nếu không ready</returns>
        Task<(bool IsReady, string ErrorMessage)> ValidateTourDetailsReadinessAsync(Guid tourDetailsId);

        /// <summary>
        /// Get readiness status của TourDetails cho frontend checking
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Detailed readiness information</returns>
        Task<TourDetailsReadinessDto> GetTourDetailsReadinessAsync(Guid tourDetailsId);
    }
}
