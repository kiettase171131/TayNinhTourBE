using TayNinhTourApi.DataAccessLayer.Entities;
using TayNinhTourApi.DataAccessLayer.Enums;

namespace TayNinhTourApi.DataAccessLayer.Repositories.Interface
{
    /// <summary>
    /// Repository interface cho TourGuideInvitation entity
    /// Quản lý các operations liên quan đến invitation workflow
    /// </summary>
    public interface ITourGuideInvitationRepository : IGenericRepository<TourGuideInvitation>
    {
        /// <summary>
        /// Lấy tất cả invitations cho một TourDetails cụ thể
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>Danh sách invitations với thông tin Guide</returns>
        Task<IEnumerable<TourGuideInvitation>> GetByTourDetailsAsync(Guid tourDetailsId);

        /// <summary>
        /// Lấy tất cả invitations cho một TourGuide cụ thể
        /// </summary>
        /// <param name="guideId">ID của TourGuide</param>
        /// <param name="status">Lọc theo status (optional)</param>
        /// <returns>Danh sách invitations với thông tin TourDetails</returns>
        Task<IEnumerable<TourGuideInvitation>> GetByGuideAsync(Guid guideId, InvitationStatus? status = null);

        /// <summary>
        /// Lấy tất cả invitations đang pending và chưa hết hạn
        /// </summary>
        /// <returns>Danh sách pending invitations</returns>
        Task<IEnumerable<TourGuideInvitation>> GetPendingInvitationsAsync();

        /// <summary>
        /// Lấy tất cả invitations đã hết hạn nhưng chưa được update status
        /// </summary>
        /// <returns>Danh sách expired invitations</returns>
        Task<IEnumerable<TourGuideInvitation>> GetExpiredInvitationsAsync();

        /// <summary>
        /// Expire tất cả pending invitations cho một TourDetails (khi có guide được chọn)
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="excludeInvitationId">ID của invitation được chấp nhận (không expire)</param>
        /// <returns>Số lượng invitations đã được expired</returns>
        Task<int> ExpireInvitationsForTourDetailsAsync(Guid tourDetailsId, Guid? excludeInvitationId = null);

        /// <summary>
        /// Kiểm tra xem TourGuide đã có invitation pending cho TourDetails chưa
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="guideId">ID của TourGuide</param>
        /// <returns>True nếu đã có invitation pending</returns>
        Task<bool> HasPendingInvitationAsync(Guid tourDetailsId, Guid guideId);

        /// <summary>
        /// Kiểm tra xem TourGuide đã từ chối lời mời cho TourDetails này trước đó chưa
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <param name="guideId">ID của TourGuide</param>
        /// <returns>True nếu đã có invitation bị reject</returns>
        Task<bool> HasRejectedInvitationAsync(Guid tourDetailsId, Guid guideId);

        /// <summary>
        /// Kiểm tra xem TourDetails đã có hướng dẫn viên chấp nhận lời mời chưa
        /// </summary>
        /// <param name="tourDetailsId">ID của TourDetails</param>
        /// <returns>True nếu đã có ít nhất một invitation được accept</returns>
        Task<bool> HasAcceptedInvitationAsync(Guid tourDetailsId);

        /// <summary>
        /// Lấy invitation với đầy đủ thông tin related entities
        /// </summary>
        /// <param name="id">ID của invitation</param>
        /// <returns>Invitation với TourDetails, Guide, CreatedBy information</returns>
        Task<TourGuideInvitation?> GetWithDetailsAsync(Guid id);
    }
}
