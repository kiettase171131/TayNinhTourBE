using TayNinhTourApi.BusinessLogicLayer.DTOs;
using TayNinhTourApi.BusinessLogicLayer.DTOs.Response;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.BusinessLogicLayer.Services.Interface
{
    /// <summary>
    /// Service interface cho TourGuide invitation workflow
    /// Quản lý việc mời, chấp nhận, từ chối invitations
    /// </summary>
    public interface ITourGuideInvitationService
    {
        /// <summary>
        /// Tạo automatic invitations cho tất cả TourGuides có skills phù hợp
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="createdById">ID của user tạo invitations (TourCompany)</param>
        /// <returns>Số lượng invitations đã tạo</returns>
        Task<BaseResposeDto> CreateAutomaticInvitationsAsync(Guid tourDetailsId, Guid createdById);

        /// <summary>
        /// Tạo manual invitation cho một TourGuide cụ thể
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="guideId">ID của TourGuide được mời</param>
        /// <param name="createdById">ID của user tạo invitation (TourCompany)</param>
        /// <returns>Kết quả tạo invitation</returns>
        Task<BaseResposeDto> CreateManualInvitationAsync(Guid tourDetailsId, Guid guideId, Guid createdById);

        /// <summary>
        /// TourGuide chấp nhận invitation
        /// </summary>
        /// <param name="invitationId">ID của invitation</param>
        /// <param name="guideId">ID của TourGuide (để verify ownership)</param>
        /// <returns>Kết quả accept invitation</returns>
        Task<BaseResposeDto> AcceptInvitationAsync(Guid invitationId, Guid guideId);

        /// <summary>
        /// TourGuide từ chối invitation
        /// </summary>
        /// <param name="invitationId">ID của invitation</param>
        /// <param name="guideId">ID của TourGuide (để verify ownership)</param>
        /// <param name="rejectionReason">Lý do từ chối (optional)</param>
        /// <returns>Kết quả reject invitation</returns>
        Task<BaseResposeDto> RejectInvitationAsync(Guid invitationId, Guid guideId, string? rejectionReason = null);

        /// <summary>
        /// Lấy danh sách invitations cho TourGuide hiện tại
        /// </summary>
        /// <param name="guideId">ID của TourGuide</param>
        /// <param name="status">Lọc theo status (optional)</param>
        /// <returns>Danh sách invitations</returns>
        Task<MyInvitationsResponseDto> GetMyInvitationsAsync(Guid guideId, InvitationStatus? status = null);

        /// <summary>
        /// Lấy danh sách invitations cho một TourDetails (admin/company view)
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Danh sách invitations với thông tin guide</returns>
        Task<TourDetailsInvitationsResponseDto> GetInvitationsForTourDetailsAsync(Guid tourDetailsId);

        /// <summary>
        /// Expire tất cả pending invitations đã hết hạn
        /// Background job sẽ gọi method này
        /// </summary>
        /// <returns>Số lượng invitations đã expired</returns>
        Task<int> ExpireExpiredInvitationsAsync();

        /// <summary>
        /// Kiểm tra và transition TourDetails từ Pending sang AwaitingGuideAssignment
        /// Background job sẽ gọi method này sau 24 hours
        /// </summary>
        /// <returns>Số lượng TourDetails đã transition</returns>
        Task<int> TransitionToManualSelectionAsync();

        /// <summary>
        /// Cancel TourDetails không có guide assignment sau 5 ngày
        /// Background job sẽ gọi method này
        /// </summary>
        /// <returns>Số lượng TourDetails đã cancelled</returns>
        Task<int> CancelUnassignedTourDetailsAsync();

        /// <summary>
        /// Lấy thông tin chi tiết của một invitation
        /// </summary>
        /// <param name="invitationId">ID của invitation</param>
        /// <returns>Thông tin chi tiết invitation</returns>
        Task<InvitationDetailsResponseDto> GetInvitationDetailsAsync(Guid invitationId);

        /// <summary>
        /// Kiểm tra xem TourGuide có thể accept invitation không
        /// (check conflicts, availability, etc.)
        /// </summary>
        /// <param name="invitationId">ID của invitation</param>
        /// <param name="guideId">ID của TourGuide</param>
        /// <returns>Kết quả validation</returns>
        Task<BaseResposeDto> ValidateInvitationAcceptanceAsync(Guid invitationId, Guid guideId);

        /// <summary>
        /// Fix TourDetails status cho các case đã có guide accept nhưng status vẫn AwaitingGuideAssignment
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails cần fix</param>
        /// <returns>Kết quả fix status</returns>
        Task<BaseResposeDto> FixTourDetailsStatusAsync(Guid tourDetailsId);

        /// <summary>
        /// Debug method để manually update TourOperation với guide info
        /// </summary>
        /// <param name="invitationId">ID của invitation đã được accept</param>
        /// <returns>Kết quả debug</returns>
        Task<BaseResposeDto> DebugUpdateTourOperationAsync(Guid invitationId);
    }
}
